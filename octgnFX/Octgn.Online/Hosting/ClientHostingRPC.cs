using Octgn.Communication;
using Octgn.Communication.Packets;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public class ClientHostingRPC : IClientHostingRPC
    {
        private readonly Client _client;

        public ClientHostingRPC(Client client) {
            _client = client;
        }

        public async Task<IHostedGame> HostGame(HostGameRequest request) {
            var packet = new RequestPacket(nameof(IClientHostingRPC.HostGame));
            HostGameRequest.AddToPacket(packet, request);

            var result = await _client.Connection.Request(packet);

            return result.As<HostedGame>();
        }

        public Task SignalGameStarted(string gameId) {
            var packet = new RequestPacket(nameof(IClientHostingRPC.SignalGameStarted));
            HostedGame.AddIdToPacket(packet, gameId);

            return _client.Connection.Request(packet);
        }
    }
}
