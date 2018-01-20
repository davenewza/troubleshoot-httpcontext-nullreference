using Api.Middleware;
using Api.Middleware.Compression;
using Api.Middleware.Datastore;
using Api.Middleware.RateLimiting;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using System.Net;
using System.Text;

namespace Api
{
    public class Startup
    {
        private object environment;

        private IConfigurationRoot Configuration;

        public Startup(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("config.json", optional: false);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services
               .AddOptions()
               .Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"))
               .Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"))
               .Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"))
               .Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));

            services.AddMemoryCache();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // In Test and Prod we do not want to display any exception information
            app.UseExceptionHandler(options =>
            {
                options.Run(
                async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("Internal Server Error", Encoding.UTF8);
                });
            });

            //app.UseRequireHttps(!env.IsDevelopment());

            app.UseMiddleware<NoCacheMiddleware>();

            // Use rate limiting
            app.UseCustomIpRateLimiting();
            app.UseCustomClientRateLimiting();

            // Use CORS policy for all requests.
            // app.UseCors("CorsPolicy");

            app.UseCompression();

            // Add requests to CRIT datastore
            app.UseDatastore();

            // Use MVC.
            app.UseMvc();

            // Intercept 404s.
            app.Use(async (context, next) =>
            {
                await next();
                if (context.Response.StatusCode == 404)
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("Not Found", Encoding.UTF8);
                }
            });
        }
    }
}