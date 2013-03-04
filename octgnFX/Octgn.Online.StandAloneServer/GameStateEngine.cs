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

        internal bool Stopped;

        public static GameStateEngine GetContext()
        {
            lock (SingletonLocker)
            {
                return current;
            }
        }

        public static void SetContext(IHostedGameState state)
        {
            lock (SingletonLocker)
            {
                if (current == null)
                    current = new GameStateEngine(state);
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

        internal HostedGameState State { get; set; }

        private readonly object Locker = new object();

        internal GameStateEngine(IHostedGameState state)
        {
            State = (HostedGameState)state;
        }

        public void SetStatus(EnumHostedGameStatus status)
        {
            lock (Locker)
            {
                State.Status = status;
                if(SasManagerServiceClient.GetContext().ConnectionState == ConnectionState.Connected)
                    SasManagerServiceClient.GetContext().HubProxy.Send<ISASToSASManagerService>().Invoke().HostedGameStateChanged(State.Id,status);
            }

        }

    }
}