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
using agsXMPP.Factory;
using agsXMPP.protocol.client;
using log4net;
using Skylabs.Lobby;
using Skylabs.Lobby.Messages;
using Skylabs.Lobby.Messages.Matchmaking;
using Message = agsXMPP.protocol.client.Message;

namespace Octgn.Online.MatchmakingService
{
    public class MatchmakingBot : XmppClient
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Singleton

        internal static MatchmakingBot SingletonContext { get; set; }

        private static readonly object GameBotSingletonLocker = new object();

        public static MatchmakingBot Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameBotSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new MatchmakingBot();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public List<MatchmakingQueue> Queue { get; set; }

        public MatchmakingBot()
            : base(AppConfig.Instance.ServerPath, AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword)
        {
			ElementFactory.AddElementType("gameitem", "octgn:gameitem", typeof(HostedGameData));
            Messanger.Map<StartMatchmakingRequest>(StartMatchmakingMessage);
            Messanger.Map<MatchmakingLeaveQueueMessage>(LeaveMatchmakingQueueMessage);

            Queue = new List<MatchmakingQueue>();

        }

        protected override void OnResetXmpp()
        {
            base.OnResetXmpp();
            Xmpp.OnMessage += XmppOnOnMessage;
        }

        private void XmppOnOnMessage(object sender, Message msg)
        {
            if (msg.Type == MessageType.normal)
            {
                if (msg.Subject == "gameready")
                {
                    Log.Info("Got gameready message");

                    var game = msg.ChildNodes.OfType<HostedGameData>().FirstOrDefault();
                    if (game == null)
                    {
                        Log.Warn("Game message wasn't in the correct format.");
                        return;
                    }
                    lock (Queue)
                    {
                        foreach (var q in Queue)
                        {
                            q.OnGameHostResponse(game);
                        }
                    }
                }
            }
        }

        private void StartMatchmakingMessage(StartMatchmakingRequest mess)
        {
            lock (Queue)
            {
                try
                {
					Log.Debug("StartMatchmakingMessage");
                    var queue = Queue.FirstOrDefault(x => x.GameId == mess.GameId
                        && x.GameMode.Equals(mess.GameMode, StringComparison.InvariantCultureIgnoreCase)
                        && x.GameVersion.Major == mess.GameVersion.Major
                        && x.OctgnVersion.CompareTo(mess.OctgnVersion) == 0);
                    if (queue == null)
                    {
						Log.Debug("Creating queue");
                        // Create queue if doesn't exist
                        queue = new MatchmakingQueue(this, mess.GameId, mess.GameName, mess.GameMode, mess.MaxPlayers, mess.GameVersion, mess.OctgnVersion);
                        Queue.Add(queue);
						queue.Start();
                    }

                    // if User is queued, drop him/her from previous queue
                    // Don't drop them if they're in this queue
                    foreach (var q in Queue.Where(x => x != queue))
                    {
                        q.Dequeue(mess.From);
                    }

                    // Add user to queue
                    queue.Enqueue(mess.From);

                    // Send user a message
                    Messanger.Send(new StartMatchmakingResponse(mess.RequestId,mess.From, queue.QueueId));

                    // Done with it.

                }
                catch (Exception e)
                {
                    Log.Error("StartMatchmakingMessage", e);
                }
            }
        }

        private void LeaveMatchmakingQueueMessage(MatchmakingLeaveQueueMessage obj)
        {
            lock (Queue)
            {
                try
                {
                    var queue = Queue.FirstOrDefault(x => x.QueueId == obj.QueueId);
                    if (queue == null)
                        return;
                    queue.UserLeave(obj.From);
                }
                catch (Exception e)
                {
                    Log.Error("StartMatchmakingMessage", e);
                }
            }
        }


        public Guid BeginHostGame(Guid gameid, Version gameVersion, string gamename,
            string password, string actualgamename, Version sasVersion, bool specators)
        {
            var hgr = new HostGameRequest(gameid, gameVersion, gamename, actualgamename, password ?? "", sasVersion, specators);
            Log.InfoFormat("BeginHostGame {0}", hgr);
            var m = new Message(new Jid(AppConfig.Instance.GameServUsername, AppConfig.Instance.ServerPath, null), this.Xmpp.MyJID, MessageType.normal, "", "hostgame");
            m.GenerateId();
            m.AddChild(hgr);
            this.Xmpp.Send(m);
            return hgr.RequestId;
        }

