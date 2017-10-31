using Octgn.Communication;
using System.Net;
using System.ServiceProcess;
using Octgn.Communication.Serializers;
using Octgn.Authenticators;
using Octgn.Online.Hosting;

namespace Octgn
{
    partial class Service: ServiceBase
    {
        public Server Server => _server;

        private readonly int _port;
        private readonly Server _server;
        public Service(IPAddress hostIp, int port, string gameServerUserId)
        {
            InitializeComponent();
            _port = port;
            var endpoint = new IPEndPoint(hostIp, _port);
            _server = new Server(new TcpListener(endpoint), new ConnectionProvider(), new XmlSerializer(), new SessionAuthenticationHandler());
            _server.Attach(new ServerHostingModule(_server, gameServerUserId));
        }

        public void Start() {
            _server.IsEnabled = true;
        }

        protected override void OnStart(string[] args)
        {
            _server.IsEnabled = true;
        }

        protected override void OnStop()
        {
            _server.IsEnabled = false;
        }
    }
}
