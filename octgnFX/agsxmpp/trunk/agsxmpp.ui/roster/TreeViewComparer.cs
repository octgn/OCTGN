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

namespace agsXMPP.ui.roster
{
    internal class TreeViewComparer : IComparer
    {

        #region IComparer Members

        public int Compare(object x, object y)
        {
            RosterNode nodeX = x as RosterNode;
            RosterNode nodeY = y as RosterNode;

            if (nodeX.NodeType == RosterNodeType.RootNode &&
                nodeY.NodeType == RosterNodeType.RootNode)
            {
                // don't sort this nodes, 
                // they should always be in the order we added them in the Init() function
                // od the Roster Tree Control
                return 0;
            }
            else
            {
                return string.Compare(nodeX.Text, nodeY.Text, true);
            }
        }

        #endregion
    }
}