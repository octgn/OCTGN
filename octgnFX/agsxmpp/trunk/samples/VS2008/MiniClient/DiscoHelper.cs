using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.disco;

namespace MiniClient
{

    public enum ClientCategory
    {
        /// <summary>
        /// An automated client that is not controlled by a human user
        /// </summary>
        bot,

        /// <summary>
        /// Minimal non-GUI client used on dumb terminals or text-only screens
        /// </summary>
        console,

        /// <summary>
        /// A client running on a PDA, RIM device, or other handheld
        /// </summary>
        handheld,

        /// <summary>
        /// Standard full-GUI client used on desktops and laptops
        /// </summary>
        pc,

        /// <summary>
        /// A client running on a mobile phone or other telephony device
        /// </summary>
        phone,

        /// <summary>
        /// 	A client operated from within a web browser
        /// </summary>
        web
    }

    class DiscoHelper
    {
        
        private XmppClientConnection xmppCon;
        private ClientCategory m_ClientCategory = ClientCategory.pc;
        private string m_ClientName = null;
        private StringCollection m_Features = new StringCollection();

        public DiscoHelper(XmppClientConnection con)
        {
            xmppCon = con;
            con.OnIq += new IqHandler(con_OnIq);

            m_Features.Add(agsXMPP.Uri.DISCO_INFO);
        }

        public ClientCategory ClientCategory
        {
            get { return m_ClientCategory; }
            set { m_ClientCategory = value; }
        }

        public string ClientName
        {
            get { return m_ClientName; }
            set { m_ClientName = value; }
        }

        public StringCollection ClientFeatures
        {
            get { return m_Features; }
        }

        private void con_OnIq(object sender, IQ iq)
        {
            if (iq.Query != null)
            {
                if (iq.Query is DiscoInfo && iq.Type == IqType.get)
                {
                    /*
                    <iq type='get'
                        from='romeo@montague.net/orchard'
                        to='plays.shakespeare.lit'
                        id='info1'>
                      <query xmlns='http://jabber.org/protocol/disco#info'/>
                    </iq>
                    */
                    iq.SwitchDirection();
                    iq.Type = IqType.result;

                    DiscoInfo di = iq.Query as DiscoInfo;

                    if (ClientName != null)
                        di.AddIdentity(new DiscoIdentity(ClientCategory.ToString(), this.ClientName, "client"));

                    foreach (string feature in ClientFeatures)
                    {
                        di.AddFeature(new DiscoFeature(feature));
                    }

                    xmppCon.Send(iq);

                }
            }
        }
    }
}
