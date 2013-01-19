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

namespace agsXMPP.protocol.iq.disco
{
	public enum DiscoAction
	{		
		NONE = -1,
		remove,
		update
	}

	/// <summary>
	///
	/// </summary>
	public class DiscoItem : Element
	{
		public DiscoItem()
		{
			this.TagName	= "item";
			this.Namespace	= Uri.DISCO_ITEMS;
		}

		public Jid Jid
		{
			get { return new Jid(GetAttribute("jid")); }
			set { SetAttribute("jid", value.ToString()); }
		}
		
		public string Name
		{
			get { return GetAttribute("name"); }
			set { SetAttribute("name", value); }
		}
		
		public string Node
		{
			get { return GetAttribute("node"); }
			set { SetAttribute("node", value); }
		}

		public DiscoAction Action
		{
			get { return (DiscoAction) GetAttributeEnum("action", typeof(DiscoAction)); }
			set 
			{ 
				if (value == DiscoAction.NONE) 
					RemoveAttribute("action");
				else
					SetAttribute("action", value.ToString()); 
			}
		}
	}
}
