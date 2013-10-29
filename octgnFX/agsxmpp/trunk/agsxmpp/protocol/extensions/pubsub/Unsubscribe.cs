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

namespace agsXMPP.protocol.extensions.pubsub
{
    /*
        
        Example 38. Entity unsubscribes from a node

        <iq type='set'
            from='francisco@denmark.lit/barracks'
            to='pubsub.shakespeare.lit'
            id='unsub1'>
          <pubsub xmlns='http://jabber.org/protocol/pubsub'>
             <unsubscribe
                 node='blogs/princely_musings'
                 jid='francisco@denmark.lit'/>
          </pubsub>
        </iq>
    
    */

    // looks exactly the same as subscribe, but has an additional Attribute subid

    public class Unsubscribe : Subscribe
    {
        #region << Constructors >>
        public Unsubscribe() : base()
        {
            this.TagName = "unsubscribe";
        }

        public Unsubscribe(string node, Jid jid) : this()
        {
            this.Node   = node;
            this.Jid    = jid;
        }

        public Unsubscribe(string node, Jid jid, string subid)
            : this(node, jid)
        {
            SubId = subid;
        }
        #endregion

        public string SubId
        {
            get { return GetAttribute("subid"); }
            set { SetAttribute("subid", value); }
        }        

    }
}
