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
using System.IO;
using System.Text;

using agsXMPP;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.sasl;

namespace agsXMPP.Sasl.Plain
{
	/// <summary>
	/// Summary description for PlainMechanism.
	/// </summary>
	public class PlainMechanism : Mechanism
	{
		private XmppClientConnection m_XmppClient	= null;

		public PlainMechanism()
		{			
		}

		public override void Init(XmppClientConnection con)
		{
			m_XmppClient = con;
			
			// <auth mechanism="PLAIN" xmlns="urn:ietf:params:xml:ns:xmpp-sasl">$Message</auth>
			m_XmppClient.Send(new protocol.sasl.Auth(protocol.sasl.MechanismType.PLAIN, Message()));
		}

		public override void Parse(Node e)
		{
			// not needed here in PLAIN mechanism
		}


		private string Message()
		{	  
			// NULL Username NULL Password
			StringBuilder sb = new StringBuilder();
			
			//sb.Append( (char) 0 );
			//sb.Append(this.m_XmppClient.MyJID.Bare);
			
			sb.Append( (char) 0 );
			sb.Append(this.Username);
			sb.Append( (char) 0 );
			sb.Append(this.Password);
			
			byte[] msg = Encoding.UTF8.GetBytes(sb.ToString());
			return Convert.ToBase64String(msg, 0, msg.Length);
		}
	}
}