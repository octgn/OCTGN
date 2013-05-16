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
using System.Text;

using agsXMPP.protocol.client;

namespace agsXMPP.protocol.extensions.pubsub
{
    public class PubSubIq : IQ
    {
        /*
            Example 1. Entity requests a new node with default configuration.

            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="create1">
                <pubsub xmlns="http://jabber.org/protocol/pubsub">
                    <create node="generic/pgm-mp3-player"/>
                    <configure/>
                </pubsub>
            </iq>
        */
        private PubSub m_PubSub = new PubSub();

        #region << Constructors >>
        public PubSubIq()
        {
            this.GenerateId();
            this.AddChild(m_PubSub);            
        }
        
        public PubSubIq(IqType type) : this()
        {
            this.Type = type;
        }

        public PubSubIq(IqType type, Jid to) : this(type)
        {
            this.To = to;
        }

        public PubSubIq(IqType type, Jid to, Jid from) : this(type, to)
        {
            this.From = from;
        }
        #endregion

        public PubSub PubSub
        {
            get
            {
                return m_PubSub;
            }
        }

        
    }		
}
