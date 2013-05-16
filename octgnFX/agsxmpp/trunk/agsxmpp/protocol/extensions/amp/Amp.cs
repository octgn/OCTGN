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

namespace agsXMPP.protocol.extensions.amp
{
    /*
        <xs:element name='amp'>
            <xs:complexType>
              <xs:sequence>
                <xs:element ref='rule' minOccurs='1' maxOccurs='unbounded'/>
              </xs:sequence>
              <xs:attribute name='from' usage='optional' type='xs:string'/>
              <xs:attribute name='per-hop' use='optional' type='xs:bool' default='false'/>
              <xs:attribute name='status' usage='optional' type='xs:NCName'/>
              <xs:attribute name='to' usage='optional' type='xs:string'/>
            </xs:complexType>
        </xs:element>
    */
    
    public class Amp : Base.DirectionalElement
    {
        public Amp()
        {
            this.TagName = "amp";
            this.Namespace = Uri.AMP;
        }

        /// <summary>
        /// The 'status' attribute specifies the reason for the amp element.
        /// When specifying semantics to be applied (client to server), this attribute MUST NOT be present. 
        /// When replying to a sending entity regarding a met condition, this attribute MUST be present and 
        /// SHOULD be the value of the 'action' attribute for the triggered rule. 
        /// (Note: Individual action definitions MAY provide their own requirements.)
        /// </summary>
        public Action Status
        {
            get
            {
                return (Action) GetAttributeEnum("status", typeof(Action));
            }
            set
            {
                if (value == Action.Unknown)
                    RemoveAttribute("status");
                else
                    SetAttribute("status", value.ToString());
            }
        }

        /// <summary>
        /// The 'per-hop' attribute flags the contained ruleset for processing at each server in the route 
        /// between the original sender and original intended recipient. 
        /// This attribute MAY be present, and MUST be either "true" or "false". 
        /// If not present, the default is "false".
        /// </summary>
        public bool PerHop
        {
            get { return GetAttributeBool("per-hop"); }
            set { SetAttribute("per-hop", value); }
        }

        public void AddRule(Rule rule)
        {
            AddChild(rule);
        }

        public Rule AddRule()
        {
            Rule rule = new Rule();
            AddChild(rule);

            return rule;
        }

        /// <summary>
        /// Gets a list of all form fields
        /// </summary>
        /// <returns></returns>
        public Rule[] GetRules()
        {
            ElementList nl = SelectElements(typeof(Rule));
            Rule[] items = new Rule[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Rule)e;
                i++;
            }
            return items;
        }
    }
}