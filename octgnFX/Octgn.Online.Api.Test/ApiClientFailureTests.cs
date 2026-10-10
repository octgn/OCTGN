using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Octgn.Online.Hosting;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

namespace Octgn.Online.Api.Test
{
    [TestFixture, NonParallelizable, Timeout(15000)]
    public class ApiClientFailureTests
    {
        private TcpListener listener;
        private Task server;
        private ApiClient client;
        private volatile int statusCode = 502;
        private const string Body = "{\"error\":\"upstream unavailable\",\"accessKey\":\"body-secret\"}";

        [OneTimeSetUp]
        public void StartServer()
        {
            listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            ApiClient.DefaultUrl = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");
            client = new ApiClient();
            server = Serve();
        }

        [OneTimeTearDown]
        public async Task StopServer()
        {
            listener.Stop();
            try { await server; }
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

#if !BASELINE
        [TestCase("UsersFromUserIds", "POST", "/api/user/fromuserids")]
        [TestCase("UserFromUserId", "POST", "/api/user/fromuserids")]
        [TestCase("CreateSession", "POST", "/api/sessions")]
        [TestCase("ClearSession", "POST", "/api/users/user/devices/device/session")]
        [TestCase("ValidateSession", "PUT", "/api/users/user/devices/device/session/validate")]
        [TestCase("IsGameServerRunning", "GET", "/api/servicestatus/gameserverrunning")]
        [TestCase("SetGameList", "PUT", "/api/game")]
        [TestCase("GetGameList", "GET", "/api/game")]
        [TestCase("GetUsersSharedDecks", "GET", "/api/shareddeck/user")]
        [TestCase("WebhookQueuePublish", "POST", "/Api/WebhookQueue/Octgn")]
        public async Task AllExistingFailurePathsUseStructuredDiagnostics(string operation, string method, string path)
        {
            statusCode = 502;
            var exception = (ApiClientException)await CaptureFailure(operation);
            Assert.Multiple(() => {
                Assert.That(exception.StatusCode, Is.EqualTo(HttpStatusCode.BadGateway));
                Assert.That(exception.RequestMethod.Method, Is.EqualTo(method));
                Assert.That(exception.RequestUri.AbsolutePath, Is.EqualTo(path));
                Assert.That(exception.RequestUri.Query, Is.Empty);
                Assert.That(exception.ResponseBody, Is.EqualTo(Body));
                Assert.That(exception.Message, Does.Contain("502").And.Contain(method).And.Contain(path));
                Assert.That(exception.ToString(), Does.Not.Contain("secret").And.Not.Contain("{resp.StatusCode}").And.Not.Contain("{content}"));
            });
        }
#endif

        [TestCase("GetUsersSharedDecks")]
        [TestCase("WebhookQueuePublish")]
        public async Task FailureMessageContainsActualStatusInsteadOfInterpolationPlaceholders(string operation)
        {
            statusCode = 502;
            var exception = await CaptureFailure(operation);
            Assert.That(exception.Message, Does.Contain("502"));
            Assert.That(exception.Message, Does.Not.Contain("{resp.StatusCode}").And.Not.Contain("{content}"));
        }

        [Test]
        public async Task ValidateSessionNotFoundRemainsFalse()
        {
            statusCode = 404;
            Assert.That(await client.ValidateSession("user", "device", "session-secret"), Is.False);
        }

        [Test]
        public async Task IncompleteOwnedRequestDoesNotStopFailureFixture()
        {
            using (var malformed = new TcpClient()) {
                await malformed.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
                var partial = Encoding.ASCII.GetBytes("GET /incomplete HTTP/1.1\r\n");
                await malformed.GetStream().WriteAsync(partial, 0, partial.Length);
            }
            statusCode = 502;
            Assert.That(await CaptureFailure("GetGameList"), Is.InstanceOf<ApiClientException>());
            Assert.That(server.IsFaulted, Is.False);
        }

        private async Task<Exception> CaptureFailure(string operation)
        {
            try {
                switch (operation) {
                    case "UsersFromUserIds": await client.UsersFromUserIds(new[] { "user" }); break;
                    case "UserFromUserId": await client.UserFromUserId("user"); break;
                    case "CreateSession": await client.CreateSession("user", "password-secret", "device"); break;
                    case "ClearSession": await client.ClearSession("user", "device", "session-secret"); break;
                    case "ValidateSession": await client.ValidateSession("user", "device", "session-secret"); break;
                    case "IsGameServerRunning": await client.IsGameServerRunning("user", "password-secret"); break;
                    case "SetGameList": await client.SetGameList("api-secret", Array.Empty<HostedGame>()); break;
                    case "GetGameList": await client.GetGameList(); break;
                    case "GetUsersSharedDecks": client.GetUsersSharedDecks("user"); break;
                    case "WebhookQueuePublish": client.WebhookQueuePublish(WebhookEndpoint.Octgn, "message", "access-secret"); break;
                    default: throw new ArgumentException(operation);
                }
            } catch (ApiClientException ex) { return ex; }
            Assert.Fail("Expected an ApiClientException");
            return null;
        }

        private async Task Serve()
        {
            while (true) {
                using var connection = await listener.AcceptTcpClientAsync();
                try {
                    using var stream = connection.GetStream();
                    var headers = new StringBuilder();
                    var buffer = new byte[1];
                    while (!headers.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal)) {
                        if (await stream.ReadAsync(buffer, 0, 1) == 0) throw new IOException("Request ended before headers");
                        headers.Append((char)buffer[0]);
                    }
                    var headerLines = headers.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
                    var contentLength = headerLines.FirstOrDefault(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
                    if (contentLength != null) {
                        var remaining = int.Parse(contentLength.Split(':')[1].Trim());
                        var bodyBuffer = new byte[1024];
                        while (remaining > 0) {
                            var read = await stream.ReadAsync(bodyBuffer, 0, Math.Min(bodyBuffer.Length, remaining));
                            if (read == 0) throw new IOException("Request ended before content");
                            remaining -= read;
                        }
                    } else if (headerLines.Any(line => line.Equals("Transfer-Encoding: chunked", StringComparison.OrdinalIgnoreCase))) {
                        while (true) {
                            var chunkSize = int.Parse(await ReadLine(stream), System.Globalization.NumberStyles.HexNumber);
                            for (var i = 0; i < chunkSize; i++) {
                                if (await stream.ReadAsync(buffer, 0, 1) == 0) throw new IOException("Incomplete request chunk");
                            }
                            await ReadLine(stream);
                            if (chunkSize == 0) break;
                        }
                    }
                    var bytes = Encoding.UTF8.GetBytes(Body);
                    var responseHeaders = Encoding.ASCII.GetBytes($"HTTP/1.1 {statusCode} Test failure\r\nContent-Type: application/json\r\nContent-Length: {bytes.Length}\r\nConnection: close\r\n\r\n");
                    await stream.WriteAsync(responseHeaders, 0, responseHeaders.Length);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    await stream.FlushAsync();
                } catch (IOException) {
                    // A closed or incomplete owned request must not terminate the accept loop.
                } catch (FormatException) {
                    // A malformed length belongs to this connection, not subsequent tests.
                }
            }
        }

        private static async Task<string> ReadLine(Stream stream)
        {
            var line = new StringBuilder();
            var buffer = new byte[1];
            while (true) {
                if (await stream.ReadAsync(buffer, 0, 1) == 0) throw new IOException("Incomplete request line");
                if (buffer[0] == '\n') return line.ToString().TrimEnd('\r');
                line.Append((char)buffer[0]);
            }
        }
    }
}
