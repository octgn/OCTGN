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
using System.Text;
using System.Windows.Forms;

using agsXMPP;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq.roster;

namespace agsXMPP.ui.roster
{

    public class RosterNode : TreeNode
    {
        private RosterNodeType	    m_NodeType;
        //private Presence	        m_Presence;
        private RosterItem          m_RosterItem;
        private Presence            m_Presence;

        
    
        #region << Constructors >>
        public RosterNode()
        {
        }

		public RosterNode(string text, RosterNodeType nodeType) : base(text)
		{
			this.NodeType = nodeType;
		}

		public RosterNode(string text): base(text)
		{
        }
        #endregion

        public RosterNodeType NodeType
		{
			get { return m_NodeType;  }
			set { m_NodeType = value; }
		}

        public Presence Presence
        {
            get { return m_Presence; }
            set { m_Presence = value; }
        }

        public RosterItem RosterItem
        {
            get
            {
                return m_RosterItem;
            }
            set
            {
                m_RosterItem = value;
            }
        }

        /// <summary>
        /// Override the Clone function to Clone() the inherited RosterNode.
        /// </summary>
        /// <returns>cloned object</returns>
        public override object Clone()
        {
            RosterNode n = base.Clone() as RosterNode;
            n.NodeType      = this.NodeType;
            n.RosterItem    = this.RosterItem;
            //n.Presence      = this.Presence;

            return n;            
        }

	}
}


