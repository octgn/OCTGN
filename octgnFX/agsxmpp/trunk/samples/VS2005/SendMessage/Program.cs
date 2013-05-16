using System;
using System.Collections.Generic;
using System.Text;

using agsXMPP;
using agsXMPP.protocol.client;

namespace SendMessage
{
    class Program
    {
        static void Main(string[] args)
        {           
            const string JID_SENDER     = "user@jabber.org";
            const string PASSWORD       = "secret";   // password of the JIS_SENDER account

            const string JID_RECEIVER   = "test@myserver.com";

            Jid jidSender = new Jid(JID_SENDER);
            XmppClientConnection xmpp = new  XmppClientConnection(jidSender.Server);
            xmpp.Open(jidSender.User, PASSWORD);            
            xmpp.OnLogin += delegate(object o) { xmpp.Send(new Message(new Jid(JID_RECEIVER), MessageType.chat, "Hello, how are you?")); };

            Console.WriteLine("Wait until you get the message and press a key to continue");
            Console.ReadLine();
            
            xmpp.Close();
        }
    }
}
