/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Threading.Tasks;
using Octgn.Communication;
using System.Threading;
using Octgn.Communication.Packets;

namespace Octgn.Library.Communication
{

    public class ClientAuthenticator : Module
    {
        public string SessionKey { get; set; }
        public string UserId { get; set; }
        public string DeviceId { get; set; }

        public ClientAuthenticator() {

        }

        public override Task<ProcessResult> Process(object obj, CancellationToken cancellationToken = default) {
            if(!(obj is RequestPacket request)) return base.Process(obj, cancellationToken));



            return base.Process(obj, cancellationToken);
        }

        public async Task<AuthenticationResult> Authenticate(Octgn.Communication.Client client, IConnection connection, CancellationToken cancellationToken) {
            var req = new Octgn.Communication.Packets.AuthenticationRequestPacket("session");
            req["sessionKey"] = SessionKey;
            req["userId"] = UserId;
            req["deviceId"] = DeviceId;

            var result = await client.Request(req, cancellationToken);

            return result.As<AuthenticationResult>();
        }
    }
}
