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

namespace agsXMPP.protocol.iq.roster
{
	/// <summary>
	/// Extension JEP-0083, delimiter for nested roster groups
	/// </summary>
	public class Delimiter : Element
	{
		/*
		3.1 Querying for the delimiter 
		All compliant clients SHOULD query for an existing delimiter at login.

		Example 1. Querying for the Delimiter
			
		CLIENT:																												 CLIENT:
		<iq type='get'
			 id='1'>
		<query xmlns='jabber:iq:private'>
			 <roster xmlns='roster:delimiter'/>
				  </query>
		</iq>

		SERVER:
		<iq type='result'
			 id='1'
		from='bill@shakespeare.lit/Globe'
		to='bill@shakespeare.lit/Globe'>
		<query xmlns='jabber:iq:private'>
			 <roster xmlns='roster:delimiter'>::</roster>
		</query>
		</iq>
		*/
		public Delimiter()
		{
			this.TagName	= "roster";
            this.Namespace	= Uri.ROSTER_DELIMITER;
		}

		public Delimiter(string delimiter) : this()
		{
			this.Value = delimiter;
		}

	}
}
