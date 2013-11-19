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

namespace agsXMPP.protocol.iq.privacy
{
    /// <summary>
    /// Summary description for PrivateIq.
    /// </summary>
    public class PrivacyIq : IQ
    {
        Privacy m_Privacy = new Privacy();

        public PrivacyIq()
        {
            base.Query = m_Privacy;
            this.GenerateId();
        }

        public PrivacyIq(IqType type)
            : this()
        {
            this.Type = type;
        }

        public PrivacyIq(IqType type, Jid to)
            : this(type)
        {
            this.To = to;
        }

        public PrivacyIq(IqType type, Jid to, Jid from)
            : this(type, to)
        {
            this.From = from;
        }

        public new Privacy Query
        {
            get { return m_Privacy; }
        }
    }
}
