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
            var uid = hubContext.Headers["uid"];

            Game = new Game(_gameRepo.Checkout(int.Parse(gameId)));
            Sender = new Player(Game, _gameRepo.Players.Get(ulong.Parse(uid)));
            Settings = settings;
        }

        public void Dispose() {
            _gameRepo.Checkin(Game);
        }
    }
}
