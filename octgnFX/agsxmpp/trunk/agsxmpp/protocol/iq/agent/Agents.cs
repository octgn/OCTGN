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

namespace agsXMPP.protocol.iq.agent
{
	// Send:<iq id='fullagents' to='myjabber.net' type='get'>
	//			<query xmlns='jabber:iq:agents'/>
	//		</iq>
	// Recv:<iq from="myjabber.net" id="fullagents" to="gnauck@myjabber.net/Office" type="result">
	//			<query xmlns="jabber:iq:agents">
	//				<agent jid="conference.myjabber.net"><name>Public Conferencing</name><service>public</service></agent>
	//				<agent jid="aim.myjabber.net"><name>AIM Transport</name><service>aim</service><transport>Enter ID</transport><register/></agent>
	//				<agent jid="yahoo.myjabber.net"><name>Yahoo! Transport</name><service>yahoo</service><transport>Enter ID</transport><register/></agent>
	//				<agent jid="icq.myjabber.net"><name>ICQ Transport</name><service>icq</service><transport>Enter ID</transport><register/></agent>
	//				<agent jid="msn.myjabber.net"><name>MSN Transport</name><service>msn</service><transport>Enter ID</transport><register/></agent>
	//			</query>
	//		</iq> 

	/// <summary>
	/// Zusammenfassung für Agent.
	/// </summary>
	public class Agents : Element
	{
		public Agents()
		{
			this.TagName	= "query";
			this.Namespace	= Uri.IQ_AGENTS;
		}


        public ElementList GetAgents()
		{
			return SelectElements("agent");
		}
	}	
}
