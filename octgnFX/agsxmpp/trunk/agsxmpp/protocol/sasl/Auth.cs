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

// <auth xmlns='urn:ietf:params:xml:ns:xmpp-sasl' mechanism='DIGEST-MD5'/>
namespace agsXMPP.protocol.sasl
{
	/// <summary>
	/// Summary description for Auth.
	/// </summary>
	public class Auth : Element
	{
		public Auth()
		{
			this.TagName	= "auth";
			this.Namespace	= Uri.SASL;
		}

		public Auth(MechanismType type) : this()
		{
			MechanismType = type;
		}

		public Auth(MechanismType type, string text) : this(type)
		{			
			this.Value		= text;			
		}


		public MechanismType MechanismType
		{
			get
			{
				return Mechanism.GetMechanismType(GetAttribute("mechanism"));
			}
			set
			{
				SetAttribute("mechanism", Mechanism.GetMechanismName(value));
			}
		}
	}
}
