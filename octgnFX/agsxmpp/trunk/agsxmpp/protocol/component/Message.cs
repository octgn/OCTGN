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

#region Using directives

using System;

#endregion

namespace agsXMPP.protocol.component
{
    /// <summary>
    /// Summary description for Message.
    /// </summary>
    public class Message : agsXMPP.protocol.client.Message
    {
        #region << Constructors >>
        public Message()
            : base()
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to)
            : base(to)
        {
            this.Namespace = Uri.ACCEPT;
        }
        
        public Message(Jid to, string body) 
            : base(to, body)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from) 
            : base(to, from)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(string to, string body) 
            : base(to, body)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, string body, string subject)
            : base(to, body, subject)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(string to, string body, string subject) 
            : base(to, body, subject)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(string to, string body, string subject, string thread)
            : base(to, body, subject, thread)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, string body, string subject, string thread)
            : base(to, body, subject, thread)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(string to, agsXMPP.protocol.client.MessageType type, string body)
            : base(to, type, body)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, agsXMPP.protocol.client.MessageType type, string body)
            : base(to, type, body)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(string to, agsXMPP.protocol.client.MessageType type, string body, string subject)
            : base(to, type, body, subject)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, agsXMPP.protocol.client.MessageType type, string body, string subject)
            : base(to, type, body, subject)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(string to, agsXMPP.protocol.client.MessageType type, string body, string subject, string thread)
            : base(to, type, body, subject, thread)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, agsXMPP.protocol.client.MessageType type, string body, string subject, string thread)
            : base(to, type, body, subject, thread)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, string body)
            : base(to, from, body)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, string body, string subject)
            : base(to, from, body, subject)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, string body, string subject, string thread)
            : base(to, from, body, subject, thread)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, agsXMPP.protocol.client.MessageType type, string body)
            : base(to, from, type, body)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, agsXMPP.protocol.client.MessageType type, string body, string subject)
            : base(to, from, type, body, subject)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Message(Jid to, Jid from, agsXMPP.protocol.client.MessageType type, string body, string subject, string thread)
            : base(to, from, type, body, subject, thread)
        {
            this.Namespace = Uri.ACCEPT;
        }
        #endregion

        /// <summary>
        /// Error Child Element
        /// </summary>
        public new agsXMPP.protocol.component.Error Error
        {
            get
            {
                return SelectSingleElement(typeof(agsXMPP.protocol.component.Error)) as agsXMPP.protocol.component.Error;

            }
            set
            {
                if (HasTag(typeof(agsXMPP.protocol.component.Error)))
                    RemoveTag(typeof(agsXMPP.protocol.component.Error));

                if (value != null)
                    this.AddChild(value);
            }
        }
    }
}
