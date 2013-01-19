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
	public enum TelephoneLocation
	{
		NONE = -1,
		HOME,
		WORK
	}
	
	public enum TelephoneType
	{		
		NONE = -1,		
		VOICE,
		FAX,
		PAGER,
		MSG,
		CELL,
		VIDEO,
		BBS,
		MODEM,
		ISDN,
		PCS, 
		PREF, 
		NUMBER	
	}

	/// <summary>
	/// Zusammenfassung für Telephone.
	/// </summary>
	public class Telephone : Element
	{
		//	<TEL><VOICE/><WORK/><NUMBER>303-308-3282</NUMBER></TEL>
		//	<TEL><FAX/><WORK/><NUMBER/></TEL>
		//	<TEL><MSG/><WORK/><NUMBER/></TEL>
		
		#region << Constructors >>
		public Telephone()
		{
			this.TagName	= "TEL";
			this.Namespace	= Uri.VCARD;			
		}

		public Telephone(TelephoneLocation loc, TelephoneType type, string number) : this()
		{
			if(loc != TelephoneLocation.NONE)
				this.Location	= loc;
			
			if(type != TelephoneType.NONE)
				this.Type		= type;
			
			this.Number		= number;
		}
		#endregion

		public string Number
		{
			get { return GetTag("NUMBER"); }
			set	{ SetTag("NUMBER", value); }
		}

		public TelephoneLocation Location
		{
			get
			{
				return (TelephoneLocation) HasTagEnum(typeof(TelephoneLocation));
			}
			set
			{
				SetTag(value.ToString());
			}
		}

		public TelephoneType Type
		{
			get
			{
				return (TelephoneType) HasTagEnum(typeof(TelephoneType));
			}
			set
			{
				SetTag(value.ToString());
			}
		}

	}
}
