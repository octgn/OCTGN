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
using Octgn.Library;

namespace Octgn.Hosting
{
    public class GameServer : IDisposable {
        #region Singleton

        internal static GameServer SingletonContext { get; set; }

        private static readonly object GameServerSingletonLocker = new object();

        public static GameServer Instance {
            get {
                if (SingletonContext == null) {
                    lock (GameServerSingletonLocker) {
                        if (SingletonContext == null) {
                            SingletonContext = new GameServer();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

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

        public Uri ConnectionString { get; set; }

        private IDisposable _webApp;
        public GameServer() {
        }

        public void Start(string connectionString) {
            ConnectionString = new Uri(connectionString);
            _webApp = WebApp.Start(ConnectionString.AbsoluteUri, Startup);
        }

        public HostedGameState HostGame(HostedGameRequest game) {
            var state = new HostedGameState(game, ConnectionString);
            state.DBId = (RandomXDigitNumber)4;
            _gameRepo.Checkin(state.DBId, state);
            var hc = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            hc.Groups.Add(game.Id.ToString(), state.Id.ToString());

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
}
