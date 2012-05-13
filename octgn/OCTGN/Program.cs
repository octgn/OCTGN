using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Mono.WebServer;

namespace OCTGN
{
	static class Program
	{
		static void Main(string[] args)
		{
			int Port = 8080;
#if(DEBUG)
			string path = "../../../Octgn.App/";
#endif
			XSPWebSource websource = new XSPWebSource(IPAddress.Any, Port);
			ApplicationServer WebAppServer = new ApplicationServer(websource);
			//"[[hostname:]port:]VPath:realpath"
			string cmdLine = "localhost:" + Port + ":/:" + path;
			WebAppServer.AddApplicationsFromCommandLine(cmdLine);
			WebAppServer.Start(true);
			Console.WriteLine("Mono.WebServer running. Press enter to exit...");
			Console.ReadLine();
			WebAppServer.Stop();
		}
	}
}
