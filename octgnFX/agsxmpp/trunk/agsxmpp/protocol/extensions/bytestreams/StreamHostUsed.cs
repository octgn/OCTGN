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
        <iq type='result' 
            from='target@host2/bar' 
            to='initiator@host1/foo' 
            id='initiate'>
          <query xmlns='http://jabber.org/protocol/bytestreams'>
            <streamhost-used jid='proxy.host3'/>
          </query>
        </iq>
    */

    /// <summary>
    /// The <streamhost-used/> element indicates the StreamHost connected to. 
    /// This element has a single attribute for the JID of the StreamHost to which the Target connected. 
    /// This element MUST NOT contain any content node.
    /// The "jid" attribute specifies the full JID of the StreamHost. 
    /// This attribute MUST be present, and MUST be a valid JID for use with an &lt;iq/&gt;.
    /// </summary>
    public class StreamHostUsed : Element
    {
        public StreamHostUsed()
        {
            this.TagName    = "streamhost-used";
            this.Namespace  = Uri.BYTESTREAMS;
        }
        
        public StreamHostUsed(Jid jid) : this()
        {
            Jid = jid;
        }        

        /// <summary>
        /// Jid of the streamhost
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
				if (value!=null)
					this.SetAttribute("jid", value.ToString());
                else
                    RemoveAttribute("jid");
			}
		}
        
    }
}
