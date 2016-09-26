/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Microsoft.AspNet.SignalR.Hubs;
using Octgn.Library.Utils;
using Octgn.Online.Library.Models;
using Octgn.Server.Data;
using System;
using System.Threading.Tasks;

namespace Octgn.Server
{
    public class RequestContext : IDisposable
    {
        public Player Sender { get; set; }
        public Game Game { get; set; }

        public IOctgnServerSettings Settings { get; private set; }
        public IClientCalls Broadcaster { get; private set; }

        private IGameRepository _gameRepo;
        private LibraryItem<IHostedGameState> _checkout;


        public RequestContext(IGameRepository gameRepo, IOctgnServerSettings settings, IClientCalls broadcaster) {
            _gameRepo = gameRepo;
            Broadcaster = broadcaster;

            Settings = settings;
        }

        public async Task Initialize(HubCallerContext hubContext) {
            var gameId = hubContext.Headers["gameid"];
            _checkout = await _gameRepo.Checkout(int.Parse(gameId));
            Game = new Game(_checkout.Item);
            var ip = _gameRepo.Players.GetOrAdd(Game, hubContext.ConnectionId, hubContext.User.Identity.Name);
            Sender = new Player(Game, ip, Broadcaster, Settings);
        }

        public void Dispose() {
            _checkout?.Dispose();
        }
    }
}
