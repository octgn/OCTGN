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

namespace agsXMPP.protocol.extensions.bytestreams
{
    /*
        <iq type='set' 
           from='initiator@host1/foo' 
           to='proxy.host3' 
           id='activate'>
           <query xmlns='http://jabber.org/protocol/bytestreams' sid='mySID'>
               <activate>target@host2/bar</activate>
           </query>
        </iq>
     
      
        <xs:element name='query'>
           <xs:complexType>
             <xs:choice>
               <xs:element ref='streamhost' minOccurs='0' maxOccurs='unbounded'/>
               <xs:element ref='streamhost-used' minOccurs='0'/>
               <xs:element name='activate' type='empty' minOccurs='0'/>
             </xs:choice>
             <xs:attribute name='sid' type='xs:string' use='optional'/>
             <xs:attribute name='mode' use='optional' default='tcp'>
               <xs:simpleType>
                 <xs:restriction base='xs:NCName'>
                   <xs:enumeration value='tcp'/>
                   <xs:enumeration value='udp'/>
                 </xs:restriction>
               </xs:simpleType>
             </xs:attribute>
           </xs:complexType>
        </xs:element>
    */

    /// <summary>
	/// ByteStreams
	/// </summary>
	public class ByteStream : Element
	{
		public ByteStream()
		{
			this.TagName	= "query";
			this.Namespace	= Uri.BYTESTREAMS;
		}

        public string Sid
        {
            set
            {
                SetAttribute("sid", value);
            }
            get
            {
                return GetAttribute("sid");
            }
        }

        public Mode Mode
        {
            get { return (Mode) GetAttributeEnum("mode", typeof(Mode)); }
            set
            {
                if (value != Mode.NONE)
                    SetAttribute("mode", value.ToString());
                else
                    RemoveAttribute("mode");
            }
        }

        /// <summary>
        /// Add a StreamHost
        /// </summary>
        /// <returns></returns>
        public StreamHost AddStreamHost()
        {
            StreamHost sh = new StreamHost();
            AddChild(sh);
            return sh;
        }

        /// <summary>
        /// Add a StreamHost
        /// </summary>
        /// <param name="sh"></param>
        /// <returns></returns>
        public StreamHost AddStreamHost(StreamHost sh)
        {            
            AddChild(sh);
            return sh;
        }

        /// <summary>
        /// Add a StreamHost
        /// </summary>
        /// <param name="jid"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        public StreamHost AddStreamHost(Jid jid, string host)
        {
            StreamHost sh = new StreamHost(jid, host);
            AddChild(sh);
            return sh;
        }

        /// <summary>
        /// Add a StreamHost
        /// </summary>
        /// <param name="jid"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public StreamHost AddStreamHost(Jid jid, string host, int port)
        {
            StreamHost sh = new StreamHost(jid, host, port);
            AddChild(sh);
            return sh;
        }

        /// <summary>
        /// Add a StreamHost
        /// </summary>
        /// <param name="jid"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="zeroconf"></param>
        /// <returns></returns>
        public StreamHost AddStreamHost(Jid jid, string host, int port, string zeroconf)
        {
            StreamHost sh = new StreamHost(jid, host, port, zeroconf);
            AddChild(sh);
            return sh;
        }
        
        /// <summary>
        /// Get the list of streamhosts
        /// </summary>
        /// <returns></returns>
        public StreamHost[] GetStreamHosts()
        {
            ElementList nl = SelectElements(typeof(StreamHost));
            StreamHost[] hosts = new StreamHost[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                hosts[i] = (StreamHost) e;
                i++;
            }
            return hosts;
        }
        
        
        /// <summary>
        /// The activate Element
        /// </summary>
        public Activate Activate
        {
            get
            {
                return SelectSingleElement(typeof(Activate)) as Activate;

            }
            set
            {
                if (HasTag(typeof(Activate)))
                    RemoveTag(typeof(Activate));
                
                if (value != null)
                    this.AddChild(value);
            }
        }

        public StreamHostUsed StreamHostUsed
        {
            get
            {
                return SelectSingleElement(typeof(StreamHostUsed)) as StreamHostUsed;
            }
            set
            {
                if (HasTag(typeof(StreamHostUsed)))
                    RemoveTag(typeof(StreamHostUsed));
                
                if (value != null)
                    this.AddChild(value);
            }
        }
	}
}