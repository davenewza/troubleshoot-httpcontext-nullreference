using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Api.Middleware.Telemetry
{
    public class RequestNameTelemetryInitialiser : ITelemetryInitializer
    {
        private IHttpContextAccessor httpContextAccessor;

        public RequestNameTelemetryInitialiser(IHttpContextAccessor contextAccessor)
        {
            httpContextAccessor = contextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            var requestTelemetry = telemetry as RequestTelemetry;

            if (requestTelemetry != null)
            {
                requestTelemetry.Name = FormatRequestName(
                    httpContextAccessor.HttpContext.Request.Method,
                    httpContextAccessor.HttpContext.Request.Path.Value);
            }
        }

        public string FormatRequestName(string method, string path)
        {
            if (!String.IsNullOrWhiteSpace(path) && path.First() != '/')
            {
                path = path.Insert(0, "/");
            }

            var operationName = $"{method.Trim().ToUpperInvariant()} {path.Trim().ToLowerInvariant()}";

            Regex squidRegex = new Regex(@"(\{){0,1}\/[0-9a-zA-Z\-_]{22}(\}){0,1}");
            operationName = squidRegex.Replace(operationName, "/{id}");

            Regex dateRegex = new Regex(@"(\{){0,1}when\/\d{4}-\d{2}-\d{2}(\}){0,1}");
            operationName = dateRegex.Replace(operationName, "when/{date}");

            Regex pagesRegex = new Regex(@"(\{){0,1}page\/(\d+)(\}){0,1}");
            operationName = pagesRegex.Replace(operationName, "page/{index}");

            return operationName.TrimEnd('/', '\\').Trim();
        }
    }
}