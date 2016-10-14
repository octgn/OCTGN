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

namespace Octgn.LocalHost
{
    public class GameServer : IDisposable
    {
        internal static log4net.ILog Log = log4net.LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

        #region Singleton

        internal static GameServer SingletonContext { get; set; }

        private static readonly object GameServerSingletonLocker = new object();

        public static GameServer Instance {
            get {
                if( SingletonContext == null ) {
                    lock( GameServerSingletonLocker ) {
                        if( SingletonContext == null ) {
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
                .Register( typeof( GameHub ), () => {
                    var settings = new OctgnServerSettings {
                        IsLocalGame = true
                    };
                    var ret = new GameHub( _requestHandler, _gameRepo, settings );
                    return ret;
                } );
        }

        private static readonly ServerGameRepository _gameRepo;
        private static readonly RequestHandler _requestHandler;

        public string ConnectionString { get; set; }
        public Uri ConnectionUri { get; set; }

        public bool IsRunning { get; private set; }

        private IDisposable _webApp;
        public GameServer() {
        }

        public void Start( Uri connectionUri ) {
            try {
                Log.Info( $"Starting host on {connectionUri}" );
                if( _webApp != null ) _webApp.Dispose();
                ConnectionUri = connectionUri;
                _webApp = WebApp.Start( ConnectionUri.AbsoluteUri, Startup );
                IsRunning = true;
            } catch {
                IsRunning = false;
                _webApp?.Dispose();
                _webApp = null;
                throw;
            }
        }

        public HostedGameState HostGame( HostedGameRequest game ) {
            Log.Info( $"[HostGame] {game.GameId} {game.Name}" );
            // TODO ConnecitonUri won't work here, it needs to be something other people can call...most likely an array
            var state = new HostedGameState( game, ConnectionUri ) {
                Id = Guid.NewGuid()
            };
            _gameRepo.Checkin( state.Id, state );

            return state;
        }

        private void Startup( IAppBuilder obj ) {
            obj.UseCors( CorsOptions.AllowAll );

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
