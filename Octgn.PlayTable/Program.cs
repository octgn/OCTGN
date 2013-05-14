namespace Octgn.PlayTable
{
    using System.Windows;
    using System.Windows.Threading;

    using Octgn.Core;
    using Octgn.Core.Play;
    using Octgn.Networking;
    using Octgn.Play;

    public static class Program
    {
        public static Client Client
        {
            get
            {
                return K.C.Get<Client>();
            }
        }

        public static PlayerStateMachine Player
        {
            get
            {
                return K.C.Get<PlayerStateMachine>();
            }
        }

        public static CardStateMachine Card
        {
            get
            {
                return K.C.Get<CardStateMachine>();
            }
        }

        public static GameplayTrace Trace
        {
            get
            {
                return K.C.Get<GameplayTrace>();
            }
        }

        public static IGameEngine GameEngine
        {
            get
            {
                return K.C.Get<IGameEngine>();
            }
        }

        static Program()
        {
            MakeBindings();
        }

        static void MakeBindings()
        {
            K.C.Bind<GameplayTrace>().ToSelf().InSingletonScope();
            K.C.Bind<Dispatcher>().ToMethod(x => Application.Current.Dispatcher);
            K.C.Bind<Client>().ToSelf().InSingletonScope();
            K.C.Bind<PlayerStateMachine>().ToSelf().InSingletonScope();
            K.C.Bind<ControllableObjectStateMachine>().ToSelf().InSingletonScope();
            K.C.Bind<IGameEngine>().To<GameEngine>().InSingletonScope();
            K.C.Bind<GroupStateMachine>().ToSelf().InSingletonScope();
            K.C.Bind<CardStateMachine>().ToSelf().InSingletonScope();

        }
    }
}