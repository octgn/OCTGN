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

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.component
{
	
	//<handshake>aaee83c26aeeafcbabeabfcbcd50df997e0a2a1e</handshake>

	/// <summary>
	/// Handshake Element
	/// </summary>
	public class Handshake : Stanza
	{
		public Handshake()
		{
			this.TagName	= "handshake";
			this.Namespace	= Uri.ACCEPT;			
		}

		public Handshake(string password, string streamid) : this()
		{
			SetAuth(password, streamid);
		}

		public void SetAuth(string password, string streamId)
		{
			this.Value = Util.Hash.Sha1Hash(streamId + password);
		}

		/// <summary>
		/// Digest (Hash) for authentication
		/// </summary>
		public string Digest
		{
			get { return this.Value; }
			set { this.Value = value; }

		}
	}
}
