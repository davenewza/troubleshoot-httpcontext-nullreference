using Microsoft.AspNetCore.Builder;

namespace Api.Middleware.Compression
{
    public static class CompressionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCompression(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CompressionMiddleware>();
        }
    }
}