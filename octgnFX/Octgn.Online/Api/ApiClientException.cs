using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Octgn.Site.Api
{
    public class ApiClientException : Exception
    {
        public const int MaximumResponseBodyLength = 4096;

        /// <summary>The HTTP status, or null for exceptions created without a response.</summary>
        public HttpStatusCode? StatusCode { get; private set; }
        public HttpMethod RequestMethod { get; private set; }

        /// <summary>The request URI without user information, query string, or fragment.</summary>
        public Uri RequestUri { get; private set; }

        /// <summary>A bounded response excerpt. Treat this as potentially sensitive server data.</summary>
        public string ResponseBody { get; private set; }
        public bool ResponseBodyTruncated { get; private set; }

        /// <summary>An optional body-read failure. May contain sensitive server data; excluded from exception logs.</summary>
        public Exception ResponseBodyReadException { get; private set; }

        public ApiClientException() : base() {
        }

        public ApiClientException(string message) : base(message) {
        }

        public ApiClientException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ApiClientException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) {
        }

        /// <summary>Creates response diagnostics synchronously. Prefer the async factory in async callers.</summary>
        public static ApiClientException FromResponse(HttpResponseMessage message) {
            if (message == null) throw new ArgumentNullException(nameof(message));

            // Start on the thread pool so custom HttpContent cannot capture a blocked caller's context.
            return Task.Run(() => FromResponseAsync(message)).GetAwaiter().GetResult();
        }

        /// <summary>Creates safe request diagnostics and a bounded, opt-in response excerpt.</summary>
        public static async Task<ApiClientException> FromResponseAsync(HttpResponseMessage message) {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var request = message.RequestMessage;
            var requestUri = SanitizeUri(request?.RequestUri);
            var description = $"Http request failed with code '{(int)message.StatusCode}' ({message.StatusCode})";
            if (request != null) description += $" for {request.Method}";
            if (requestUri != null) description += $" {requestUri}";

            var body = string.Empty;
            var truncated = false;
            Exception readError = null;
            if (message.Content != null) {
                try {
                    var stream = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var encoding = Encoding.UTF8;
                    var charset = message.Content.Headers.ContentType?.CharSet;
                    if (!string.IsNullOrEmpty(charset)) encoding = Encoding.GetEncoding(charset.Trim('"'));

                    using (var reader = new StreamReader(stream, encoding, true, 1024, leaveOpen: true)) {
                        var buffer = new char[MaximumResponseBodyLength + 1];
                        var count = 0;
                        while (count < buffer.Length) {
                            var read = await reader.ReadAsync(buffer, count, buffer.Length - count).ConfigureAwait(false);
                            if (read == 0) break;
                            count += read;
                        }
                        truncated = count > MaximumResponseBodyLength;
                        body = new string(buffer, 0, Math.Min(count, MaximumResponseBodyLength));
                    }
                } catch (Exception ex) {
                    // A body read failure must not replace the original HTTP status failure.
                    readError = ex;
                    description += " (response body unavailable)";
                }
            }

            // Keep server bodies out of Message: exception logs may otherwise expose echoed credentials.
            return new ApiClientException(description) {
                StatusCode = message.StatusCode,
                RequestMethod = request?.Method,
                RequestUri = requestUri,
                ResponseBody = body,
                ResponseBodyTruncated = truncated,
                ResponseBodyReadException = readError
            };
        }

        private static Uri SanitizeUri(Uri uri) {
            if (uri == null) return null;
            if (uri.IsAbsoluteUri) {
                return new Uri(uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped));
            }
            if (uri.OriginalString.StartsWith("//", StringComparison.Ordinal)) {
                var absoluteUri = SanitizeUri(new Uri("http:" + uri.OriginalString));
                return new Uri(absoluteUri.GetComponents(UriComponents.HostAndPort | UriComponents.Path, UriFormat.UriEscaped).Insert(0, "//"), UriKind.Relative);
            }
            return new Uri(uri.OriginalString.Split('?', '#')[0], UriKind.Relative);
        }
    }
}
