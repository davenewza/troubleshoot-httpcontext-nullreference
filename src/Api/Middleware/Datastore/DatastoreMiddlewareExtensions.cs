using Microsoft.AspNetCore.Builder;

namespace Api.Middleware.Datastore
{
    public static class DatastoreMiddlewareExtensions
    {
        public static IApplicationBuilder UseDatastore(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DatastoreMiddleware>();
        }
    }
}