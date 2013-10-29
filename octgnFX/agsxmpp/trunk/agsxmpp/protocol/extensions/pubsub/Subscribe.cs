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

namespace agsXMPP.protocol.extensions.pubsub
{
    public class Subscribe : Element
    {
        /*
        Example 25. Entity subscribes to a node

        <iq type="set"
            from="sub1@foo.com/home"
            to="pubsub.jabber.org"
            id="sub1">
          <pubsub xmlns="http://jabber.org/protocol/pubsub">
            <subscribe
                node="generic/pgm-mp3-player"
                jid="sub1@foo.com"/>
          </pubsub>
        </iq>
        */
        #region << Constructors >>
        public Subscribe()
        {
            this.TagName    = "subscribe";
            this.Namespace  = Uri.PUBSUB;
        }

        public Subscribe(string node, Jid jid) : this()
        {
            this.Node   = node;
            this.Jid    = jid;
        }
        #endregion

        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

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
    }
}
