using Microsoft.AspNetCore.Builder;

namespace Api.Middleware.RateLimiting
{
    public static class CustomIpRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomIpRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomIpRateLimitMiddleware>();
        }
    }
}