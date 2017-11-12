using System.Threading.Tasks;
using Octgn.Communication;
using System;
using Octgn.Communication.Packets;
using Octgn.Communication.Serializers;

namespace Octgn.Online.Hosting
{
    public class ServerHostingModule : IServerModule
    {
        private readonly Server _server;
        private readonly string _gameServerUserId;

        public ServerHostingModule(Server server, string gameServerUserId) {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _gameServerUserId = gameServerUserId ?? throw new ArgumentNullException(nameof(gameServerUserId));

            _requestHandler.Register(nameof(IClientHostingRPC.HostGame), OnHostGame);
            _requestHandler.Register(nameof(IClientHostingRPC.SignalGameStarted), OnSignalGameStarted);

            if(_server.Serializer is XmlSerializer serializer) {
                serializer.Include(typeof(HostedGame));
            }
        }

        private Task<ResponsePacket> OnSignalGameStarted(RequestContext context, RequestPacket request) {
            return _server.Request(request, _gameServerUserId);
        }

        private Task<ResponsePacket> OnHostGame(RequestContext context, RequestPacket request) {
            return _server.Request(request, _gameServerUserId);
        }

        private readonly RequestHandler _requestHandler = new RequestHandler();

        public Task HandleRequest(object sender, RequestPacketReceivedEventArgs args) {
            return _requestHandler.HandleRequest(sender, args);
        }

        public Task UserStatucChanged(object sender, UserStatusChangedEventArgs e) {
            return Task.CompletedTask;
        }
    }
}