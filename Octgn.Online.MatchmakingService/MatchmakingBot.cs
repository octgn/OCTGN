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
using log4net;
using Skylabs.Lobby.Messages;
using Skylabs.Lobby.Messages.Matchmaking;

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

		// TODO Shoul dispose queue's when last user leaves.
        public List<MatchmakingQueue> Queue { get; set; }

        public MatchmakingBot() 
            : base(AppConfig.Instance.ServerPath, AppConfig.Instance.XmppUsername, AppConfig.Instance.XmppPassword)
        {
			GenericMessage.Register<StartMatchmakingRequest>();
			Messanger.Map<StartMatchmakingRequest>(StartMatchmakingMessage);

			Queue = new List<MatchmakingQueue>();
        }

        private void StartMatchmakingMessage(StartMatchmakingRequest mess)
        {
            lock (Queue)
            {
                try
                {
                    var queue = Queue.FirstOrDefault(x => x.GameId == mess.GameId
                        && x.GameMode.Equals(mess.GameMode, StringComparison.InvariantCultureIgnoreCase)
                        && x.GameVersion.Major == mess.GameVersion.Major
                        && x.OctgnVersion.CompareTo(mess.OctgnVersion) > 0);
                    if (queue == null)
                    {
                        // Create queue if doesn't exist
                        queue = new MatchmakingQueue(mess.GameId, mess.GameName, mess.GameMode, mess.MaxPlayers, mess.GameVersion, mess.OctgnVersion);
                        Queue.Add(queue);
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
                    Messanger.Send(new StartMatchmakingResponse(mess.From, queue.QueueId));

                    // Done with it.

                }
                catch (Exception e)
                {
                    Log.Error("StartMatchmakingMessage", e);
                }
            }
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
		public Guid QueueId { get; set; }
        public Guid GameId { get; set; }
        public Version GameVersion { get; set; }
        public Version OctgnVersion { get; set; }
        public string GameName { get; set; }
        public string GameMode { get; set; }
        public int MaxPlayers { get; set; }
        public List<Jid> Users { get; set; } 

        private readonly CancellationTokenSource _runCancelToken = new CancellationTokenSource();
        public MatchmakingQueue(Guid gameId, string gameName, string gameMode, int maxPlayers, Version gameVersion, Version octgnVersion)
        {
            QueueId = Guid.NewGuid();
            GameId = gameId;
            GameName = gameName;
            GameMode = gameMode;
            MaxPlayers = maxPlayers;
            GameVersion = gameVersion;
            OctgnVersion = octgnVersion;
        }

        public void Start()
        {
            Task.Factory.StartNew(Run, _runCancelToken.Token,TaskCreationOptions.LongRunning,TaskScheduler.Current);
        }

        private void Run()
        {
            while (_runCancelToken.IsCancellationRequested == false)
            {
                lock (Users)
                {
                    foreach (var u in Users)
                    {
                        // TODO Send them each a message with updated queue info
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }

        public void Enqueue(Jid user)
        {
            throw new NotImplementedException();
        }

        public bool ContainsUser(Jid user)
        {
			throw new NotImplementedException();
        }

        public void Dequeue(Jid user)
        {
			// If user count == 0 stop and dispose
			_runCancelToken.Cancel(false);
            throw new NotImplementedException();
        }

        public void Dispose()
        {
			_runCancelToken.Dispose();
            throw new NotImplementedException();
        }
    }
}
