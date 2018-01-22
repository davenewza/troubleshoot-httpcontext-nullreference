using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Api.Middleware.Telemetry
{
    public class RequestIdTelemetryInitialiser : ITelemetryInitializer
    {
        private IHttpContextAccessor httpContextAccessor;

        public RequestIdTelemetryInitialiser(IHttpContextAccessor contextAccessor)
        {
            httpContextAccessor = contextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;

            if (requestTelemetry != null)
            {
                if (httpContextAccessor.HttpContext.Request.Headers.TryGetValue("requestId", out StringValues requestId))
                {
                    requestTelemetry.Context.Properties["requestId"] = requestId;
                }
            }
        }
    }
}