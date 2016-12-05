using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NuGet;

namespace Octgn.Test.Utils
{
	public class HttpEcho : IDisposable
	{
		public int Port { get; private set; }
		public string Url { get; private set; }

		public Action<HttpListenerContext> Response { get; set; } = (x) => {
			x.Response.StatusCode = 200;
			using (var sw = new StreamWriter(x.Response.OutputStream)) {
				sw.Write("200 OK");
			}
		};

		private readonly HttpListener _listener;

		public HttpEcho()
		{
			Port = FreeTcpPort();
			_listener = new HttpListener();
			Url = "http://localhost:" + Port + "/";
			_listener.Prefixes.Add(Url);

			_listener.Start();

			Task.Factory.StartNew(() => {
				while (_listener.IsListening) {
					var context = _listener.GetContext();
					Response.Invoke(context);
				}
			}, TaskCreationOptions.LongRunning);
		}

		private static int FreeTcpPort()
		{
			var l = new TcpListener(IPAddress.Loopback, 0);
			l.Start();
			var port = ((IPEndPoint)l.LocalEndpoint).Port;
			l.Stop();
			return port;
		}

		public void Dispose()
		{
			try {
				_listener?.Close();
			} catch { }
		}
	}
}
