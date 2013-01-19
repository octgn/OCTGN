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

using agsXMPP.protocol.Base;

namespace agsXMPP.protocol.x.rosterx
{
	public enum Action
	{
		NONE = -1,
		add,
		remove,
		modify
	}

	/// <summary>
	/// Summary description for RosterItem.
	/// </summary>
	public class RosterItem : agsXMPP.protocol.Base.RosterItem
	{
		/*
		<item action='delete' jid='rosencrantz@denmark' name='Rosencrantz'>   
			<group>Visitors</group>   
		</item> 
		*/

		public RosterItem() : base()
		{
			this.Namespace	= Uri.X_ROSTERX;
		}

		public RosterItem(Jid jid) : this()
		{
			Jid = jid;				
		}

		public RosterItem(Jid jid, string name) : this(jid)
		{
			Name = name;
		}

		public RosterItem(Jid jid, string name, Action action) : this(jid, name)
		{
			Action = action;
		}

		public Action Action
		{
			get 
			{ 
				return (Action) GetAttributeEnum("action", typeof(Action)); 
			}
			set { SetAttribute("action", value.ToString()); }
		}

	}
}
