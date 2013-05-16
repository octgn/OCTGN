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
using System.Text;

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.bytestreams
{
    /*
        <streamhost 
            jid='proxy.host3' 
            host='24.24.24.1' 
            zeroconf='_jabber.bytestreams'/>
        <xs:element name='streamhost'>
            <xs:complexType>
              <xs:simpleContent>
                <xs:extension base='empty'>
                  <xs:attribute name='jid' type='xs:string' use='required'/>
                  <xs:attribute name='host' type='xs:string' use='required'/>
                  <xs:attribute name='zeroconf' type='xs:string' use='optional'/>
                  <xs:attribute name='port' type='xs:string' use='optional'/>
                </xs:extension>
              </xs:simpleContent>
            </xs:complexType>
        </xs:element>
    */
    public class StreamHost : Element
    {
        public StreamHost()
        {
            this.TagName    = "streamhost";
            this.Namespace  = Uri.BYTESTREAMS;
        }

        public StreamHost(Jid jid, string host) : this()
        {
            Jid     = jid;
            Host    = host;
        }

        public StreamHost(Jid jid, string host, int port) : this(jid, host)            
        {            
            Port    = port;
        }

        public StreamHost(Jid jid, string host, int port, string zeroconf) : this(jid, host, port)
        {
            Zeroconf = zeroconf;
        }

        /// <summary>
        /// a port associated with the hostname or IP address for SOCKS5 communications over TCP
        /// </summary>
        public int Port
        {
            get { return GetAttributeInt("port"); }
            set { SetAttribute("port", value); }
        }

        /// <summary>
        /// the hostname or IP address of the StreamHost for SOCKS5 communications over TCP
        /// </summary>
        public string Host
        {
            get { return GetAttribute("host"); }
            set { SetAttribute("host", value); }
        }
        
        /// <summary>
        /// The XMPP/Jabber id of the streamhost
        /// </summary>
        public Jid Jid
        {
            get
            {
                if (HasAttribute("jid"))
                    return new Jid(this.GetAttribute("jid"));
                else
                    return null;
            }
            set
            {
                if (value != null)
                    this.SetAttribute("jid", value.ToString());
                else
                    RemoveAttribute("jid");
            }
        }

        /// <summary>
        /// a zeroconf [5] identifier to which an entity may connect, for which the service identifier and 
        /// protocol name SHOULD be "_jabber.bytestreams".
        /// </summary>
        public string Zeroconf
        {
            get { return GetAttribute("zeroconf"); }
            set { SetAttribute("zeroconf", value); }
        }
    }
}
