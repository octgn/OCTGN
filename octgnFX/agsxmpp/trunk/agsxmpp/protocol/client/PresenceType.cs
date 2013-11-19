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

namespace agsXMPP.protocol.client
{
	
	/// <summary>
	/// Enumeration for the Presence Type structure. 
	/// This enum is used to describe what type of Subscription Type the current subscription is.
	/// When sending a presence or receiving a subscription this type is used to easily identify the type of subscription it is.
	/// </summary>
	public enum PresenceType
	{		
		/// <summary>
		/// Used when one wants to send presence to someone/server/transport that you’re available. 
		/// </summary>
		available = -1,		
		
		/// <summary>
		/// Used to send a subscription request to someone.
		/// </summary>
		subscribe,		
		
		/// <summary>
		/// Used to accept a subscription request.
		/// </summary>		
		subscribed,	
	
		/// <summary>
		/// Used to unsubscribe someone from your presence. 
		/// </summary>
		unsubscribe,
		
		/// <summary>
		/// Used to deny a subscription request.
		/// </summary>
		unsubscribed,		
		
		/// <summary>
		/// Used when one wants to send presence to someone/server/transport that you’re unavailable.
		/// </summary>
		unavailable,		
		
		/// <summary>
		/// Used when you want to see your roster, but don't want anyone on you roster to see you
		/// </summary>
		invisible,
		
		/// <summary>
		/// presence error
		/// </summary>
		error,
		
		/// <summary>
		/// used in server to server protocol to request presences
		/// </summary>
		probe
	}
}