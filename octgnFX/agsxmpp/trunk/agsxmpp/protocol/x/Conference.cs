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

namespace agsXMPP.protocol.x
{
	/*
	<message from='crone1@shakespeare.lit/desktop' to='hecate@shakespeare.lit'>
		<body>You have been invited to darkcave@macbeth.</body>
		<x jid='room@service' xmlns='jabber:x:conference'/>
	</message>
	*/

	/// <summary>
	/// is used for inviting somebody to a chatroom
	/// </summary>
	public class Conference : Element
	{
		public Conference()
		{
			this.TagName	= "x";
			this.Namespace	= Uri.X_CONFERENCE;
		}

		public Conference(Jid room) : this()
		{
			Chatroom = room;
		}

		/// <summary>
		/// Room Jid
		/// </summary>
		public Jid Chatroom
		{
			get { return new Jid(GetAttribute("jid")); }
			set { SetAttribute("jid", value.ToString()); }
		}
	}
}
