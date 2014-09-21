/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
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

        private readonly object _queueLock = new object();
        private readonly Timer _timer;

        public MatchmakingBot()
            : base(AppConfig.Instance.ServerPath, AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword)
        {
            ElementFactory.AddElementType("gameitem", "octgn:gameitem", typeof(HostedGameData));
            Messanger.Map<StartMatchmakingRequest>(StartMatchmakingMessage);
            Messanger.Map<MatchmakingLeaveQueueMessage>(LeaveMatchmakingQueueMessage);

            Queue = new List<MatchmakingQueue>();
            _timer = new Timer(2000);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        protected override void OnResetXmpp()
        {
            Log.InfoFormat("Resetting Bot");
            base.OnResetXmpp();
            Xmpp.OnMessage += XmppOnOnMessage;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (_queueLock)
            {
                foreach (var q in Queue.ToArray())
                {
                    if (q.Disposed)
                    {
						Log.InfoFormat("Queue disposed, removing {0}",q);
                        Queue.Remove(q);
                    }
                }
            }
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
                    lock (_queueLock)
                    {
                        foreach (var q in Queue.ToArray())
                        {
							q.OnGameHostResponse(game);
                        }
                    }
                }
            }
        }

        private void StartMatchmakingMessage(StartMatchmakingRequest mess)
        {
            lock (_queueLock)
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
                    Messanger.Send(new StartMatchmakingResponse(mess.RequestId, mess.From, queue.QueueId));

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
            lock (_queueLock)
            {
                try
                {
					Log.InfoFormat("User left matchmaking {0}",obj.From);
                    var queue = Queue.FirstOrDefault(x => x.QueueId == obj.QueueId);
                    if (queue == null)
                        return;
                    queue.UserLeave(obj.From);
                }
                catch (Exception e)
                {
                    Log.Error("LeaveMatchmakingQueueMessage", e);
                }
            }
        }


        public Guid BeginHostGame(Guid gameid, Version gameVersion, string gamename,
            string gameIconUrl, string password, string actualgamename, Version sasVersion, bool specators)
        {
            var hgr = new HostGameRequest(gameid, gameVersion, gamename, actualgamename, gameIconUrl, password ?? "", sasVersion, specators);
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
            lock (_queueLock)
            {
                foreach (var q in Queue)
                {
                    q.Dispose();
                }
                Queue.Clear();
            }
			_timer.Dispose();
        }
    }

    public enum MatchmakingQueueState
    {
        WaitingForUsers, WaitingForReadyUsers, WaitingForHostedGame
    }
}
