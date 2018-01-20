using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Middleware.Datastore
{
    public class DatastoreMiddleware
    {
        private readonly RequestDelegate next;

        private ILogger Logger { get; set; }

        public DatastoreMiddleware(RequestDelegate next, ILogger<DatastoreMiddleware> logger)
        {
            this.next = next;
            Logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Create a new request identifier. This will be added to the response headers and telemetry.
            var requestId = Guid.NewGuid();
            var requestInstant = SystemClock.Instance.GetCurrentInstant();

            context.Request.Headers.Add("requestId", requestId.ToString());

            // Callback to add the requestId to the response when it starts.
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;
                httpContext.Response.Headers.Add("requestId", requestId.ToString());
                return Task.CompletedTask;
            }, context);

            var originalRequestBodyStream = context.Request.Body;
            var originalResponseBodyStream = context.Response.Body;

            using (var requestBodyStream = new MemoryStream())
            using (var responseBodyStream = new MemoryStream())
            using (var requestReader = new StreamReader(requestBodyStream))
            using (var responseReader = new StreamReader(responseBodyStream))
            {
                string requestBodyText = String.Empty;
                string responseBodyText = String.Empty;

                // Inbound (i.e. request portion)
                //Swap out both request and response bodies with memoryStreams, which are rewindable.
                try
                {
                    //Since the request already contains information, we copy it out now.
                    await context.Request.Body.CopyToAsync(requestBodyStream);
                    requestBodyStream.Seek(0, SeekOrigin.Begin);
                    requestBodyText = requestReader.ReadToEnd();
                    requestBodyStream.Seek(0, SeekOrigin.Begin);

                    context.Request.Body = requestBodyStream;
                    context.Response.Body = responseBodyStream;
                }
                catch (Exception ex)
                {
                    Logger.LogError(default(EventId), ex, $"DatastoreMiddleware failed on inbound. RequestId: {requestId}. {ex.Message}");
                }

                /***No*Response***/
                await next(context);
                /***Has*Response**/

                try
                {
                    //Copy the response info out now.
                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    responseBodyText = responseReader.ReadToEnd();

                    responseBodyStream.Seek(0, SeekOrigin.Begin);
                    await responseBodyStream.CopyToAsync(originalResponseBodyStream);
                }
                catch (Exception ex)
                {
                    Logger.LogError(default(EventId), ex, $"DatastoreMiddleware failed on outbound. RequestId: {requestId}. {ex.Message}");
                }
                finally
                {
                    //Reset the streams to the original non-memory streams, and dispose memory streams.
                    context.Request.Body = originalRequestBodyStream;
                    context.Response.Body = originalResponseBodyStream;
                    requestBodyStream.Dispose();
                    responseBodyStream.Dispose();
                }

                try
                {
                    //Save data we have to datastore.
                    var request = Create(context, requestId, requestInstant, requestBodyText, responseBodyText);
                    Task.Delay(100).Wait(); // do Something
                }
                catch (Exception ex)
                {
                    Logger.LogError(default(EventId), ex, $"DatastoreMiddleware failed to write to store. RequestId: {requestId}. {ex.Message}");
                }
            }

            return;
        }

        private Request Create(HttpContext context, Guid requestId, Instant requestInstant, string requestBodyText, string responseBodyText)
        {
            var responseInstant = SystemClock.Instance.GetCurrentInstant();
            var callDurationInMilliseconds = (responseInstant.ToUnixTimeMilliseconds() - requestInstant.ToUnixTimeMilliseconds());

            return new Request()
            {
                // Identifier
                RequestId = requestId,

                // Environment
                Application = "Troubleshoting",
                Machine = Environment.MachineName,

                // User
                ClientId = GetClaim(context, "client_id"),
                ClientTenant = GetClaim(context, "client_tenant"),
                ClientCoverage = GetClaim(context, "client_coverage"),
                RemoteIpAddress = context.Connection.RemoteIpAddress.ToString(),

                // Request
                RequestMethod = context.Request.Method,
                RequestUri = context.Request.Path,
                RequestQueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value.ToString() : null,
                RequestHeaders = SerialiseHeaders(context.Request.Headers),
                RequestContentBody = requestBodyText,
                RequestTimestamp = requestInstant.ToString(),

                // Response
                ResponseStatusCode = (int)context.Response.StatusCode,
                ResponseHeaders = SerialiseHeaders(context.Response.Headers),
                ResponseContentBody = responseBodyText,
                ResponseTimestamp = responseInstant.ToString(),
                CallDurationInMilliseconds = callDurationInMilliseconds
            };
        }

        private string GetClaim(HttpContext context, string claimName)
        {
            if (context == null || context.User == null || context.User.Claims == null || !context.User.Claims.Any())
            {
                return null;
            }

            var claim = context.User.Claims.SingleOrDefault(x => x.Type.Equals(claimName, StringComparison.InvariantCultureIgnoreCase));
            if (claim == null)
            {
                return null;
            }

            return claim.Value;
        }

        private string SerialiseHeaders(IHeaderDictionary headers)
        {
            return String.Concat(headers.Select(x => $"{x.Key}:{x.Value};"));
        }
    }
}