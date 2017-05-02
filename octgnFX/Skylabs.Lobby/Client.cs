/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using log4net;
using System.Threading.Tasks;

namespace Skylabs.Lobby
{
    public class Client
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string email;

        public string Username { get; private set; }
        public string Password { get; private set; }
        public User Me { get; private set; }
        public int CurrentHostedGamePort { get; set; }
        public bool IsConnected { get; private set; }

        private readonly ILobbyConfig _config;
        private readonly Octgn.Chat.Communication.Client _client;

        public Client(ILobbyConfig config, string username, string password)
        {
            Username = username;
            Password = password;
            _config = config;
            _client = new Octgn.Chat.Communication.Client(new Octgn.Chat.Communication.TcpConnection(_config.ChatHost));
            _client.DeliverableReceived += Client_DeliverableReceived;
        }

        public async Task<Octgn.Chat.Communication.Messages.Login.LoginResultType> Connect()
        {
            return await _client.Connect(Username, Password);
        }

        public void Stop()
        {
            Log.Info("Xmpp Stop called");
            Trace.WriteLine("[Lobby]Stop Called.");
            _client.Connection.IsClosed = false;
        }

        private void Client_DeliverableReceived(object sender, Octgn.Chat.Communication.DeliverableReceivedEventArgs args)
        {
            if (msg.Type == MessageType.normal) {
                if (msg.Subject == "gameready") {
                    Log.Info("Got gameready message");

                    var game = msg.ChildNodes.OfType<HostedGameData>().FirstOrDefault();
                    if (game == null) {
                        Log.Warn("Game message wasn't in the correct format.");
                        return;
                    }

                    this.CurrentHostedGamePort = game.Port;

                    if (this.OnDataReceived != null)
                        this.OnDataReceived.Invoke(this, DataRecType.HostedGameReady, game);

                } else if (msg.Subject == "gamelist") {
                    Log.Info("Got gamelist msg, this doesn't do anything anymore");
                    //var list = new List<HostedGameData>();
                    //foreach (object a in msg.ChildNodes)
                    //{
                    //    var gi = a as HostedGameData;
                    //    if (gi != null)
                    //    {
                    //        list.Add(gi);
                    //    }
                    //}

                    //this.games = list;
                    //if (this.OnDataReceived != null)
                    //{
                    //    this.OnDataReceived.Invoke(this, DataRecType.GameList, list);
                    //}
                } else if (msg.Subject == "refresh") {
                    Log.Info("Server wants a refresh of game list");
                    if (this.OnDataReceived != null) {
                        Log.Info("Firing server wants a refresh of game list");
                        this.OnDataReceived.Invoke(this, DataRecType.GamesNeedRefresh, null);
                    }
                } else if (msg.Subject == "invitetogamerequest") {
                    Log.InfoFormat("Received game invite from user {0}", msg.From.User);
                    InviteToGameRequest req = msg.ChildNodes.OfType<InviteToGameRequest>().FirstOrDefault();
                    if (req == null) {
                        Log.WarnFormat("Tried to read invitetogamerequest packet but it was broken...");
                        return;
                    }
                    if (this.OnDataReceived != null) {
                        var sreq = new InviteToGame();
                        sreq.From = new User(msg.From);
                        sreq.SessionId = req.SessionId;
                        sreq.Password = req.Password;
                        this.OnDataReceived.Invoke(this, DataRecType.GameInvite, sreq);
                    }
                } else if (msg.From.Bare.ToLower() == this.xmpp.MyJID.Server.ToLower()) {
                    if (msg.Subject == null) {
                        msg.Subject = string.Empty;
                    }

                    if (msg.Body == null) {
                        msg.Body = string.Empty;
                    }

                    var d = new Dictionary<string, string>();
                    d["Message"] = msg.Body;
                    d["Subject"] = msg.Subject;
                    if (this.OnDataReceived != null) {
                        this.OnDataReceived.Invoke(this, DataRecType.Announcement, d);
                    }
                }
            }
        }

        public void BeginHostGame(Octgn.DataNew.Entities.Game game, string gamename,
            string password, string actualgamename, string gameIconUrl, Version sasVersion, bool specators)
        {
            var hgr = new HostGameRequest(game.Id, game.Version, gamename, actualgamename, gameIconUrl, password ?? "", sasVersion, specators);
            Log.InfoFormat("BeginHostGame {0}", hgr);

            _client.Request(new Octgn.Chat.Package(_config.GameBotUser.UserName, hgr));
        }

        //public void KillGame(Guid gameId)
        //{
        //    var m = new Message(this._config.GameBotUser.JidUser, this.Me.JidUser, MessageType.normal, string.Format("{0}#:999:#{1}", gameId, this.Password), "killgame");
        //    m.GenerateId();
        //    this.xmpp.Send(m);
        //}
        public void HostedGameStarted()
        {
            var message = new Octgn.Chat.Message(_config.GameBotUser.UserName, "gamestarted");

            _client.Request(message);
        }

        public void SendGameInvite(User user, Guid sessionId, string gamePassword)
        {
            Log.InfoFormat("Sending game request to {0}", user.UserName);
            var req = new InviteToGameRequest(sessionId, gamePassword);

            _client.Request(new Octgn.Chat.Package(user.UserName, req));
        }
    }

    public class InviteToGame
    {
        public User From { get; set; }
        public Guid SessionId { get; set; }
        public string Password { get; set; }
    }
}
