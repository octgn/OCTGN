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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace agsXMPP.ui.roster
{
    public class RosterData
    {        
        private Dictionary<string, PresenceData> m_Presences = 
                new Dictionary<string, PresenceData>();
        
        private RosterNode m_RosterNode;
     
        public RosterData(RosterNode node)
        {
            m_RosterNode = node;
        }

        public RosterNode RosterNode
        {
            get { return m_RosterNode; }
            set { m_RosterNode = value; }
        }

        public Dictionary<string, PresenceData> Presences
        {
            get { return m_Presences; }
            set { m_Presences = value; }
        }

        /// <summary>
        /// gets the group in which this contat is displayed
        /// </summary>
        public string Group
        {
            get { return m_RosterNode.Parent.Name; }            
        }

        /// <summary>
        /// Is this node in the online section?
        /// </summary>
        public bool Online
        {
            get { return m_Presences.Count > 0 ? true : false; }           
        }
    }
}
