using Microsoft.AspNetCore.Builder;

namespace Api.Middleware.RateLimiting
{
    public static class CustomClientRateLimitMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomClientRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomClientRateLimitMiddleware>();
        }
    }
}