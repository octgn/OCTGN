using System;

using agsXMPP;
using agsXMPP.protocol.component;

namespace Component
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class Component
	{
		private static XmppComponentConnection comp = null;

        private const int       PORT        = 5275;
        private const string    SECRET      = "secret";
        //private const string HOST = "localhost";
        private const string    HOST        = "vm-2k";
        private const string    JID         = "weather.vm-2k";
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
            // create a new component connection to the given serverdomain, port and password
            // a password is needed for the most servers for security reasons, if your server runs behind
            // a firewall or a closed company network then you could allow component connections without password.
            comp = new XmppComponentConnection(HOST, PORT, SECRET);

            // The Jid which this component will use
			comp.ComponentJid = new Jid(JID);

            // Setup event handlers
			comp.OnLogin	+= new ObjectHandler(comp_OnLogin);
			comp.OnClose	+= new ObjectHandler(comp_OnClose);
			comp.OnRoute	+= new agsXMPP.XmppComponentConnection.RouteHandler(comp_OnRoute);			
			comp.OnReadXml	+= new XmlHandler(comp_OnReadXml);
			comp.OnWriteXml	+= new XmlHandler(comp_OnWriteXml);

            comp.OnMessage  += new MessageHandler(comp_OnMessage);
            comp.OnIq       += new IqHandler(comp_OnIq);
            comp.OnPresence += new PresenceHandler(comp_OnPresence);

            comp.OnAuthError += new XmppElementHandler(comp_OnAuthError);

			comp.Open();

			Console.ReadLine();
			comp.Close();
			
            //////Console.ReadLine();
			
            //////Console.WriteLine("And open the Connection again");
            //////comp.Open();
            //////Console.ReadLine();

            //////comp.Close();
			
            //////Console.ReadLine();
			
		}

        static void comp_OnPresence(object sender, Presence pres)
        {
            Console.WriteLine("OnPresence\r\n" + pres.ToString());
        }

        static void comp_OnIq(object sender, IQ iq)
        {
            Console.WriteLine("OnIq\r\n" + iq.ToString());
        }

        static void comp_OnMessage(object sender, agsXMPP.protocol.component.Message msg)
        {
            Console.WriteLine("OnMessage\r\n" + msg.ToString());
        }

        static void comp_OnAuthError(object sender, agsXMPP.Xml.Dom.Element e)
        {
            Console.WriteLine("Authentication error ==> wrong Password\r\n");
        }

		private static void comp_OnClose(object sender)
		{
			Console.WriteLine("OnClose\r\n");
		}

		private static void comp_OnLogin(object sender)
		{
			Console.WriteLine("OnLogin\r\n");
           
            agsXMPP.protocol.component.Message msg = new agsXMPP.protocol.component.Message();
            msg.Type = agsXMPP.protocol.client.MessageType.chat;
            msg.Body = "Hello";
            msg.To = new Jid("alex@gnauck.dyndns.org");
            msg.From = comp.ComponentJid;
            //msg.To = new Jid("gnauck@myjabber.net");

            comp.Send(msg);
		}

		private static void comp_OnRoute(object sender, agsXMPP.protocol.component.Route r)
		{
            Console.WriteLine("OnRoute: " + r.ToString() + "\r\n");
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
