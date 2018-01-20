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
    public class CustomIpRateLimitMiddleware : IpRateLimitMiddleware
    {
        private readonly IIpAddressParser ipAddressParser;

        private readonly string quotaExceededMessage;

        private readonly int httpStatusCode;

        public CustomIpRateLimitMiddleware(RequestDelegate next, IOptions<IpRateLimitOptions> options, IRateLimitCounterStore counterStore, IIpPolicyStore policyStore, ILogger<IpRateLimitMiddleware> logger, IIpAddressParser ipParser = null)
            : base(next, options, counterStore, policyStore, logger, ipParser)
        {
            if (String.IsNullOrWhiteSpace(options.Value.QuotaExceededMessage))
            {
                throw new InvalidOperationException("API quota exceeded message not set in the config.");
            }

            if (options.Value.HttpStatusCode == default(int))
            {
                throw new InvalidOperationException("API quota exceeded status code message not set in the config.");
            }

            if (String.IsNullOrWhiteSpace(options.Value.RealIpHeader))
            {
                throw new InvalidOperationException("API real IP header value not set in the config.");
            }

            quotaExceededMessage = options.Value.QuotaExceededMessage;
            httpStatusCode = options.Value.HttpStatusCode;
            ipAddressParser = ipParser != null ? ipParser : new ReversProxyIpParser(options.Value.RealIpHeader);
        }

        public override ClientRequestIdentity SetIdentity(HttpContext httpContext)
        {
            var extractedTenantIds = httpContext.GetTenantIds();
            var tenantId = extractedTenantIds.Count() > 0 ? extractedTenantIds.First().ToString() : "anon";

            var clientIp = String.Empty;

            try
            {
                var ip = ipAddressParser.GetClientIp(httpContext);

                if (ip == null)
                {
                    throw new Exception("IpRateLimitMiddleware can't parse caller IP");
                }

                clientIp = ip.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("IpRateLimitMiddleware can't parse caller IP", ex);
            }

            return new ClientRequestIdentity
            {
                ClientIp = clientIp,
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

        //    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(RateLimitModel.Create(quotaExceededMessage), serializerSettings), Encoding.UTF8);
        //}
    }
}