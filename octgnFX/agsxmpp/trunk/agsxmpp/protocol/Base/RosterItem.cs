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

namespace agsXMPP.protocol.Base
{
	// jabber:iq:roster
	// <iq from="gnauck@myjabber.net/Office" id="doroster_1" type="result">
	//		<query xmlns="jabber:iq:roster">
	//			<item subscription="both" name="Nachtkrapp" jid="50198521@icq.myjabber.net"><group>ICQ</group></item>
	//			<item subscription="both" name="czerkasov" jid="62764180@icq.myjabber.net"><group>ICQ</group></item>
	//			<item subscription="both" name="Poacher" jid="92179686@icq.myjabber.net"><group>ICQ</group></item>
	//			<item subscription="both" name="Diabolo" jid="102840558@icq.myjabber.net"><group>ICQ</group></item>
	//		</query>
	// </iq> 

	// # "none" -- the user does not have a subscription to the contact's presence information, and the contact does not have a subscription to the user's presence information
	// # "to" -- the user has a subscription to the contact's presence information, but the contact does not have a subscription to the user's presence information
	// # "from" -- the contact has a subscription to the user's presence information, but the user does not have a subscription to the contact's presence information
	// # "both" -- both the user and the contact have subscriptions to each other's presence information

	/// <summary>
	/// Item is used in jabber:iq:roster, x roster
	/// </summary>
	public class RosterItem : Item
	{
		public RosterItem() : base()
		{
		}

		/// <summary>
		/// Groups a roster Item is assigned to
		/// </summary>
        public ElementList GetGroups()
		{
			return this.SelectElements("group");			
		}

		/// <summary>
		/// Add a new group to the Rosteritem
		/// </summary>
		/// <param name="groupname"></param>
		public void AddGroup(string groupname)
		{
			Group g = new Group(groupname);
			this.AddChild(g);
		}

		public bool HasGroup(string groupname)
		{
            ElementList groups = GetGroups();
			foreach (Group g in groups)
			{
				if (g.Name == groupname)
					return true;
			}
			return false;
		}

		public void RemoveGroup(string groupname)
		{
            ElementList groups = GetGroups();
			foreach (Group g in groups)
			{
				if (g.Name == groupname)
				{
					g.Remove();
					return;
				}
			}
		}		
	}	
}
