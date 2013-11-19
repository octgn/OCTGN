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


// Send:	<iq type='get' id='MX_5' to='jfrankel@coversant.net/SoapBox'>
//				<query xmlns='jabber:iq:last'></query>
//			</iq>
// Recv:	<iq from="jfrankel@coversant.net/SoapBox" id="MX_5" to="gnauck@myjabber.net/Office" type="result">
//				<query seconds="644" xmlns="jabber:iq:last"/>
//			</iq> 

namespace agsXMPP.protocol.iq.last
{
	/// <summary>
	/// Zusammenfassung für Last.
	/// </summary>
	public class Last : Element
	{
		public Last()
		{
			this.TagName	= "query";
			this.Namespace	= Uri.IQ_LAST;
		}

		/// <summary>
		/// Seconds since the last activity.
		/// </summary>
		public int Seconds
		{
			get { return Int32.Parse(GetAttribute("seconds"));  }
			set { SetAttribute("seconds", value.ToString()); }
		}
	}
}
