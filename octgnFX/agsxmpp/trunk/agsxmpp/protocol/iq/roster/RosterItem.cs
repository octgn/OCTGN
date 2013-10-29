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

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.roster
{
	// jabber:iq:roster
	// <iq from="user@server.com/Office" id="doroster_1" type="result">
	//		<query xmlns="jabber:iq:roster">
	//			<item subscription="both" name="juiliet" jid="11111@icq.myjabber.net"><group>ICQ</group></item>
	//			<item subscription="both" name="roman" jid="22222@icq.myjabber.net"><group>ICQ</group></item>
	//			<item subscription="both" name="angie" jid="33333@icq.myjabber.net"><group>ICQ</group></item>
	//			<item subscription="both" name="bob" jid="44444@icq.myjabber.net"><group>ICQ</group></item>
	//		</query>
	// </iq> 

	// # "none" -- the user does not have a subscription to the contact's presence information, and the contact does not have a subscription to the user's presence information
	// # "to" -- the user has a subscription to the contact's presence information, but the contact does not have a subscription to the user's presence information
	// # "from" -- the contact has a subscription to the user's presence information, but the user does not have a subscription to the contact's presence information
	// # "both" -- both the user and the contact have subscriptions to each other's presence information

    // TODO rename to Ask and move to a seperate file, so it matches better to all other enums
	public enum AskType
	{		
		NONE = -1,	
		subscribe,		
		unsubscribe
	}

    // TODO rename to Subscription and move to a seperate file, so it matches better to all other enums
	public enum SubscriptionType
	{        
		none,
		to,
		from,
		both,
		remove
	}

	/// <summary>
	/// Item is used in jabber:iq:roster, jabber:iq:search
	/// </summary>
	public class RosterItem : agsXMPP.protocol.Base.RosterItem
	{
		public RosterItem() : base()
		{
			this.Namespace	= Uri.IQ_ROSTER;
		}
		
		public RosterItem(Jid jid) : this()
		{
			Jid = jid;				
		}

		public RosterItem(Jid jid, string name) : this(jid)
		{
			Name = name;
		}

		public SubscriptionType Subscription
		{
			get 
			{ 
				return (SubscriptionType) GetAttributeEnum("subscription", typeof(SubscriptionType)); 
			}
			set { SetAttribute("subscription", value.ToString()); }
		}

		public AskType Ask
		{
			get 
			{ 
				return (AskType) GetAttributeEnum("ask", typeof(AskType)); 
			}
			set 
			{ 
				if (value == AskType.NONE) 
					RemoveAttribute("ask");
				else
					SetAttribute("ask", value.ToString()); 
			}
		}
	}	
}
