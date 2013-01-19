/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * Copyright (c) 2003-2012 by AG-Software 											 *
 * All Rights Reserved.																 *
 * Contact information for AG-Software is available at http://www.ag-software.de	 *
 *																					 *
 * Licence:																			 *
 * The agsXMPP SDK is released under a dual licence									 *
 * agsXMPP can be used under either of two licences									 *
 * 																					 *
 * A commercial licence which is probably the most appropriate for commercial 		 *
 * corporate use and closed source projects. 										 *
 *																					 *
 * The GNU Public License (GPL) is probably most appropriate for inclusion in		 *
 * other open source projects.														 *
 *																					 *
 * See README.html for details.														 *
 *																					 *
 * For general enquiries visit our website at:										 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.bosh
{
    
    public class Body : Element
    {
        public Body()
        {
            this.TagName = "body";
            this.Namespace = Uri.HTTP_BIND;
        }

        /*
        POST /webclient HTTP/1.1
        Host: httpcm.jabber.org
        Accept-Encoding: gzip, deflate
        Content-Type: text/xml; charset=utf-8
        Content-Length: 153

        <body rid='1249243564'
              sid='SomeSID'
              type='terminate'
              xmlns='http://jabber.org/protocol/httpbind'>
          <presence type='unavailable'
                    xmlns='jabber:client'/>
        </body>
        
        HTTP/1.1 200 OK
        Content-Type: text/xml; charset=utf-8
        Content-Length: 128

        <body authid='ServerStreamID'
              wait='60'
              inactivity='30'
              polling='5'
              requests='2'
              accept='deflate,gzip'
              sid='SomeSID'
              secure='true'
              stream='firstStreamName'
              charsets='ISO_8859-1 ISO-2022-JP'
              xmlns='http://jabber.org/protocol/httpbind'/>
        */
        
        public string Sid
        {
            get { return GetAttribute("sid"); }
            set { SetAttribute("sid", value); }
        }
         
        public long Rid
        {
            get { return GetAttributeLong("rid"); }
            set { SetAttribute("rid", value); }
        }

        public long Ack
        {
            get { return GetAttributeLong("ack"); }
            set { SetAttribute("ack", value); }
        }

        public bool Secure
        {
            get { return GetAttributeBool("secure"); }
            set { SetAttribute("secure", value); }
        }

        /// <summary>
        /// Specifies the longest time (in seconds) that the connection manager is allowed to wait before responding to any request 
        /// during the session. This enables the client to limit the delay before it discovers any network failure, 
        /// and to prevent its HTTP/TCP connection from expiring due to inactivity.
        /// </summary>
        public int Wait
        {
            get { return GetAttributeInt("wait"); }
            set { SetAttribute("wait", value); }
        }

        /// <summary>
        /// If the connection manager supports session pausing (inactivity) then it SHOULD advertise that to the client by including a 'maxpause'
        /// attribute in the session creation response element. The value of the attribute indicates the maximum length of a temporary 
        /// session pause (in seconds) that a client MAY request.
        /// </summary>
        public int MaxPause
        {
            get { return GetAttributeInt("maxpause"); }
            set { SetAttribute("maxpause", value); }
        }

        public int Inactivity
        {
            get { return GetAttributeInt("inactivity"); }
            set { SetAttribute("inactivity", value); }
        }

        public int Polling
        {
            get { return GetAttributeInt("polling"); }
            set { SetAttribute("polling", value); }
        }

        public int Requests
        {
            get { return GetAttributeInt("requests"); }
            set { SetAttribute("requests", value); }
        }

        /// <summary>
        /// Specifies the target domain of the first stream.
        /// </summary>
        public Jid To
        {
            get { return GetAttributeJid("to"); }
            set { SetAttribute("to", value); }
        }

        public Jid From
        {
            get { return GetAttributeJid("from"); }
            set { SetAttribute("from", value); }
        }

        /// <summary>
        /// specifies the maximum number of requests the connection manager is allowed to keep waiting at any one time during the session. 
        /// If the client is not able to use HTTP Pipelining then this SHOULD be set to "1".
        /// </summary>
        public int Hold
        {
            get { return GetAttributeInt("hold"); }
            set { SetAttribute("hold", value); }
        }             
        
        /// <summary>
        /// <para>
        /// Specifies the highest version of the BOSH protocol that the client supports. 
        /// The numbering scheme is "<major>.<minor>" (where the minor number MAY be incremented higher than a single digit, 
        /// so it MUST be treated as a separate integer).
        /// </para>
        /// <remarks>
        /// The 'ver' attribute should not be confused with the version of any protocol being transported.
        /// </remarks>
        /// </summary>
        public string Version
        {
            get { return GetAttribute("ver"); }
            set { SetAttribute("ver", value); }
        }

        public string NewKey
        {
            get { return GetAttribute("newkey"); }
            set { SetAttribute("newkey", value); }
        }

        public string Key
        {
            get { return GetAttribute("key"); }
            set { SetAttribute("key", value); }
        }

        public BoshType Type
        {
            get { return (BoshType) GetAttributeEnum("type", typeof(BoshType)); }
            set
            {
                if (value == BoshType.NONE)
                    RemoveAttribute("type");
                else
                    SetAttribute("type", value.ToString());
            }
        }

        public string XmppVersion
        {
            get { return GetAttribute("xmpp:version"); }
            set 
            {
                AddBoshNamespace();
                SetAttribute("xmpp:version", value); 
            }
        }

        public bool XmppRestart
        {
            get { return GetAttributeBool("xmpp:restart"); }
            set
            {
                AddBoshNamespace();
                SetAttribute("xmpp:restart", value); 
            }
        }

        internal void AddBoshNamespace()
        {
            this.SetAttribute("xmlns:xmpp", "urn:xmpp:xbosh"); 
        }
    }
}