using System;
using System.Collections;
using System.Collections.Generic;

using agsXMPP;
using agsXMPP.protocol.client;

namespace MiniClient
{
	/// <summary>
	/// Summary description for Util.
	/// </summary>
	public class Util
	{
		public static Hashtable	ChatForms       = new Hashtable();
        public static Hashtable GroupChatForms  = new Hashtable();

        public class XmppServices
        {
            public XmppServices()
            {
            }

            public List<Jid> Search = new List<Jid>();
            public List<Jid> Proxy = new List<Jid>();
        }

        public static XmppServices Services = new XmppServices();


        public static string AppPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
        }

        public static int GetRosterImageIndex(Presence pres)
        {
            if (pres.Type == PresenceType.unavailable)
            {
                return 0;
            }
            else if (pres.Type == PresenceType.error)
            {
                // presence error, we dont care in the miniclient here
            }
            else
            {
                switch (pres.Show)
                {
                    case ShowType.NONE:
                        return 1;
                    case ShowType.away:
                        return 2;
                    case ShowType.chat:
                        return 4;
                    case ShowType.xa:
                        return 3;
                    case ShowType.dnd:
                        return 5;
                }
            }
            return 0;
        }
	}   
}
