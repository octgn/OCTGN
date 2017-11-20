using Octgn.Communication;
using System;
using System.Threading.Tasks;
using Octgn.Communication.Packets;
using Octgn.Site.Api;
using System.Threading;

namespace Octgn.Authenticators
{
    public class SessionAuthenticationHandler : IAuthenticationHandler
    {
        public async Task<AuthenticationResult> Authenticate(Server server, IConnection connection, AuthenticationRequestPacket packet, CancellationToken cancellationToken) {
            if (packet.AuthenticationType != "session")
                throw new InvalidOperationException($"This authentication handler is a '{packet.AuthenticationType}' authentication type, not a 'session' authentication type.");

            var sessionKey = (string)packet["sessionKey"];
            var userId = (string)packet["userId"];
            var deviceId = (string)packet["deviceId"];

            var client = new ApiClient();
            try {
                if (!await client.ValidateSession(userId, deviceId, sessionKey, cancellationToken)) {
                    return new AuthenticationResult {
                        ErrorCode = "SessionInvalid",
                        Successful = false
                    };
                }

                cancellationToken.ThrowIfCancellationRequested();

                var apiUser = await client.UserFromUserId(userId, cancellationToken);

                var user = new User(userId, apiUser.UserName);

                return new AuthenticationResult {
                    Successful = true,
                    User = user
                };
            } catch (ApiClientException) {
                return new AuthenticationResult {
                    ErrorCode = "ApiClientError",
                    Successful = false
                };
            }
        }
    }
}
