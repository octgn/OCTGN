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
        <xs:element name='items'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='item' minOccurs='0' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='max_items' type='xs:positiveInteger' use='optional'/>
              <xs:attribute name='node' type='xs:string' use='required'/>
              <xs:attribute name='subid' type='xs:string' use='optional'/>
            </xs:complexType>
        </xs:element>
     
        <iq type='get'
            from='francisco@denmark.lit/barracks'
            to='pubsub.shakespeare.lit'
            id='items1'>
          <pubsub xmlns='http://jabber.org/protocol/pubsub'>
            <items node='blogs/princely_musings'/>
          </pubsub>
        </iq>
    */
    public class Items : Publish
    {
        #region << Constructors >>
        public Items() : base()
        {
            this.TagName    = "items";            
        }

        public Items(string node) : this()
        {
            this.Node = node;
        }

        public Items(string node, string subId) : this(node)
        {
            this.SubId = subId;
        }

        public Items(string node, string subId, int maxItems) : this(node, subId)
        {
            this.MaxItems = maxItems;
        }
        #endregion

        //public string Node
        //{
        //    get { return GetAttribute("node"); }
        //    set { SetAttribute("node", value); }
        //}

        public string SubId
        {
            get { return GetAttribute("subid"); }
            set { SetAttribute("subid", value); }
        }

        public int MaxItems
        {
            get { return GetAttributeInt("max_items"); }
            set { SetAttribute("max_items", value); }
        }
    }
}
