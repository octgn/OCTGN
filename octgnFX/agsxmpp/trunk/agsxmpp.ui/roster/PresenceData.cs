/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *																					 *
 * Copyright (c) 2005-2009 by AG-Software												 *
 * All Rights Reserved.																 *
 *																					 *
 * You should have received a copy of the AG-Software Shared Source License			 *
 * along with this library; if not, email gnauck@ag-software.de to request a copy.   *
 *																					 *
 * For general enquiries, email gnauck@ag-software.de or visit our website at:		 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

using agsXMPP.protocol.client;

namespace agsXMPP.ui.roster
{
    public class PresenceData
    {
        private RosterNode  m_Node;
        private Presence    m_Presence;

        public PresenceData(RosterNode node)
        {
            m_Node = node;
        }

        public PresenceData(RosterNode node, Presence pres)
        {
            m_Node = node;
            m_Presence = pres;
        }

        public RosterNode Node
        {
            get { return m_Node; }
            set { m_Node = value; }
        }

        public Presence Presence
        {
            get { return m_Presence; }
            set { m_Presence = value; }
        }
    }
}
