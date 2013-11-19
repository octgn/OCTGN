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
	//	<!-- Email address property. Default type is INTERNET. -->
	//	<!ELEMENT EMAIL (
	//	HOME?, 
	//	WORK?, 
	//	INTERNET?, 
	//	PREF?, 
	//	X400?, 
	//	USERID
	//	)>
	public enum EmailType
	{
		NONE = -1,
		HOME,
		WORK,
		INTERNET,
		X400,
	}

	/// <summary>
	/// 
	/// </summary>
	public class Email : Element
	{	
		// <EMAIL><INTERNET/><PREF/><USERID>stpeter@jabber.org</USERID></EMAIL>
		#region << Constructors >>
		public Email()
		{
			this.TagName	= "EMAIL";
			this.Namespace	= Uri.VCARD;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">Type of the new Email Adress</param>
		/// <param name="address">Email Adress</param>
		/// <param name="prefered">Is this adressed prefered</param>
		public Email(EmailType type, string userid, bool prefered) : this()
		{
			Type		= type;
			IsPrefered	= prefered;
			UserId		= userid;			
		}
		#endregion

		public EmailType Type
		{
			get { return (EmailType) HasTagEnum(typeof(EmailType)); }
			set 
			{ 
				if (value != EmailType.NONE)
					SetTag(value.ToString()); 
			}

		}
		/// <summary>
		/// Is this the prefered contact adress?
		/// </summary>
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

		/// <summary>
		/// The email Adress
		/// </summary>
		public string UserId
		{
			get { return GetTag("USERID"); }
			set { SetTag("USERID", value); }
		}
	}
}
