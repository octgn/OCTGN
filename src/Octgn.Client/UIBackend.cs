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
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Microsoft.AspNet.SignalR;

namespace Octgn.Client
{
    public class UIBackend : IDisposable
    {
        //internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public string WebFolderPath { get; private set; }
        public string Path { get; private set; }
        public int Port { get; private set; }
        public int SignalRPort { get; private set; }
        protected IDisposable WebHost { get; set; }
        protected IDisposable SignalrHost { get; set; }

        public void Start(string pathToWebFolder)
        {
            Port = FreeTcpPort();
            SignalRPort = FreeTcpPort();
            Path = String.Format("http://localhost:{0}/", Port);
            WebFolderPath = pathToWebFolder;
            WebHost = WebApp.Start(Path, x =>
            {
                x.Use<Middleware>(x);
                x.UseNancy(op =>
                {
                    op.Bootstrapper = new Bootstrapper(this);
                }).MaxConcurrentRequests(1);
            });
            SignalrHost = WebApp.Start(String.Format("http://localhost:{0}/", SignalRPort), x =>
            {
                x.Use<Middleware>(x);
                x.UseCors(CorsOptions.AllowAll);
                x.MapSignalR();
                GlobalHost.HubPipeline.AddModule(new Modules.CallerCulturePipelineModule());
            });
        }

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
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