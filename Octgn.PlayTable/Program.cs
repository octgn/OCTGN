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

        public static IObjectCreator Creator
        {
            get
            {
                return K.C.Get<IObjectCreator>();
            }
        }

        static Program()
        {
            MakeBindings();
        }

        private static void MakeBindings()
        {
            K.C.Bind<GameplayTrace>().ToSelf().InSingletonScope();
            K.C.Bind<Dispatcher>().ToMethod(x => Application.Current.Dispatcher);
            K.C.Bind<Client>().ToSelf().InSingletonScope();
            K.C.Bind<IGameEngine>().To<GameEngine>().InSingletonScope();
            K.C.Bind<GameStateMachine>().ToSelf().InSingletonScope();
            K.C.Bind<IObjectCreator>().To<ObjectCreator>().InSingletonScope();
        }
    }

}