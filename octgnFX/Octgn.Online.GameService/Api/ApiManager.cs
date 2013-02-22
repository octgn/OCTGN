namespace Octgn.Online.GameService.Api
{
    using System.Configuration;
    using System.Net.Http.Formatting;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using System.Web.Http.SelfHost;

    using log4net;

    public class ApiManager
    {
        internal HttpSelfHostConfiguration Config;
        internal HttpSelfHostServer Server;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static ApiManager current;
        private static readonly object Locker = new object();
        public static ApiManager GetContext()
        {
            lock (Locker)
            {
                return current ?? (current = new ApiManager());
            }
        }

        protected ApiManager()
        {
            Log.Info("Initializing");
            string hostString = "http://localhost:" + ConfigurationManager.AppSettings["ListenPort"];
            Log.Info("Host String: " + hostString);
            Config = new HttpSelfHostConfiguration(hostString);
            Config.Routes.MapHttpRoute(
                name: "Default",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            //Config.Routes.Add("Default",new HttpRoute("api/{controller}/{action}"));
            Config.Formatters.JsonFormatter.MediaTypeMappings.Add(new QueryStringMapping("json", "true", "application/json"));
            Log.Info("Stopped");
        }

        public void Start()
        {
            Log.Info("Starting");
            Server = new HttpSelfHostServer(Config);
            Server.OpenAsync().Wait();
            Log.Info("Started");
        }

        public void Stop()
        {
            Log.Info("Stopping");
            Server.CloseAsync().Wait();
            Server.Dispose();
            lock(Locker)
                current = null;
            Log.Info("Stopped");
        }
    }
}
