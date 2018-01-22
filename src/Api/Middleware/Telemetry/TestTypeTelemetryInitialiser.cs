using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Api.Middleware.Telemetry
{
    public class TestTypeTelemetryInitialiser : ITelemetryInitializer
    {
        private IHttpContextAccessor httpContextAccessor;

        public TestTypeTelemetryInitialiser(IHttpContextAccessor contextAccessor)
        {
            httpContextAccessor = contextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var testTypeHeader = httpContextAccessor.HttpContext.Request.Headers["Test-Type"];

                if (testTypeHeader.Count() > 0)
                {
                    if (!telemetry.Context.Properties.ContainsKey("Test-Type"))
                    {
                        telemetry.Context.Properties.Add("Test-Type", testTypeHeader);
                    }
                }
            }
        }
    }
}