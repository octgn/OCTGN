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
        <iq type='result'
            from='pubsub.shakespeare.lit'
            to='francisco@denmark.lit'
            id='affil1'>
          <pubsub xmlns='http://jabber.org/protocol/pubsub'>
            <affiliations>
              <affiliation node='node1' jid='francisco@denmark.lit' affiliation='owner'/>
              <affiliation node='node2' jid='francisco@denmark.lit' affiliation='publisher'/>
              <affiliation node='node5' jid='francisco@denmark.lit' affiliation='outcast'/>
              <affiliation node='node6' jid='francisco@denmark.lit' affiliation='owner'/>
            </affiliations>
          </pubsub>
        </iq>
    */

    public class Affiliations : Element
    {
        #region << Consrtuctors >>
        public Affiliations()
        {
            this.TagName    = "affiliations";
            this.Namespace  = Uri.PUBSUB;
        }
        #endregion

        public Affiliation AddAffiliation()
        {
            Affiliation aff = new Affiliation();
            AddChild(aff);
            return aff;
        }


        public Affiliation AddAffiliation(Affiliation aff)
        {
            AddChild(aff);
            return aff;
        }

        public Affiliation[] GetAffiliations()
        {
            ElementList nl = SelectElements(typeof(Affiliation));
            Affiliation[] items = new Affiliation[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Affiliation) e;
                i++;
            }
            return items;
        }
    }
}
