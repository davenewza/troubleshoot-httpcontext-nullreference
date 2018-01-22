using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Api.Middleware.Telemetry
{
    public class TokenClaimsTelemetryInitialiser : ITelemetryInitializer
    {
        private IHttpContextAccessor httpContextAccessor;

        public TokenClaimsTelemetryInitialiser(IHttpContextAccessor contextAccessor)
        {
            httpContextAccessor = contextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (httpContextAccessor.HttpContext != null)
            {
                var claims = httpContextAccessor.HttpContext.User.Claims;
                var clientId = claims.SingleOrDefault(x => x.Type.Equals("client_id", StringComparison.InvariantCultureIgnoreCase));
                if (clientId != null)
                {
                    telemetry.Context.User.AuthenticatedUserId = clientId.Value;
                }

                var clientTenantId = claims.SingleOrDefault(x => x.Type.Equals("client_tenant", StringComparison.InvariantCultureIgnoreCase));
                if (clientTenantId != null)
                {
                    telemetry.Context.User.AccountId = clientTenantId.Value;
                }
            }
        }
    }
}