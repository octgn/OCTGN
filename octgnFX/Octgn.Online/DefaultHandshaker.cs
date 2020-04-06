using Octgn.Communication;
using Octgn.Communication.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Online
{
    public class DefaultHandshaker : Module, IHandshaker
    {
        public string SessionKey { get; set; }

        public string UserId { get; set; }

        public string DeviceId { get; set; }


        public DefaultHandshaker() {

        }

        public async Task<HandshakeResult> Handshake(IConnection connection, CancellationToken cancellationToken) {
            if (!IsConfigured(out var invalidFields)) {
                return new HandshakeResult() {
                    Successful = false,
                    ErrorCode = $"{nameof(DefaultHandshaker)}: These fields are invalid: {string.Join(",", invalidFields)}"
                };
            }

            var req = new HandshakeRequestPacket("default") {
                [nameof(SessionKey)] = SessionKey,
                [nameof(UserId)] = UserId,
                [nameof(DeviceId)] = DeviceId
            };

            var result = await connection.Request(req, cancellationToken);

            if (result.Data == null) {
                return new HandshakeResult() {
                    Successful = false,
                    ErrorCode = $"{nameof(DefaultHandshaker)}: Invalid response data. Expecting {nameof(HandshakeResult)}, but no data received"
                };
            }

            if (result.Data is HandshakeResult handshakeResult) {
                return handshakeResult;
            }

            return new HandshakeResult() {
                Successful = false,
                ErrorCode = $"{nameof(DefaultHandshaker)}: Invalid response data. Expecting {nameof(HandshakeResult)}, received {result.Data.GetType().Name}"
            };
        }

        public Task<HandshakeResult> OnHandshakeRequest(HandshakeRequestPacket request, IConnection connection, CancellationToken cancellationToken) {
            if(request.HandshakeType != "default") {

            }


            throw new NotImplementedException();
        }

        public bool IsConfigured(out IEnumerable<string> invalidFields) {
            var iv = new HashSet<string>();

            invalidFields = iv;

            if (string.IsNullOrWhiteSpace(SessionKey)) {
                iv.Add(nameof(SessionKey));
            }

            if(SessionKey.Length > 128) {
                iv.Add(nameof(SessionKey));
            }

            if (string.IsNullOrWhiteSpace(UserId)) {
                iv.Add(nameof(UserId));
            }

            if(UserId.Length > 128) {
                iv.Add(nameof(UserId));
            }

            if (string.IsNullOrWhiteSpace(DeviceId)) {
                iv.Add(nameof(DeviceId));
            }

            if(DeviceId.Length > 128) {
                iv.Add(nameof(DeviceId));
            }


            return iv.Count == 0;
        }
    }
}
