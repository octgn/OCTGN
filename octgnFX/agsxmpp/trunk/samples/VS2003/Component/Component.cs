using System;

using agsXMPP;

namespace Component
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Component
	{
		private static XmppComponentConnection comp = null;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            // create a new component connection to the given serverdomain, port and password
            // a password is needed for the most servers for security reasons, if your server runs behind
            // a firewall or a closed company network then you could allow component connections without password.
			comp = new XmppComponentConnection("localhost", 5555, "secret");

            // The Jid which this component will use
			comp.ComponentJid = new Jid("test.gnauck.dyndns.org");

            // Setup event handlers
			comp.OnLogin	+= new ObjectHandler(comp_OnLogin);
			comp.OnClose	+= new ObjectHandler(comp_OnClose);
			comp.OnRoute	+= new agsXMPP.XmppComponentConnection.RouteHandler(comp_OnRoute);			
			comp.OnReadXml	+= new XmlHandler(comp_OnReadXml);
			comp.OnWriteXml	+= new XmlHandler(comp_OnWriteXml);			

			comp.Open();

			Console.ReadLine();
			comp.Close();
			
			Console.ReadLine();
			
			Console.WriteLine("And open the Connection again");
			comp.Open();
			Console.ReadLine();

			comp.Close();
			
			Console.ReadLine();
			
		}

		private static void comp_OnClose(object sender)
		{
			Console.WriteLine("OnClose\r\n");
		}

		private static void comp_OnLogin(object sender)
		{
			Console.WriteLine("OnLogin\r\n");
		}

		private static void comp_OnRoute(object sender, agsXMPP.protocol.component.Route r)
		{

		}

		private static void comp_OnReadXml(object sender, string xml)
		{
			Console.WriteLine("OnReadXml: " + xml + "\r\n");
		}

		private static void comp_OnWriteXml(object sender, string xml)
		{
			Console.WriteLine("OnWriteXml: " + xml + "\r\n");
		}
	}
}
