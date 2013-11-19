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

namespace agsXMPP.protocol.iq.version
{
	// Send:<iq type='get' id='MX_6' to='jfrankel@coversant.net/SoapBox'>
	//			<query xmlns='jabber:iq:version'></query>
	//		</iq>
	//
	// Recv:<iq from="jfrankel@coversant.net/SoapBox" id="MX_6" to="gnauck@myjabber.net/Office" type="result">
	//			<query xmlns="jabber:iq:version">
	//				<name>SoapBox</name>
	//				<version>2.1.2 beta</version>
	//				<os>Windows NT 5.1 (en-us)</os>
	//			</query>
	//		</iq> 


	/// <summary>
	/// Zusammenfassung für Version.
	/// </summary>
	public class Version : Element
	{
		public Version()
		{
			this.TagName	= "query";
			this.Namespace	= Uri.IQ_VERSION;
		}

		public string Name
		{
			set	{ SetTag("name", value); }
			get	{ return GetTag("name"); }
		}

		public string Ver
		{
			set	{ SetTag("version", value); }
			get	{ return GetTag("version");	}
		}

		public string Os
		{
			set { SetTag("os", value); }
			get { return GetTag("os"); }
		}

	}
}
