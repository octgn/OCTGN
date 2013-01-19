using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using agsXMPP;

namespace WebClient
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void cmdLogin_Click(object sender, EventArgs e)
        {
            XmppClientConnection xmpp;

            xmpp = (XmppClientConnection)Application["xmpp"];
            if (xmpp == null)
            {
                xmpp = new XmppClientConnection();
                Application["xmpp"] = xmpp;                
            }

            xmpp.OnLogin += new ObjectHandler(xmpp_OnLogin);
            xmpp.AutoPresence = true;
            
            xmpp.AutoResolveConnectServer   = true;
            xmpp.Port                       = 5222;           
            xmpp.UseSSL                     = false;
            xmpp.Server                     = "jabber.org";
            xmpp.Username                   = "testuser";
            xmpp.Password                   = "your password";
            
            xmpp.Open();
        }

        void xmpp_OnLogin(object sender)
        {
            
        }

        protected void cmdLogout_Click(object sender, EventArgs e)
        {
            XmppClientConnection xmpp = (XmppClientConnection)Application["xmpp"];
            xmpp.Close();
        }
    }
}