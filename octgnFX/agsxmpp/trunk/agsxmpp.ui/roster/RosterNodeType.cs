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

namespace agsXMPP.ui.roster
{
    public enum RosterNodeType
    {
        /// <summary>
        /// a rootnode, normally "offline" or "online"
        /// </summary>
        RootNode,
        /// <summary>
        /// a groupnode
        /// </summary>
        GroupNode,
        /// <summary>
        /// a contact item (RosterItem)
        /// </summary>
        RosterNode,
        /// <summary>
        /// a resource node (Presence)
        /// </summary>
        ResourceNode      
    }
}
