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

namespace agsXMPP.protocol.extensions.pubsub.owner
{
    /*
        <iq type='result'
            from='pubsub.shakespeare.lit'
            to='hamlet@denmark.lit/elsinore'
            id='ent1'>
          <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
            <affiliates node='blogs/princely_musings'>
              <affiliate jid='hamlet@denmark.lit' affiliation='owner'/>
              <affiliate jid='polonius@denmark.lit' affiliation='outcast'/>
            </affiliates>
          </pubsub>
        </iq>
     
     
        <xs:element name='affiliates'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='affiliate' minOccurs='0' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='node' type='xs:string' use='required'/>
            </xs:complexType>
          </xs:element>
    */

    public class Affiliates : Element
    {
        #region << Constructors >>
        public Affiliates()
        {
            this.TagName    = "affiliates";
            this.Namespace  = Uri.PUBSUB_OWNER;
        }
                
        public Affiliates(string node) : this()
        {
            this.Node = node;
        }
        #endregion

        public string Node
        {
            get { return GetAttribute("node"); }
            set { SetAttribute("node", value); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Affiliate AddAffiliate()
        {
            Affiliate affiliate = new Affiliate();
            AddChild(affiliate);
            return affiliate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliate"></param>
        /// <returns></returns>
        public Affiliate AddAffiliate(Affiliate affiliate)
        {
            AddChild(affiliate);
            return affiliate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliates"></param>
        public void AddAffiliates(Affiliate[] affiliates)
        {
            foreach (Affiliate affiliate in affiliates)
            {
                AddAffiliate(affiliate);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Affiliate[] GetAffiliates()
        {
            ElementList nl = SelectElements(typeof(Affiliate));
            Affiliate[] affiliates = new Affiliate[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                affiliates[i] = (Affiliate)e;
                i++;
            }
            return affiliates;
        }
    }
}