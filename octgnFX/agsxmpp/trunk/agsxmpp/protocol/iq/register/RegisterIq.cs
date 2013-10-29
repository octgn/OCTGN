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

namespace agsXMPP.protocol.iq.register
{
	/// <summary>
	/// Used for registering new usernames on Jabber/XMPP Servers
	/// </summary>
	public class RegisterIq : IQ
	{
		private Register m_Register = new Register();

		public RegisterIq()
		{
			base.Query = m_Register;
			this.GenerateId();
		}

		public RegisterIq(IqType type) : this()
		{			
			this.Type = type;		
		}

		public RegisterIq(IqType type, Jid to) : this(type)
		{
			this.To = to;
		}

		public RegisterIq(IqType type, Jid to, Jid from) : this(type, to)
		{
			this.From = from;
		}

		public new Register Query
		{
			get
			{
				return m_Register;
			}           
		}
	}
}