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
	// Avatar is in multiple Namespaces. So better to work with a Base class

	/// <summary>
	/// Summary description for Avatar.
	/// </summary>
	public class Avatar : Element
	{		
		public Avatar()
		{
			this.TagName	= "query";				
		}

		public byte[] Data
		{
			get
			{
				if ( HasTag("data") )
					return Convert.FromBase64String(GetTag("data"));
				else
					return null;
			}
			set
			{
				SetTag("data", Convert.ToBase64String(value, 0, value.Length));
			}
		}

		public string MimeType
		{
			get
			{
				Element data = SelectSingleElement("data");
				if (data != null)
					return GetAttribute("mimetype");
				else
					return null;
			}
			set
			{
				Element data = SelectSingleElement("data");
				if (data != null)
					SetAttribute("mimetype", value);
			}
		}
	}
	
}
