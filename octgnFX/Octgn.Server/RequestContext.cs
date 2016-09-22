using Microsoft.AspNet.SignalR.Hubs;
using Octgn.Server.Data;
using System;

namespace Octgn.Server
{
    public class RequestContext : IDisposable
    {
        public Player Sender { get; set; }
        public Game Game { get; set; }

        public IOctgnServerSettings Settings { get; private set; }

        private IGameRepository _gameRepo;

        public RequestContext(HubCallerContext hubContext, IGameRepository gameRepo, IOctgnServerSettings settings) {
            _gameRepo = gameRepo;
            var gameId = hubContext.Headers["gameid"];

            Game = new Game(_gameRepo.Checkout(int.Parse(gameId)));
            var ip = _gameRepo.Players.GetOrAdd(Game, hubContext.ConnectionId, hubContext.User.Identity.Name);
            Sender = new Player(Game, ip);
            Settings = settings;
        }

        public void Dispose() {
            _gameRepo.Checkin(Game);
        }
    }
}
