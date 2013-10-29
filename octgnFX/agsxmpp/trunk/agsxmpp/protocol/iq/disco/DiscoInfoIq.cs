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

using agsXMPP.protocol;
using agsXMPP.protocol.client;

namespace agsXMPP.protocol.iq.disco
{
	/// <summary>
	/// Discovering Information About a Jabber Entity
	/// </summary>
	public class DiscoInfoIq : IQ
	{
		private DiscoInfo m_DiscoInfo = new DiscoInfo();
		
		public DiscoInfoIq()
		{
			base.Query = m_DiscoInfo;
			this.GenerateId();
		}

		public DiscoInfoIq(IqType type) : this()
		{			
			this.Type = type;		
		}	

		public new DiscoInfo Query
		{
			get
			{
				return m_DiscoInfo;
			}
		}
	}
}
