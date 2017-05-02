/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Linq;
using System.Reflection;
using log4net;
using Octgn.Site.Api;
using Skylabs.Lobby;
using LoginResult = Octgn.Site.Api.LoginResult;

namespace Octgn.Online.GameService
{
    using System.Runtime.Caching;
    using System.Threading;
    using System.Timers;

    using Timer = System.Timers.Timer;

    public class GameBot : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static GameBot SingletonContext { get; set; }

        private static readonly object GameBotSingletonLocker = new object();

        public static GameBot Instance {
            get {
                if (SingletonContext == null)
                {
                    lock (GameBotSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new GameBot();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private readonly MemoryCache _userRequests;
        private readonly Timer _refreshGamesTimer;

        private GameBot()
        {
            _userRequests = new MemoryCache(nameof(_userRequests));
            _refreshGamesTimer = new Timer(2000);
            _refreshGamesTimer.Start();
            _refreshGamesTimer.Elapsed += RefreshGamesTimerOnElapsed;
        }

        private void RefreshGamesTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                _refreshGamesTimer.Enabled = false;

                if (_userRequests.GetCount() == 0) return;
                var games = GameManager.Instance.Games.ToArray();

                foreach (var game in games)
                {
                    var strname = "hostrequest_" + game.Id;
                    if (_userRequests.Contains(strname))
                    {
                        _userRequests.Remove(strname);

                        SendGameReady(game);
                    }
                }

            }
            catch(Exception e)
            {
                Log.Error("[RefreshGamesTimerOnElapsed]", e);
            }
            finally
            {
                _refreshGamesTimer.Enabled = true;
            }
        }

        public void Start()
        {
            //_xmpp = new XmppClientConnection(AppConfig.Instance.ServerPath);
            //ElementFactory.AddElementType("hostgamerequest", "octgn:hostgamerequest", typeof(HostGameRequest));

            //_xmpp.Username = AppConfig.Instance.XmppUsername;
            //_xmpp.Password = AppConfig.Instance.XmppPassword;
            throw new NotImplementedException();
        }

        public void SendGameReady(HostedGameData game)
        {
            //var m = new Message(game.Username + "@of.octgn.net", MessageType.normal, "", "gameready");
            throw new NotImplementedException();
        }

        public void CheckBotStatus()
        {
            throw new NotImplementedException();
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
            try
            {
                switch (msg.Type)
                {
                    case MessageType.normal:
                        if (msg.Subject == "hostgame")
                        {

                            if (!msg.HasChildElements)
                            {
                                Log.Error("BADREQUEST hostgame has no children");
                                // F it, someone screwed up this year.
                                return;
                            }

                            if (!msg.ChildNodes.OfType<HostGameRequest>().Any())
                            {
                                Log.Error("BADREQUEST hostgame doesn't have a request");
                                // Again, what the fuuuuu
                                return;
                            }

                            var req = msg.ChildNodes.OfType<HostGameRequest>().First();

                            Log.InfoFormat("Host game from {0}", msg.From);
                            var endTime = DateTime.Now.AddSeconds(10);
                            while (SasUpdater.Instance.IsUpdating)
                            {
                                Thread.Sleep(100);
                                if (endTime > DateTime.Now) throw new Exception("Couldn't host, sas is updating");
                            }
                            var id = GameManager.Instance.HostGame(req, new User(msg.From));

                            if (id == Guid.Empty) throw new InvalidOperationException("id == Guid.Empty");

                            if (id != Guid.Empty)
                                _userRequests.Add("hostrequest_" + id, id, DateTimeOffset.UtcNow.AddSeconds(30));
                        }
                        else if (msg.Subject == "gamelist")
                        {
                            // If someone tried to refresh their game list too soon, f them
                            if (_userRequests.Contains("refreshrequest_" + msg.From.User.ToLower()))
                                return;
                            // Mark the user as already requested a list for the next 15 seconds
                            _userRequests.Add("refreshrequest_" + msg.From.User.ToLower(), 1, DateTimeOffset.UtcNow.AddSeconds(15));
                            var list = GameManager.Instance.Games;
                            var m = new Message(msg.From, MessageType.normal, "", "gamelist");
                            m.GenerateId();
                            foreach (var a in list)
                            {
                                m.AddChild(a);
                            }
                            _xmpp.Send(m);
                        }
                        else if (msg.Subject == "killgame")
                        {
                            var items = msg.Body.Split(new[] { "#:999:#" }, StringSplitOptions.RemoveEmptyEntries);
                            if (items.Length != 2) return;
                            var client = new ApiClient();
                            var res = client.Login(msg.From.User, items[1]);
                            if (res.Type == LoginResultType.Ok)
                            {
                                var id = Guid.Parse(items[0]);
                                GameManager.Instance.KillGame(id);
                            } else throw new Exception("Error verifying user " + res);
                        }
                        break;
                    case MessageType.error:
                        Log.Error("Message Error " + msg.Error);
                        break;
                    case MessageType.chat:
                        if (!msg.From.User.Equals("d0c", StringComparison.InvariantCultureIgnoreCase)) return;
                        // Keep this around in case we want to add commands at some point, we'll have an idea on how to write the code
                        //if (msg.Body.Equals("pause"))
                        //{
                        //    _isPaused = true;
                        //    Log.Warn(":::::: PAUSED ::::::");
                        //    var m = new Message(msg.From, MessageType.chat, "Paused");
                        //    m.GenerateId();
                        //    Xmpp.Send(m);
                        //}
                        break;
                }

            }
            catch (Exception e)
            {
                Log.Error("[Bot]XmppOnOnMessage Error", e);
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Log.Info(nameof(GameBot) + " Disposed");
            _userRequests.Dispose();
            _refreshGamesTimer.Elapsed -= RefreshGamesTimerOnElapsed;
        }

        #endregion
    }
}
