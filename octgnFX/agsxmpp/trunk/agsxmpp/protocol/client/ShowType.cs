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
	//	# away -- The entity or resource is temporarily away.
	//	# chat -- The entity or resource is actively interested in chatting.
	//	# dnd -- The entity or resource is busy (dnd = "Do Not Disturb").
	//	# xa -- The entity or resource is away for an extended period (xa = "eXtended Away").
	
	/// <summary>
	/// Enumeration that represents the online state.
	/// </summary>
	public enum ShowType
	{
		/// <summary>
		/// 
		/// </summary>
		NONE = -1,
		
		/// <summary>
		/// The entity or resource is temporarily away.
		/// </summary>
		away,
		
		/// <summary>
		/// The entity or resource is actively interested in chatting.
		/// </summary>
		chat,
		
		/// <summary>
		/// The entity or resource is busy (dnd = "Do Not Disturb").
		/// </summary>
		dnd,
		
		/// <summary>
		/// The entity or resource is away for an extended period (xa = "eXtended Away").
		/// </summary>
		xa,
	}
}