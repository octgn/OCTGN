using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.Logging;
using Owin;
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Windows;

namespace Octgn.Client
{
    public class Server : IDisposable
    {
        //internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string WebFolderPath { get; private set; }
        public string Path { get; private set; }
        protected IDisposable WebHost { get; set; }
        protected IDisposable SignalrHost { get; set; }

        public void Start(string pathToWebFolder)
        {
            Path = "http://localhost:9000/";
            WebFolderPath = pathToWebFolder;
            WebHost = WebApp.Start(Path, x =>
            {
                x.Use<Middleware>(x);
                x.UseNancy(op =>
                {
                    op.Bootstrapper = new Bootstrapper();
                });
            });
            SignalrHost = WebApp.Start("http://localhost:9001/", x =>
            {
                x.Use<Middleware>(x);
                x.UseCors(CorsOptions.AllowAll);
                x.MapSignalR();
            });
        }

        public void Dispose()
        {
            WebHost.Dispose();
            SignalrHost.Dispose();
        }

        public void PingClients()
        {
            MainHub.Instance.Clients.All.Ping();
        }

        private class Middleware : OwinMiddleware
        {
            private readonly ILogger _logger;

            public Middleware(OwinMiddleware next, IAppBuilder app)
                : base(next)
            {
                _logger = app.CreateLogger<Middleware>();
            }

            public async override Task Invoke(IOwinContext context)
            {
                _logger.WriteVerbose(
                    string.Format("{0} {1}: {2}",
                    context.Request.Scheme,
                    context.Request.Method,
                    context.Request.Path));
                
                await Next.Invoke(context);
            }
        }
    }
}