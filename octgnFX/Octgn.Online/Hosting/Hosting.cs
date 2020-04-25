using Octgn.Communication;
using Octgn.Communication.Packets;
using Octgn.Communication.Serializers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Online.Hosting
{
    public class Hosting : Module
    {
        private readonly Server _server;
        private readonly Client _client;
        private readonly string _gameServerUserId;
        private readonly RequestHandler _requestHandler = new RequestHandler();

        public IClientHostingRPC ClientRPC { get; private set; }

        public Hosting(Server server, string gameServerUserId) {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _gameServerUserId = gameServerUserId ?? throw new ArgumentNullException(nameof(gameServerUserId));

            if (_server.Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }
        }

        public Hosting(Client client, Version octgnVersion) {
            _client = client ?? throw new ArgumentNullException(nameof(client));

            if (_client.Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }

            ClientRPC = new ClientHostingRPC(_client, octgnVersion);
        }

        public override Task<ProcessResult> Process(object obj, CancellationToken cancellationToken = default(CancellationToken)) {
            if (!(obj is RequestPacket request)) return base.Process(obj, cancellationToken);

            if (_server != null) {
                switch (request.Name) {
                    case nameof(IClientHostingRPC.HostGame): {
                            return OnHostGameRequest(request);
                        }
                }
            }

            return base.Process(obj, cancellationToken);
        }

        private async Task<ProcessResult> OnHostGameRequest(RequestPacket request) {
            var sendRequest = new RequestPacket(request);

            var gsResp = await _server.Request(sendRequest, _gameServerUserId);

            return new ProcessResult(gsResp.Data);
        }
    }
}
