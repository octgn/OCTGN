using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Octgn.Site.Api;

namespace Octgn.Online.Api.Test
{
    [TestFixture]
    public class ApiClientExceptionTests
    {
        [Test]
        public void SyncFactoryPreservesResponseBody()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadGateway) {
                Content = new StringContent("{\"error\":\"upstream unavailable\"}")
            };
            var exception = ApiClientException.FromResponse(response);
            // Reflection allows this exact regression to run against the pre-fix production source.
            var bodyProperty = typeof(ApiClientException).GetProperty("ResponseBody");
            Assert.That(bodyProperty, Is.Not.Null, "HTTP response details must be available to callers");
            Assert.That(bodyProperty.GetValue(exception), Is.EqualTo("{\"error\":\"upstream unavailable\"}"));
        }

#if !BASELINE
        [TestCase("{\"error\":\"invalid session\",\"SessionKey\":\"body-secret\"}", "application/json")]
        [TestCase("<html><body>Upstream failed: body-secret</body></html>", "text/html")]
        public async Task PreservesBodyAndSafeRequestContext(string body, string mediaType)
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadGateway) {
                Content = new StringContent(body, Encoding.UTF8, mediaType),
                RequestMessage = new HttpRequestMessage(HttpMethod.Post,
                    "https://user-secret:password-secret@example.com:8443/api/sessions?accessKey=query-secret#fragment-secret")
            };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.Multiple(() => {
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
                Assert.That(exception.RequestMethod, Is.EqualTo(HttpMethod.Post));
                Assert.That(exception.RequestUri.AbsoluteUri, Is.EqualTo("https://example.com:8443/api/sessions"));
                Assert.That(exception.ResponseBody, Is.EqualTo(body));
                Assert.That(exception.ResponseBodyTruncated, Is.False);
                Assert.That(exception.Message, Does.Contain("502").And.Contain("POST").And.Contain("/api/sessions"));
                Assert.That(exception.ToString(), Does.Not.Contain("secret").And.Not.Contain(body));
            });
        }

        [Test]
        public async Task InvalidCharsetFallsBackToUtf8()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadGateway) {
                Content = new ByteArrayContent(Encoding.UTF8.GetBytes("\u00e9chec"))
            };
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain") { CharSet = "not-a-real-charset" };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.That(exception.ResponseBody, Is.EqualTo("\u00e9chec"));
            Assert.That(exception.ResponseBodyReadException, Is.Null);
        }

        [TestCase(12)]
        [TestCase(10000)]
        public async Task SeekableResponseBodyRemainsReadableFromItsOriginalPosition(int length)
        {
            var body = new string('x', length);
            using var response = new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = new StringContent(body) };
            var stream = await response.Content.ReadAsStreamAsync();
            var position = stream.Position;
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.That(exception.ResponseBody, Is.EqualTo(body.Substring(0, Math.Min(length, 4096))));
            Assert.That(stream.Position, Is.EqualTo(position));
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
            Assert.That(await reader.ReadToEndAsync(), Is.EqualTo(body));
        }

        [Test]
        public async Task MalformedRelativeAuthorityDoesNotReplaceHttpFailure()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadGateway) {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("///x?secret", UriKind.Relative))
            };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
            Assert.That(exception.RequestUri, Is.Null);
            Assert.That(exception.ToString(), Does.Not.Contain("secret"));
        }

        [Test]
        public async Task HandlesMissingContentAndRequest()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.Content = null;
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.Multiple(() => {
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(exception.RequestUri, Is.Null);
                Assert.That(exception.RequestMethod, Is.Null);
                Assert.That(exception.ResponseBody, Is.Empty);
                Assert.That(exception.ResponseBodyTruncated, Is.False);
                Assert.That(exception.ResponseBodyReadException, Is.Null);
            });
        }

        [TestCase(0, false)]
        [TestCase(4096, false)]
        [TestCase(4097, true)]
        [TestCase(1000000, true)]
        public async Task LimitsResponseExcerpt(int bodyLength, bool truncated)
        {
            using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) {
                Content = new StringContent(new string('x', bodyLength))
            };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.That(exception.ResponseBody.Length, Is.EqualTo(Math.Min(bodyLength, 4096)));
            Assert.That(exception.ResponseBodyTruncated, Is.EqualTo(truncated));
        }

        [Test]
        public async Task DoesNotReadAnUnboundedStreamToCompletion()
        {
            using var stream = new EndlessStream();
            using var response = new HttpResponseMessage(HttpStatusCode.BadGateway) {
                Content = new StreamContent(stream)
            };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.That(exception.ResponseBody.Length, Is.EqualTo(4096));
            Assert.That(exception.ResponseBodyTruncated, Is.True);
            Assert.That(stream.BytesRead, Is.LessThanOrEqualTo(8192));
            Assert.That(stream.CanRead, Is.True, "The response owns its stream");
        }

        [Test]
        public async Task DecodesDeclaredCharset()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
                Content = new StringContent("\u00e9chec", Encoding.Unicode)
            };
            Assert.That((await ApiClientException.FromResponseAsync(response)).ResponseBody, Is.EqualTo("\u00e9chec"));
        }

        [Test]
        public async Task HandlesRelativeUriWithoutQuery()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("api/game?accessKey=secret#secret", UriKind.Relative))
            };
            Assert.That((await ApiClientException.FromResponseAsync(response)).RequestUri.ToString(), Is.EqualTo("api/game"));
        }

        [Test]
        public async Task HandlesRelativeAuthorityWithoutCredentials()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("//user-secret:password-secret@example.com/api/game?accessKey=query-secret", UriKind.Relative))
            };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.That(exception.RequestUri.ToString(), Does.StartWith("//example.com").And.EndWith("/api/game"));
            Assert.That(exception.ToString(), Does.Not.Contain("secret"));
        }

        [Test]
        public async Task RetainsStatusWhenBodyReadFailsAsynchronously()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) {
                Content = new DeferredContent(true),
                RequestMessage = new HttpRequestMessage(HttpMethod.Put, "https://example.com/api/game?password=secret")
            };
            var exception = await ApiClientException.FromResponseAsync(response);
            Assert.Multiple(() => {
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
                Assert.That(exception.RequestUri.AbsolutePath, Is.EqualTo("/api/game"));
                Assert.That(exception.ResponseBodyReadException, Is.TypeOf<InvalidOperationException>());
                Assert.That(exception.ToString(), Does.Contain("response body unavailable").And.Not.Contain("secret"));
            });
        }

        [Test]
        public void SyncFactoryDoesNotCaptureBlockedSynchronizationContext()
        {
            using var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new DeferredContent(false) };
            var completed = new ManualResetEventSlim();
            ApiClientException exception = null;
            Exception failure = null;
            var thread = new Thread(() => {
                SynchronizationContext.SetSynchronizationContext(new UnpumpedContext());
                try { exception = ApiClientException.FromResponse(response); }
                catch (Exception ex) { failure = ex; }
                finally { completed.Set(); }
            }) { IsBackground = true };
            thread.Start();
            Assert.That(completed.Wait(TimeSpan.FromSeconds(5)), Is.True, "Factory deadlocked on the caller's context");
            Assert.That(failure, Is.Null);
            Assert.That(exception.ResponseBody, Is.EqualTo("deferred error"));
        }

        [Test]
        public void RejectsNullResponse()
        {
            Assert.Throws<ArgumentNullException>(() => ApiClientException.FromResponse(null));
            Assert.ThrowsAsync<ArgumentNullException>(async () => await ApiClientException.FromResponseAsync(null));
        }

        private sealed class UnpumpedContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback callback, object state) { }
        }

        private sealed class DeferredContent : HttpContent
        {
            private readonly bool fail;
            public DeferredContent(bool fail) { this.fail = fail; }
            protected override bool TryComputeLength(out long length) { length = 0; return false; }
            protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                await Task.Delay(10);
                if (fail) throw new InvalidOperationException("read failed with body-secret");
                var bytes = Encoding.UTF8.GetBytes("deferred error");
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }

        private sealed class EndlessStream : Stream
        {
            public long BytesRead { get; private set; }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => BytesRead; set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count)
            {
                for (var i = offset; i < offset + count; i++) buffer[i] = (byte)'x';
                BytesRead += count;
                return count;
            }
            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
                => Task.FromResult(Read(buffer, offset, count));
            public override void Flush() => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
#endif
    }
}
