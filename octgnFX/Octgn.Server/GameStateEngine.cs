namespace Octgn.Server
{
    using Octgn.Online.Library;

    public class GameStateEngine
    {
        private static IGameStateEngine Context;
        private static readonly object Locker = new object();
        public static IGameStateEngine Get()
        {
            lock (Locker)
            {
                return Context;
            }
        }

        public static void Set(IGameStateEngine engine)
        {
            lock (Locker)
            {
                Context = engine;
            }
        }
    }
}