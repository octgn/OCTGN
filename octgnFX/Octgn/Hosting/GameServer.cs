/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Microsoft.Owin.Hosting;
using System;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.AspNet.SignalR;
using Octgn.Server.Signalr;
using Octgn.Server;
using Octgn.Online.Library.Models;
using Microsoft.AspNet.SignalR.Client;

namespace Octgn.Hosting
{
    public class GameServer : IDisposable
    {
        static GameServer() {
            _gameRepo = new ServerGameRepository();
            _requestHandler = new RequestHandler();
            GlobalHost.DependencyResolver
                .Register(typeof(GameHub), () => {
                    var settings = new OctgnServerSettings {
                        IsLocalGame = true
                    };
                    var ret = new GameHub(_requestHandler, _gameRepo, settings);
                    return ret;
                });
        }

        private static readonly ServerGameRepository _gameRepo;
        private static readonly RequestHandler _requestHandler;

        private IDisposable _webApp;
        public GameServer() {
        }

        public void Start() {
            string url = "http://*:8080";
            _webApp = WebApp.Start(url, Startup);
        }

        public HostedGameState HostGame(HostedGameRequest game, string connectionId) {
            var uri = new Uri("http://localhost:8080");
            var state = new HostedGameState(game, uri);
            _gameRepo.Checkin(state.DBId, state);
            var hc = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            hc.Groups.Add(connectionId, state.Id.ToString());

            return state;
        }

        private void Startup(IAppBuilder obj) {
            obj.UseCors(CorsOptions.AllowAll);


            // GlobalHost.HubPipeline.AddModule()
            // maybe figure out how to use default authentication
            //      mechanisms built in.

            obj.MapSignalR();
        }

        public void Dispose() {
            _webApp?.Dispose();
            _webApp = null;
        }
    }

    public class GameClient
    {
        public GameClient(ulong gameId) {
            var hubConnection = new HubConnection($"http://localhost:8080/");
            hubConnection.Headers["gameid"] = gameId.ToString();
            var gameHubProxy = hubConnection.CreateHubProxy(nameof(GameHub));

            // http://www.asp.net/signalr/overview/guide-to-the-api/hubs-api-guide-net-client
            //stockTickerHubProxy.On<Stock>("UpdateStockPrice", stock => Console.WriteLine("Stock update for {0} new price {1}", stock.Symbol, stock.Price));
            //await hubConnection.Start();
        }
    }
}
