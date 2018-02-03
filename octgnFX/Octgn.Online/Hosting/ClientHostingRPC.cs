using Octgn.Communication;
using Octgn.Communication.Packets;
using System;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public class ClientHostingRPC : IClientHostingRPC
    {
        private readonly Client _client;
        private readonly Version _octgnVersion;

        public ClientHostingRPC(Client client, Version octgnVersion) {
            _client = client;
            _octgnVersion = octgnVersion;
        }

        public async Task<HostedGame> HostGame(HostedGame game) {
            var packet = new RequestPacket(nameof(IClientHostingRPC.HostGame));
            game.OctgnVersion = _octgnVersion.ToString();

            HostedGame.AddToPacket(packet, game);

            var result = await _client.Connection.Request(packet);

            return result.As<HostedGame>();
        }
    }
}
