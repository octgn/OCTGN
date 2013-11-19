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
using agsXMPP.protocol.client;

namespace agsXMPP.protocol.iq.vcard
{
	//<iq id="id_62" to="gnauck@myjabber.net" type="get"><vCard xmlns="vcard-temp"/></iq>
	
	/// <summary>
	/// Summary description for VcardIq.
	/// </summary>
	public class VcardIq : IQ
	{
		private Vcard m_Vcard = new Vcard();

		#region << Constructors >>
		public VcardIq()
		{
			this.GenerateId();
			this.AddChild(m_Vcard);
		}	

		public VcardIq(IqType type) : this()
		{			
			this.Type = type;		
		}

		public VcardIq(IqType type, Vcard vcard) : this(type)
		{			
			this.Vcard = vcard;
		}

		public VcardIq(IqType type, Jid to) : this(type)
		{
			this.To = to;
		}

		public VcardIq(IqType type, Jid to, Vcard vcard) : this(type, to)
		{
			this.Vcard = vcard;
		}

		public VcardIq(IqType type, Jid to, Jid from) : this(type, to)
		{
			this.From = from;
		}

		public VcardIq(IqType type, Jid to, Jid from, Vcard vcard) : this(type, to, from)
		{
			this.Vcard = vcard;
		}
		#endregion
			
		public override Vcard Vcard 
		{
			get 
			{ 
				return m_Vcard;
			}
			set
			{
				ReplaceChild(value);
			}
		}
	}
}
