using System;

using agsXMPP;
using agsXMPP.Xml.Dom;

namespace MiniClient.Settings
{
    /*    
    This class shows how agsXMPP could also be used read and write custom xml files.
    Here we use it for the application settings which are stored in xml files    
    */
    public class Login : Element
    {
        public Login()
        {
            this.TagName    = "Login";
            this.Namespace  = null;
        }

        public Jid Jid
        {
            get { return GetTagJid("Jid"); }
            set { SetTag("Jid", value); }
        }

        public string Password
        {
            get { return GetTag("Password"); }
            set { SetTag("Password", value); }
        }

        public string Resource
        {
            get { return GetTag("Resource"); }
            set { SetTag("Resource", value); }
        }

        public int Priority
        {
            get { return GetTagInt("Priority"); }
            set { SetTag("Priority", value); }
        }

        public int Port
        {
            get { return GetTagInt("Port"); }
            set { SetTag("Port", value); }
        }

        public bool Ssl
        {
            get { return GetTagBool("Ssl"); }
            set { SetTag("Ssl", value); }
        }
    }
}