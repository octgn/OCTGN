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

namespace agsXMPP.protocol.extensions.pubsub.@event
{
    /*
        <message from='pubsub.shakespeare.lit' to='francisco@denmark.lit' id='foo'>
          <event xmlns='http://jabber.org/protocol/pubsub#event'>
            <items node='blogs/princely_musings'>
              <item id='ae890ac52d0df67ed7cfdf51b644e901'>
                <entry xmlns='http://www.w3.org/2005/Atom'>
                  <title>Soliloquy</title>
                  <summary>
                        To be, or not to be: that is the question:
                        Whether 'tis nobler in the mind to suffer
                        The slings and arrows of outrageous fortune,
                        Or to take arms against a sea of troubles,
                        And by opposing end them?
                  </summary>
                  <link rel='alternate' type='text/html' 
                        href='http://denmark.lit/2003/12/13/atom03'/>
                  <id>tag:denmark.lit,2003:entry-32397</id>
                  <published>2003-12-13T18:30:02Z</published>
                  <updated>2003-12-13T18:30:02Z</updated>
                </entry>
              </item>
            </items>
          </event>
        </message>
     
        <xs:element name='item'>
            <xs:complexType>
              <xs:choice minOccurs='0'>
                <xs:element name='retract' type='empty'/>
                <xs:any namespace='##other'/>
              </xs:choice>
              <xs:attribute name='id' type='xs:string' use='optional'/>
            </xs:complexType>
        </xs:element>
    */

    // This class is the same as the Item class in the main pubsub namespace,
    // so inherit it and overwrite some properties and functions

    public class Item : agsXMPP.protocol.extensions.pubsub.Item
    {
        #region << Constructors >>
        public Item() : base()
        {
            this.Namespace = Uri.PUBSUB_EVENT;
        }
        
        public Item(string id) : this()
        {
            this.Id = id;
        }
        #endregion

        private const string RETRACT = "retract";

        public bool Retract
        {
            get { return HasTag(RETRACT); }
            set 
            {
                if (value)
                    SetTag(RETRACT);
                else
                    RemoveTag(RETRACT);
            }
        }
    }
}