        public override void Dispose()
        {
            base.Dispose();
            lock (Queue)
            {
                foreach (var q in Queue)
                {
                    q.Dispose();
                }
                Queue.Clear();
            }
        }
    }

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
        public MatchmakingQueueState State { get; set; }
        public AverageTime AverageTime { get; set; }

        private Guid _waitingRequestId = Guid.Empty;
        private readonly CancellationTokenSource _runCancelToken = new CancellationTokenSource();
		private readonly TimeBlock _hostGameTimeout = new TimeBlock(TimeSpan.FromSeconds(10));
        private readonly List<QueueUser> _users;
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
        }

        public void Start()
        {
            Task.Factory.StartNew(Run, _runCancelToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        private void Run()
        {
            var sendStatusUpdatesBlock = new TimeBlock(TimeSpan.FromSeconds(30));
            var readyTimeout = new TimeBlock(TimeSpan.FromSeconds(60));
            var disposeThis = new TimeBlock(TimeSpan.FromDays(1));
            while (_runCancelToken.IsCancellationRequested == false)
            {
                lock (_users)
                {
                    if (_users.Count > 0)
                    {
                        disposeThis.SetRun();
                    }
                    if (disposeThis.IsTime)
                    {
						this.Dispose();
                        lock (this.Bot.Queue)
                        {
                            this.Bot.Queue.Remove(this);
                        }
                        break;
                    }
                    switch (State)
                    {
                        case MatchmakingQueueState.WaitingForUsers:
                            if (_users.Count >= MaxPlayers)
                            {
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
                                // This happens if 60 seconds has passed since ready messages were sent out.
                                // This shouldn't happen unless someone didn't respond back with a ready response.
                                foreach (var p in _users.Where(x => x.IsInReadyQueue))
                                {
                                    // If player didn't signal ready throw them in the back of the queue.
                                    if (p.IsReady == false)
                                    {
                                        p.FailedReadyCount++;
                                        Dequeue(p);
                                        // If they've been knocked to the back of the queue 4 or more times, kick them.
                                        if (p.FailedReadyCount < 4)
                                        {
                                            Enqueue(p);
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
                if (_runCancelToken.IsCancellationRequested == false)
                    Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public void OnGameHostResponse(HostedGameData data)
        {
            if (_waitingRequestId != data.Id)
                return;
            lock (_users)
            {
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
                foreach (var u in _users.Where(x => x.IsInReadyQueue).ToArray())
                {
                    message.To = u.JidUser;
					message.GenerateId();
                    this.Bot.Xmpp.Send(message);
                    Dequeue(u);
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
				var usr = _users.FirstOrDefault(x => x == user);
                if (usr == null)
                    return;
                switch (State)
                {
                    case MatchmakingQueueState.WaitingForReadyUsers:
                    case MatchmakingQueueState.WaitingForHostedGame:
                        if (usr.IsInReadyQueue)
                        {
							State = MatchmakingQueueState.WaitingForUsers;
                            foreach (var u in _users)
                            {
                                u.IsInReadyQueue = false;
                                u.IsReady = false;
                            }
                            var msg = new MatchmakingReadyFail(null, this.QueueId);
                            foreach (var u in _users.Where(x => x.IsInReadyQueue && x != usr))
                            {
                                msg.To = u.JidUser;
								this.Bot.Messanger.Send(msg);
                            }
                        }
                        break;
                }

                _users.Remove(usr);
            }
        }

        public void Enqueue(Jid user)
        {
            lock (_users)
            {
                _users.Add(user);
            }
        }

        public void Dequeue(Jid user)
        {
            lock (_users)
            {
                var item = _users.FirstOrDefault(x => x == user);
                if (item != null)
                {
                    _users.Remove(item);
                }
            }
        }

        private void OnMatchmakingReadyResponse(MatchmakingReadyResponse obj)
        {
            lock (_users)
            {
                if (State != MatchmakingQueueState.WaitingForReadyUsers)
                    return;
                var user = _users.FirstOrDefault(x => x == obj.From);
                if (user != null)
                    user.IsReady = true;

                if (_users.Where(x => x.IsInReadyQueue).All(x => x.IsReady))
                {
                    // All users are ready.
                    // Spin up a gameserver for them to join
                    var agamename = string.Format("Matchmaking: {0}[{1}]", this.GameName, this.GameMode);
                    _waitingRequestId = Bot.BeginHostGame(this.GameId, this.GameVersion, agamename,obj.QueueId.ToString().ToLower(), this.GameName, typeof(MatchmakingBot).Assembly.GetName().Version, true);
                    State = MatchmakingQueueState.WaitingForHostedGame;
					_hostGameTimeout.SetRun();
                }
            }
        }

        public void Dispose()
        {
            _runCancelToken.Cancel(false);
            _runCancelToken.Token.WaitHandle.WaitOne(60000);
            _runCancelToken.Dispose();
        }
    }

    public enum MatchmakingQueueState
    {
        WaitingForUsers, WaitingForReadyUsers, WaitingForHostedGame
    }

    public class AverageTime
    {
        public TimeSpan Time
        {
            get
            {
                lock (this)
                {
                    var list = _previousOnes.ToList();
					list.Add(DateTime.Now.Ticks - _startTime.Ticks);
                    var span = new TimeSpan((long)list.Average(x => x));
                    return span;
                }
            }
        }

        private DateTime _startTime;
        private readonly int _count;
        private readonly Queue<long> _previousOnes = new Queue<long>();

        public AverageTime(int count)
        {
            _count = count;
			_startTime = DateTime.Now;
        }

        public void Cycle()
        {
            lock (this)
            {
                _previousOnes.Enqueue(DateTime.Now.Ticks - _startTime.Ticks);
                while (_previousOnes.Count > _count)
                {
                    _previousOnes.Dequeue();
                }
            }
        }
    }
}
