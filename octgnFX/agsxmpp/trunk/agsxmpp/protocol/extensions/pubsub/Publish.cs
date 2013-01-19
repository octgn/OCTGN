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

namespace agsXMPP.protocol.extensions.pubsub
{
    /*
     Example 9. Entity publishes an item with an ItemID of the Payload Type UserTune

        <iq type="set"
            from="pgm@jabber.org"
            to="pubsub.jabber.org"
            id="publish1">
          <pubsub xmlns="http://jabber.org/protocol/pubsub">
            <publish node="generic/pgm-mp3-player">
              <item id="current">
                <tune xmlns="http://jabber.org/protocol/tune">
                  <artist>Ralph Vaughan Williams</artist>
                  <title>Concerto in F for Bass Tuba</title>
                  <source>Golden Brass: The Collector's Edition</source>
                </tune>
              </item>
            </publish>
          </pubsub>
        </iq>
     
    */

    public class Publish : Element
    {
        #region << Constructors >>
        public Publish()
        {
            this.TagName    = "publish";
            this.Namespace  = Uri.PUBSUB;
        }

        /// <summary>
        /// Its recommended to use this constructor because a node is required
        /// </summary>
        /// <param name="node">Node to publish</param>
        public Publish(string node) : this()
        {
            this.Node = node;
        }

        public Publish(string node, Item item) : this(node)
        {
            this.AddItem(item);
        }
        #endregion

        /// <summary>
        /// The node to publish to. This Property is required
        /// </summary>
        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        /// <summary>
        /// Add a payload Item
        /// </summary>
        /// <returns>returns the added Item</returns>
        public Item AddItem()
        {
            Item item = new Item();
            AddChild(item);
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns>returns the added item</returns>
        public Item AddItem(Item item)
        {
            AddChild(item);
            return item;
        }

        /// <summary>
        /// This will return all payload items. Multiple items are possible, but doe the most implementaions one item 
        /// should be enough
        /// </summary>
        /// <returns>returns an Array of Items</returns>
        public Item[] GetItems()
        {
            ElementList nl = SelectElements(typeof(Item));
            Item[] items = new Item[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Item)e;
                i++;
            }
            return items;
        }
    }
}
