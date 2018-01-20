using Api.Extensions;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Middleware.RateLimiting
{
    public class CustomClientRateLimitMiddleware : ClientRateLimitMiddleware
    {
        private readonly string quotaExceededMessage;

        private readonly int httpStatusCode;

        private ILogger<ClientRateLimitMiddleware> Logger { get; }

        public CustomClientRateLimitMiddleware(
            RequestDelegate next,
            IOptions<ClientRateLimitOptions> options,
            IOptions<ClientRateLimitPolicies> clientPolicies,
            IRateLimitCounterStore counterStore,
            IClientPolicyStore policyStore,

            ILogger<ClientRateLimitMiddleware> logger)
                : base(next, options, counterStore, policyStore, logger)
        {
            if (String.IsNullOrWhiteSpace(options.Value.QuotaExceededMessage))
            {
                throw new InvalidOperationException("API quota exceeded message not set in the config.");
            }

            if (options.Value.HttpStatusCode == default(int))
            {
                throw new InvalidOperationException("API quota exceeded status code message not set in the config.");
            }

            Logger = logger;
            quotaExceededMessage = options.Value.QuotaExceededMessage;
            httpStatusCode = options.Value.HttpStatusCode;
        }

        public override ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            var extractedTenantIds = httpContext.GetTenantIds();

            // This is only for clients, so they should only have one tenant id and we therefore can take the First().
            var tenantId = extractedTenantIds.Count() > 0 ? extractedTenantIds.First().ToString() : "anon";

            return new ClientRequestIdentity
            {
                Path = httpContext.Request.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpContext.Request.Method.ToLowerInvariant(),
                ClientId = tenantId.ToLowerInvariant()
            };
        }

        //public override async Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        //{
        //    httpContext.Response.ContentType = "application/json";
        //    httpContext.Response.StatusCode = httpStatusCode;

        //    var serializerSettings = new JsonSerializerSettings()
        //    {
        //        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        //        NullValueHandling = NullValueHandling.Ignore
        //    };

        //    // Log the quota exceeded so that we can report back.
        //    var extractedTenantIds = httpContext.GetTenantIds();
        //    var tenantId = extractedTenantIds.Count() > 0 ? extractedTenantIds.First().ToString() : "anon";

        //    Task.Delay(50).Wait();

        //    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(quotaExceededMessage, serializerSettings), Encoding.UTF8);
        //}
    }
}