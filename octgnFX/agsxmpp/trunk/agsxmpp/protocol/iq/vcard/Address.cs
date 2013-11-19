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

namespace agsXMPP.protocol.iq.vcard
{
	public enum AddressLocation
	{
		NONE = -1,
		HOME,
		WORK
	}

	/// <summary>
	/// 
	/// </summary>
	public class Address : Element
	{
		//		<!-- Structured address property. Address components with
		//		multiple values must be specified as a comma separated list
		//		of values. -->
		//		<!ELEMENT ADR (
		//		HOME?, 
		//		WORK?, 
		//		POSTAL?, 
		//		PARCEL?, 
		//		(DOM | INTL)?, 
		//		PREF?, 
		//		POBOX?, 
		//		EXTADR?, 
		//		STREET?, 
		//		LOCALITY?, 
		//		REGION?, 
		//		PCODE?, 
		//		CTRY?
		//		)>

		// <ADR>
		//	<WORK/>
		//	<EXTADD>Suite 600</EXTADD>
		//	<STREET>1899 Wynkoop Street</STREET>
		//	<LOCALITY>Denver</LOCALITY>
		//	<REGION>CO</REGION>
		//	<PCODE>80202</PCODE>
		//	<CTRY>USA</CTRY>
		// </ADR>
		public Address()
		{
			this.TagName	= "ADR";
			this.Namespace	= Uri.VCARD;
		}

		public Address(AddressLocation loc, string extra, string street, string locality, string region, string postalcode, string country, bool prefered) : this()
		{
			Location		= loc;
			ExtraAddress	= extra;
			Street			= street;
			Locality		= locality;
			Region			= region;
			PostalCode		= postalcode;
			Country			= country;
			IsPrefered		= prefered;
		}

		public AddressLocation Location
		{
			get
			{
				return (AddressLocation) HasTagEnum(typeof(AddressLocation));
			}
			set
			{
				SetTag(value.ToString());
			}
		}

		public bool IsPrefered
		{
			get { return HasTag("PREF"); }
			set 
			{
				if (value == true)
					SetTag("PREF");
				else
					RemoveTag("PREF");
			}			
		}

		public string ExtraAddress
		{
			get { return GetTag("EXTADD"); }
			set { SetTag("EXTADD", value); }
		}

		public string Street
		{
			get { return GetTag("STREET"); }
			set { SetTag("STREET", value); }
		}

		public string Locality
		{
			get { return GetTag("LOCALITY"); }
			set { SetTag("LOCALITY", value); }
		}

		public string Region
		{
			get { return GetTag("REGION"); }
			set { SetTag("REGION", value); }
		}

		public string PostalCode
		{
			get { return GetTag("PCODE"); }
			set { SetTag("PCODE", value); }
		}

		public string Country
		{
			get { return GetTag("CTRY"); }
			set { SetTag("CTRY", value); }
		}
	}
}
