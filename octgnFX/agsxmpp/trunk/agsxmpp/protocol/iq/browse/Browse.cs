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

// JEP-0011: Jabber Browsing
//
// This JEP defines a way to describe information about Jabber entities and the relationships between entities. 
// Note: This JEP is superseded by JEP-0030: Service Discovery.

// WARNING: This JEP has been deprecated by the Jabber Software Foundation. 
// Implementation of the protocol described herein is not recommended. Developers desiring similar functionality should 
// implement the protocol that supersedes this one (if any).

// Most components and gateways still dont implement Service discovery. So we must use jabber:iq:browse for them until everything
// is replaced with JEP 30 (Service Discovery).
namespace agsXMPP.protocol.iq.browse
{
	/// <summary>
	/// Summary description for Browse.
	/// </summary>
	public class Browse : Element
	{
		public Browse()
		{
			this.TagName	= "query";
			this.Namespace	= Uri.IQ_BROWSE;
		}

		public string Category
		{
			get { return GetAttribute("category"); }
			set { SetAttribute("category", value); }
		}
		
		public string Type
		{
			get { return GetAttribute("type"); }
			set { SetAttribute("type", value); }
		}
		
		public string Name
		{
			get { return GetAttribute("name"); }
			set { SetAttribute("name", value); }
		}

		public string[] GetNamespaces()
		{
            ElementList elements = SelectElements("ns");
			string[] nss = new string[elements.Count];
			
			int i=0;
			foreach (Element ns in elements)
			{
				nss[i] = ns.Value;
				i++;
			}

			return nss;
		}

		public BrowseItem[] GetItems()
		{
            ElementList nl = SelectElements(typeof(BrowseItem));
			BrowseItem[] items = new BrowseItem[nl.Count];
			int i = 0;
			foreach (Element item in nl)
			{
				items[i] = item as BrowseItem;
				i++;
			}
			return items;
		}
	}
}
