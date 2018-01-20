using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Middleware.Datastore
{
    public class Request
    {
        // Identifier
        public Guid RequestId { get; set; }

        // Environment
        public string Application { get; set; }

        public string Machine { get; set; }

        // User
        public string ClientId { get; set; }

        public string ClientTenant { get; set; }

        public string ClientCoverage { get; set; }

        public string RemoteIpAddress { get; set; }

        // Request
        public string RequestMethod { get; set; }

        public string RequestUri { get; set; }

        public string RequestQueryString { get; set; }

        public string RequestHeaders { get; set; }

        public string RequestContentBody { get; set; }

        public string RequestTimestamp { get; set; }

        // Response
        public int? ResponseStatusCode { get; set; }

        public string ResponseHeaders { get; set; }

        public string ResponseContentBody { get; set; }

        public string ResponseTimestamp { get; set; }

        public long CallDurationInMilliseconds { get; set; }
    }
}