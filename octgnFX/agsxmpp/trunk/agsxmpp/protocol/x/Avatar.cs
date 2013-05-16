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

namespace agsXMPP.protocol.x
{
	
	// <x xmlns="jabber:x:avatar"><hash>bbf231f2b7fa1772c2ec5cffa620d3aedb4bd793</hash></x>

	/// <summary>
	/// JEP-0008 avatars
	/// </summary>
	public class Avatar : Element
	{
		public Avatar()
		{
			this.TagName	= "x";
			this.Namespace	= Uri.X_AVATAR;
		}

        public Avatar(string hash) : this()
        {
            Hash = hash;
        }

		public string Hash
		{
			get
			{
				return GetTag("hash");
			}
			set
			{
				SetTag("hash", value);
			}
		}
	}
}
