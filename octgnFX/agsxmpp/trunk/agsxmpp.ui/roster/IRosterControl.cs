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
using agsXMPP.protocol.iq.roster;

namespace agsXMPP.ui.roster
{
    /// <summary>
    /// implement this interface if you create your own roster control.
    /// If all rostercontrols use this interface its easy to change a roster control
    /// in an application, or let the user decide which control to use. Its also
    /// possible to use them as a kind of plugin system.
    /// </summary>
    interface IRosterControl
    {
        RosterNode AddRosterItem(RosterItem ritem);
        
        bool RemoveRosterItem(Jid jid);
        bool RemoveRosterItem(RosterItem ritem);
        
        void SetPresence(Presence pres);
        
        void Clear();
    }
}