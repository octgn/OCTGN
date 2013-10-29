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
      <xs:element name='item'>
        <xs:complexType>
          <xs:sequence minOccurs='0'>
            <xs:any namespace='##other'/>
          </xs:sequence>
          <xs:attribute name='id' type='xs:string' use='optional'/>
        </xs:complexType>
      </xs:element>
    */
    public class Item : Element
    {
        public Item()
        {
            this.TagName    = "item";
            this.Namespace  = Uri.PUBSUB;
        }

        public Item(string id) : this()
        {
            this.Id = id;
        }

        /// <summary>
        /// The optional id
        /// </summary>
        public string Id
        {
            get { return GetAttribute("id"); }
            set { SetAttribute("id", value); }
        }
    }
}