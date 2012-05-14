using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Octgn
{
	public class WebHoster
	{
		public int Port { get; private set; }
		public DirectoryInfo WebRoot { get; private set; }

		private static bool _keepAppRunning = true;
		private static HttpListener _listener;
		private static Thread _listeningThread = null;
		private static RemoteDomain _host = null;

		public WebHoster(int port, DirectoryInfo webRoot) 
		{ 
			Port = port;
			WebRoot = webRoot;
		}

		public void Start()
		{
			Console.WriteLine("Starting the new AppDomain to host our ASP.NET runtime system...");
			_host = RemoteDomain.InitialiseHost(WebRoot.FullName);

			Console.WriteLine("Starting the HttpListener on port:{0}", Port);

			InitialiseListener();

			_listeningThread = new Thread(ListeningThread);
			_listeningThread.Start();

			Console.WriteLine("Listener started on port: {0}, waiting for requests, press ANY KEY to quit.", Port);

			while (_keepAppRunning)
			{
				Thread.Sleep(100);
			}

			// Close down our thread and listener if required.
			if (_listeningThread.ThreadState == System.Threading.ThreadState.Running)
				_listeningThread.Abort();

			if (_listener.IsListening)
				_listener.Stop();
		}

		public void Stop() { _keepAppRunning = false; }

		private void InitialiseListener()
		{
			_listener = new HttpListener();

			_listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;

			string prefix = string.Format("http://+:{0}/", Port);
			_listener.Prefixes.Add(prefix);

			_listener.Start();
		}

		private void ListeningThread()
		{
			while (_keepAppRunning)
			{
				HttpListenerContext context = _listener.GetContext();

				#region Stopwatch start & console output

				Console.WriteLine("---Request Recieved----");

				// Yet another new V2 feature
				Stopwatch sw = new Stopwatch();
				sw.Reset();
				sw.Start();

				#endregion

				try
				{
					WriteRequestHeaderInformation(context);

					CreateResponseDocument(context);
				}
				catch (Exception ex)
				{
					#region Display any exception details
					ConsoleColor oldCol = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.WriteLine("\nThere was an error processing this request. See below for detals :-");
					Console.WriteLine(ex.Message);
					Console.WriteLine("\n");
					Console.ForegroundColor = oldCol;
					#endregion
				}

				Console.WriteLine("----Request processed in {0} milliseconds ----", sw.ElapsedMilliseconds);
			}
		}

		private void CreateResponseDocument(HttpListenerContext ctxt)
		{
			#region Check if its a file request and setup variables/flags

			bool isFileToProcess = true;
			string htmlOutput = null;

			// Check if its a .HTML or .HTM file
			if (ctxt.Request.Url.PathAndQuery.ToUpper().IndexOf(".DUMMY") >= 0)
			{
				isFileToProcess = false;
			}

			#endregion

			// Check if the user has request a html file, if so, serve it up,
			// otherwise, give them our simple response.
			if (isFileToProcess)
			{
				// Get our host to service the file request - going cross domain here
				var p = ctxt.Request.Url.LocalPath;
				if (p[0] == '/') p = p.Substring(1);
				var fpath = Path.Combine(WebRoot.FullName, p);
				if (File.Exists(fpath)) htmlOutput = File.ReadAllText(fpath);
				else htmlOutput = _host.ProcessRequest(p, ctxt.Request.Url.Query.Replace("?", ""));
			}
			else
			{
				// There was no file request, so we just return a simple
				// response document.
				htmlOutput = string.Format("<h1>Glavs SuperDuper Web Server - Baldy Edition</h1><p>You have hit the HttpListener demo at {0}</p>", DateTime.Now.ToString("hh:mm:ss"));
			}

			// Send the output, if any, to the response context for the listener
			if (htmlOutput != null && ctxt.Response.OutputStream.CanWrite)
			{
				byte[] htmlData = System.Text.UTF8Encoding.UTF8.GetBytes(htmlOutput);
				ctxt.Response.OutputStream.Write(htmlData, 0, htmlData.Length);
			}

			// Have we been asked to shutdown the server?
			if (ctxt.Request.QueryString.Count > 0 && ctxt.Request.QueryString["quit"] != null && ctxt.Request.QueryString["quit"].ToUpper() == "TRUE")
			{
				_keepAppRunning = false;
				byte[] finalOutput = Encoding.UTF8.GetBytes("<h2>The Server has been shutdown!</h2>");
				ctxt.Response.OutputStream.Write(finalOutput, 0, finalOutput.Length);
			}

			ctxt.Response.Close();
		}
		private void WriteRequestHeaderInformation(HttpListenerContext ctxt)
		{
			Console.WriteLine("Header Information:");
			ConsoleColor oldColour = Console.ForegroundColor;

			// Show all the headers in our request context
			foreach (string key in ctxt.Request.Headers.AllKeys)
			{
				Console.ForegroundColor = ConsoleColor.Green;

				Console.Write("[{0}]", key);

				Console.CursorLeft = 25;
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.Write(" : ");
				Console.ForegroundColor = ConsoleColor.Cyan;

				Console.WriteLine("[{0}]", ctxt.Request.Headers[key]);
			}
			Console.WriteLine("--");

			Console.ForegroundColor = ConsoleColor.DarkCyan;
			Console.Write("User Information");
			Console.CursorLeft = 25;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write(" : ");
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.CursorLeft = 28;

			// Show the user information
			if (ctxt.User != null)
			{
				Console.WriteLine("Type = [{0}]", ctxt.User);
				Console.CursorLeft = 28;
				Console.WriteLine("Name = [{0}]", ctxt.User.Identity.Name);
			}
			else
			{
				Console.WriteLine("No User defined");
			}
            GetInterface();
            string msg = iBase.GetMessage();
            Console.WriteLine("iBase value: {0}", iBase.GetMessage());

			Console.ForegroundColor = oldColour;
		}


        private IBaseInterface iBase = null;
        private IBaseInterface GetInterface()
        {
            if (iBase == null)
            {
                System.ServiceModel.ChannelFactory<IBaseInterface> pipeFactory =
        new System.ServiceModel.ChannelFactory<IBaseInterface>(
          new System.ServiceModel.NetNamedPipeBinding(),
          new System.ServiceModel.EndpointAddress(
            "net.pipe://localhost/PipeBase"));

                iBase = pipeFactory.CreateChannel();
            }

            return (iBase);
        }
	}
	public class RemoteDomain : MarshalByRefObject
	{
		public string ProcessRequest(string page, string query)
		{
			// Process the request and send back the string output
			using (System.IO.StringWriter sw = new System.IO.StringWriter())
			{
				HttpRuntime.ProcessRequest(new SimpleWorkerRequest(page, query, sw));
				return sw.ToString();
			}
		}


		public static RemoteDomain InitialiseHost(string webRoot)
		{
			RemoteDomain theHost = (RemoteDomain)ApplicationHost.CreateApplicationHost(typeof(RemoteDomain), "/", webRoot);
			return theHost;
		}
	}

}
