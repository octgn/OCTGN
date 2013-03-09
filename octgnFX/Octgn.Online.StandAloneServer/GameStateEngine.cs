namespace Octgn.Online.StandAloneServer
{
    using System;
    using System.Reflection;

    using KellyElton.SignalR.TypeSafe.ExtensionMethods;

    using Octgn.Online.Library;
    using Octgn.Online.Library.Coms;
    using Octgn.Online.Library.Enums;
    using Octgn.Online.Library.Models;
    using Octgn.Online.StandAloneServer.Clients;

    using log4net;

    using ConnectionState = Microsoft.AspNet.SignalR.Client.ConnectionState;

    public class GameStateEngine : IGameStateEngine
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static GameStateEngine current;
        private static readonly object SingletonLocker = new object();

        public static GameStateEngine GetContext()
        {
            lock (SingletonLocker)
            {
                return current;
            }
        }

        public static void SetContext(IHostedGameState state, bool isLocal)
        {
            lock (SingletonLocker)
            {
                if (current == null)
                    current = new GameStateEngine(state, isLocal);
            }
        }
        #endregion

        public IHostedGameState Game {
            get
            {
                lock(Locker)
                    return State;
            }
        }


        internal bool Stopped;
        public bool IsLocal { get; internal set; }
        internal HostedGameState State { get; set; }

        private readonly object Locker = new object();

        internal GameStateEngine(IHostedGameState state, bool isLocal)
        {
            State = (HostedGameState)state;
            IsLocal = isLocal;
        }

        public void SetStatus(EnumHostedGameStatus status)
        {
            lock (Locker)
            {
                State.Status = status;
                // Don't use the SasManagerServiceClient if local
                if (IsLocal) return;
                if(SasManagerServiceClient.GetContext().ConnectionState == ConnectionState.Connected)
                    SasManagerServiceClient.GetContext().HubProxy.Send<ISASToSASManagerService>().Invoke().HostedGameStateChanged(State.Id,status);
            }

        }

    }
}