/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using agsXMPP;
using agsXMPP.protocol.client;
using log4net;
using Skylabs.Lobby;
using Skylabs.Lobby.Messages;
using Skylabs.Lobby.Messages.Matchmaking;

namespace Octgn.Online.MatchmakingService
{
    public class MatchmakingQueue : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Guid QueueId { get; set; }
        public Guid GameId { get; set; }
        public Version GameVersion { get; set; }
        public Version OctgnVersion { get; set; }
        public string GameName { get; set; }
        public string GameMode { get; set; }
        public int MaxPlayers { get; set; }
        public MatchmakingBot Bot { get; set; }

        public MatchmakingQueueState State
        {
            get { return _state; }
            set
            {
                if (value == _state) return;
				Log.InfoFormat("[{0}] State Changed From {1} To {2}", this,_state,value);
                _state = value;
            }
        }

        public AverageTime AverageTime { get; set; }
        public bool Disposed { get; set; }

        private Guid _waitingRequestId = Guid.Empty;
        private readonly CancellationTokenSource _runCancelToken = new CancellationTokenSource();
        private readonly TimeBlock _hostGameTimeout = new TimeBlock(TimeSpan.FromSeconds(10));
        private readonly List<QueueUser> _users;
        private MatchmakingQueueState _state;

        public MatchmakingQueue(MatchmakingBot bot, Guid gameId, string gameName, string gameMode, int maxPlayers, Version gameVersion, Version octgnVersion)
        {
            QueueId = Guid.NewGuid();
            GameId = gameId;
            GameName = gameName;
            GameMode = gameMode;
            MaxPlayers = maxPlayers;
            GameVersion = gameVersion;
            OctgnVersion = octgnVersion;
            _users = new List<QueueUser>();
            Bot = bot;
            Bot.Messanger.Map<MatchmakingReadyResponse>(OnMatchmakingReadyResponse);
            State = MatchmakingQueueState.WaitingForUsers;
            AverageTime = new AverageTime(10);
            Log.InfoFormat("[{0}] Queue Created", this);
        }

