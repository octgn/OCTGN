using System;
using System.Net.Http;

namespace Octgn.Site.Api
{

    public class ApiClientException : Exception
    {
        public ApiClientException() : base() {
        }

        public ApiClientException(string message) : base(message) {
        }

        public ApiClientException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ApiClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        public static ApiClientException FromResponse(HttpResponseMessage message) {
            var ex = new ApiClientException($"Http request failed with code '{(int)message.StatusCode}'");
            return ex;
        }
    }
}