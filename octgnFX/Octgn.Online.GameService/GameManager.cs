namespace Octgn.Online.GameService
{
    using System.Configuration;
    using System.Reflection;

    using log4net;
    using Microsoft.Owin.Hosting;
using Owin;

    public class GameManager
    {
        #region singleton
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static GameManager current;
        private static readonly object Locker = new object();
        public static GameManager GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new GameManager());
            }
        }
        #endregion

        protected GameManager()
        {
            Log.Info("Creating");
            Log.Info("Created");
        }

        public void Start()
        {
            Log.Info("Starting");
            WebApplication.Start(ConfigurationManager.AppSettings["GameManagerHost"]);
            Log.Info("Started");
        }

        public void Stop()
        {
            Log.Info("Stopping");
            Log.Error("Stop mechanism not implemented");
            Log.Info("Stopped");
        }
    }
}
