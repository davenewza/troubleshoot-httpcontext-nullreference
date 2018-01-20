using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Api.Middleware
{
    public class NoCacheMiddleware
    {
        private readonly RequestDelegate next;

        public NoCacheMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Add("Pragma", "no-cache");
            context.Response.Headers.Add("Expires", "0");

            await next(context);
        }
    }
}