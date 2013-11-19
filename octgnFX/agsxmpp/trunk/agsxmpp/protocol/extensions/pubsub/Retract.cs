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
    // Publish and retract looks exactly the same, so inherit from publish here
    public class Retract : Publish
    {

        /*
            A service SHOULD allow a publisher to delete an item once it has been published to a node that 
            supports persistent items.
            To delete an item, the publisher sends a retract request as shown in the following examples. 
            The <retract/> element MUST possess a 'node' attribute and SHOULD contain one <item/> element
            (but MAY contain more than one <item/> element for Batch Processing of item retractions); 
            the <item/> element MUST be empty and MUST possess an 'id' attribute.
            
            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="deleteitem1">
              <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <retract node="generic/pgm-mp3-player">
                  <item id="current"/>
                </retract>
              </pubsub>
            </iq>
        */

        public Retract() : base()
        {
            this.TagName    = "retract";            
        }

        public Retract(string node) : this()
        {
            this.Node = node;
        }

        public Retract(string node, string id) : this(node)
        {
            this.AddItem(new Item(id));
        }
        
    }
}