        public void Start()
        {
            Log.InfoFormat("[{0}] Starting Queue",this);
            Task.Factory.StartNew(Run, _runCancelToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        private void Run()
        {
			Log.InfoFormat("[{0}] Queue Running",this);
            var sendStatusUpdatesBlock = new TimeBlock(TimeSpan.FromSeconds(30));
            var readyTimeout = new TimeBlock(TimeSpan.FromSeconds(60));
            var disposeThis = new TimeBlock(TimeSpan.FromHours(1));
			disposeThis.SetRun();
            while (_runCancelToken.IsCancellationRequested == false && Disposed == false)
            {
                lock (_users)
                {
                    try
                    {
                        if (_users.Count > 0)
                        {
                            disposeThis.SetRun();
                        }
                        if (disposeThis.IsTime)
                        {
							Log.InfoFormat("[{0}] Dispose timer timed out",this);
                            Disposed = true;
                            this.Dispose();
                            break;
                        }
                        switch (State)
                        {
                            case MatchmakingQueueState.WaitingForUsers:
                                if (_users.Count >= MaxPlayers)
                                {
                                    Log.InfoFormat("[{0}] Got enough players for ready queue.",this);
                                    State = MatchmakingQueueState.WaitingForReadyUsers;
                                    var readyMessage = new MatchmakingReadyRequest(null, this.QueueId);
                                    foreach (var p in _users)
                                    {
                                        p.IsReady = false;
                                        p.IsInReadyQueue = false;
                                    }
                                    for (var i = 0; i < MaxPlayers; i++)
                                    {
                                        readyMessage.To = _users[i];
                                        _users[i].IsInReadyQueue = true;
                                        Bot.Messanger.Send(readyMessage);
                                    }
                                    readyTimeout.SetRun();
                                }
                                break;
                            case MatchmakingQueueState.WaitingForReadyUsers:
                                if (readyTimeout.IsTime)
                                {
                                    Log.InfoFormat("[{0}] Timed out waiting for users to ready up.",this);
                                    // This happens if 60 seconds has passed since ready messages were sent out.
                                    // This shouldn't happen unless someone didn't respond back with a ready response.
                                    foreach (var p in _users.Where(x => x.IsInReadyQueue).ToArray())
                                    {
                                        // If player didn't signal ready throw them in the back of the queue.
                                        if (p.IsReady == false)
                                        {
                                            p.FailedReadyCount++;
                                            _users.Remove(p);
                                            // If they've been knocked to the back of the queue 4 or more times, kick them.
                                            if (p.FailedReadyCount < 4)
                                            {
                                                Log.InfoFormat("[{0}] {1} User failed ready 4 times, getting kicked.",this,p);
                                                _users.Add(p);
                                            }
                                        }
                                        p.IsInReadyQueue = false;
                                        p.IsReady = false;
                                    }
                                }
                                break;
                            case MatchmakingQueueState.WaitingForHostedGame:
                                if (_hostGameTimeout.IsTime)
                                {
                                    Log.WarnFormat("[{0}] Timed out waiting for hosted game to be created...Oh Noes!",this);
                                    State = MatchmakingQueueState.WaitingForUsers;
                                    foreach (var u in _users)
                                    {
                                        u.IsInReadyQueue = false;
                                        u.IsReady = false;
                                    }
                                }
                                break;
                        }

                        // Send status messages
                        if (sendStatusUpdatesBlock.IsTime)
                        {
                            var mess = new MatchmakingInLineUpdateMessage(AverageTime.Time, null, QueueId);
                            foreach (var u in _users.Where(x => x.IsInReadyQueue == false))
                            {
                                mess.To = u.JidUser;
                                mess.GenerateId();
                                Bot.Messanger.Send(mess);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Log.Error ("[" + this + "] Run",e);
                    }
                }
                if (_runCancelToken.IsCancellationRequested == false && !Disposed)
                    Thread.Sleep(TimeSpan.FromSeconds(1));
            }
			Log.InfoFormat("[{0}] Queue done running",this);
        }

        public void OnGameHostResponse(HostedGameData data)
        {
            if (_waitingRequestId != data.Id)
                return;
            lock (_users)
            {
				Log.InfoFormat("[{0}] Got Hosted Game Data {1}",this,data);
                if (State != MatchmakingQueueState.WaitingForHostedGame)
                {
                    // Actually this can happen if someone cancels out of the queue
                    //Log.Fatal("Timeed out before hosted game could be returned. Need to increase timeout.");
                    //this._hostGameTimeout.When = new TimeSpan(0,0,(int)_hostGameTimeout.When.TotalSeconds + 5);
                    return;
                }
                // Send all users a message telling them the info they need to connect to game server
                // Kick all the users from the queue.
                var message = new Message(new Jid("a@b.com"), MessageType.normal, "", "gameready");
                message.ChildNodes.Add(data);
				Log.InfoFormat("[{0}] Sending users game data",this);
                foreach (var u in _users.Where(x => x.IsInReadyQueue).ToArray())
                {
                    message.To = u.JidUser;
                    message.GenerateId();
                    this.Bot.Xmpp.Send(message);
                    _users.Remove(u);
                }
                // set time to game
                AverageTime.Cycle();
                State = MatchmakingQueueState.WaitingForUsers;
            }
        }

        public void UserLeave(Jid user)
        {
            lock (_users)
            {
				Log.InfoFormat("[{0}] {1} User Left",this, user);
                var usr = _users.FirstOrDefault(x => x == user);
                if (usr == null)
                    return;
                switch (State)
                {
                    case MatchmakingQueueState.WaitingForReadyUsers:
                    case MatchmakingQueueState.WaitingForHostedGame:
                        if (usr.IsInReadyQueue)
                        {
							Log.InfoFormat("[{0}] {1} User was in ready queue, so cancel that.",this,usr);
                            State = MatchmakingQueueState.WaitingForUsers;
                            foreach (var u in _users)
                            {
                                u.IsInReadyQueue = false;
                                u.IsReady = false;
                            }
                            var msg = new MatchmakingReadyFail(null, this.QueueId);
							Log.InfoFormat("[{0}] {1} Tell everyone ready queue failed.",this,usr);
                            foreach (var u in _users.Where(x => x.IsInReadyQueue && x != usr))
                            {
                                msg.To = u.JidUser;
                                this.Bot.Messanger.Send(msg);
                            }
                        }
                        break;
                }
				Log.InfoFormat("[{0}] {1} Actually remove the user",this,usr);
                _users.Remove(usr);
            }
        }

        public void Enqueue(Jid user)
        {
            lock (_users)
            {
				Log.InfoFormat("[{0}] {1} Queueing User",this,user);
                _users.Add(user);
            }
        }

        public void Dequeue(Jid user)
        {
            lock (_users)
            {
				Log.InfoFormat("[{0}] {1} Dequeueing User",this,user);
                var item = _users.FirstOrDefault(x => x == user);
                if (item != null)
                {
                    _users.Remove(item);
                }
            }
        }

        private void OnMatchmakingReadyResponse(MatchmakingReadyResponse obj)
        {
            if (obj.QueueId != this.QueueId)
                return;
            lock (_users)
            {
				Log.InfoFormat("[{0}]{1} Got Ready Response from user",this,obj.From);
                if (State != MatchmakingQueueState.WaitingForReadyUsers)
                {
					Log.InfoFormat("[{0}] Not in ready queue anymore, so forget it.",this);
                    return;
                }
                var user = _users.FirstOrDefault(x => x == obj.From);
                if (user != null)
                    user.IsReady = true;

                if (_users.Where(x => x.IsInReadyQueue).All(x => x.IsReady))
                {
					Log.InfoFormat("[{0}] All users are ready, spin up a game",this);
                    // All users are ready.
                    // Spin up a gameserver for them to join
                    var agamename = string.Format("Matchmaking: {0}[{1}]", this.GameName, this.GameMode);
                    _waitingRequestId = Bot.BeginHostGame(this.GameId, this.GameVersion, agamename,"",obj.QueueId.ToString().ToLower(), this.GameName, typeof(XmppClient).Assembly.GetName().Version, true);
                    State = MatchmakingQueueState.WaitingForHostedGame;
                    _hostGameTimeout.SetRun();
                }
            }
        }

        public void Dispose()
        {
			Log.InfoFormat("[{0}] Disposing",this);
            _runCancelToken.Cancel(false);
            _runCancelToken.Token.WaitHandle.WaitOne(60000);
            _runCancelToken.Dispose();
			Log.InfoFormat("[{0}] Disposed Cancel Wait Handle",this);
            Disposed = true;
        }

        public override string ToString()
        {
            return String.Format("Q[{0} D={1} S={2} GID={3} GV={4} OV={5} GN={6} GM={7} MP={8}]", QueueId,Disposed,State,GameId, GameVersion, OctgnVersion, GameName, GameMode, MaxPlayers);
        }
    }
}