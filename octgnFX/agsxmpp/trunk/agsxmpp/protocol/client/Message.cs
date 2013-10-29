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

using agsXMPP.protocol.x;
using agsXMPP.protocol.extensions.html;
using agsXMPP.protocol.extensions.chatstates;
using agsXMPP.protocol.extensions.nickname;
using agsXMPP.protocol.extensions.shim;

namespace agsXMPP.protocol.client
{
	/// <summary>
	/// This class represents a XMPP message.
	/// </summary>
	public class Message : Base.Stanza
	{
		#region << Constructors >>
		public Message()
		{
			this.TagName	= "message";
			this.Namespace	= Uri.CLIENT;
		}

        public Message(Jid to) : this()
        {
            To      = to;
        }
		public Message(Jid to, string body) : this(to)
		{			
			Body	= body;
		}

        public Message(Jid to, Jid from) : this()
        {
            To      = to;
            From    = from;
        }

		public Message(string to, string body) : this()
		{
			To		= new Jid(to);
			Body	= body;
		}

		public Message(Jid to, string body, string subject) : this()
		{
			To		= to;
			Body	= body;
			Subject	= subject;
		}

		public Message(string to, string body, string subject) : this()
		{
			To		= new Jid(to);
			Body	= body;
			Subject	= subject;
		}

		public Message(string to, string body, string subject, string thread) : this()
		{
			To		= new Jid(to);
			Body	= body;
			Subject	= subject;
			Thread	= thread;
		}

		public Message(Jid to, string body, string subject, string thread) : this()
		{
			To		= to;
			Body	= body;
			Subject	= subject;
			Thread	= thread;
		}

		public Message(string to, MessageType type, string body) : this()
		{
			To		= new Jid(to);
			Type	= type;
			Body	= body;
		}

		public Message(Jid to, MessageType type, string body) : this()
		{
			To		= to;
			Type	= type;
			Body	= body;
		}

		public Message(string to, MessageType type, string body, string subject) : this()
		{
			To		= new Jid(to);
			Type	= type;
			Body	= body;
			Subject	= subject;
		}

		public Message(Jid to, MessageType type, string body, string subject) : this()
		{
			To		= to;
			Type	= type;
			Body	= body;
			Subject	= subject;
		}

		public Message(string to, MessageType type, string body, string subject, string thread) : this()
		{
			To		= new Jid(to);
			Type	= type;
			Body	= body;
			Subject	= subject;
			Thread	= thread;
		}

		public Message(Jid to, MessageType type, string body, string subject, string thread) : this()
		{
			To		= to;
			Type	= type;
			Body	= body;
			Subject	= subject;
			Thread	= thread;
		}
	
		public Message(Jid to, Jid from, string body) : this()
		{
			To		= to;
			From	= from;
			Body	= body;
		}

		public Message(Jid to, Jid from, string body, string subject) : this()
		{
			To		= to;
			From	= from;
			Body	= body;
			Subject	= subject;
		}

		public Message(Jid to, Jid from, string body, string subject, string thread) : this()
		{
			To		= to;
			From	= from;
			Body	= body;
			Subject	= subject;
			Thread	= thread;
		}

		public Message(Jid to, Jid from, MessageType type, string body) : this()
		{
			To		= to;
			From	= from;
			Type	= type;
			Body	= body;
		}

		public Message(Jid to, Jid from, MessageType type, string body, string subject) : this()
		{
			To		= to;
			From	= from;
			Type	= type;
			Body	= body;
			Subject	= subject;
		}

		public Message(Jid to, Jid from, MessageType type, string body, string subject, string thread) : this()
		{
			To = to;
			From	= from;
			Type	= type;
			Body	= body;
			Subject	= subject;
			Thread	= thread;
		} 

		#endregion

		#region << Properties >>
		/// <summary>
		/// The body of the message. This contains the message text.
		/// </summary>
        public string Body
		{
			set	{ SetTag("body", value); }
			get { return GetTag("body"); }
		}

        /// <summary>
        /// subject of this message. Its like a subject in a email. The Subject is optional.
        /// </summary>
		public string Subject
		{
			set	{ SetTag("subject", value);	}
			get	{ return GetTag("subject");	}
		}

        /// <summary>
        /// messages and conversations could be threaded. You can compare this with threads in newsgroups or forums.
        /// Threads are optional.
        /// </summary>
		public string Thread
		{
			set	{ SetTag("thread", value); }
			get	{ return GetTag("thread"); }
		}

        /// <summary>
        /// message type (chat, groupchat, normal, headline or error).
        /// </summary>
		public MessageType Type
		{
			get 
			{ 
				return (MessageType) GetAttributeEnum("type", typeof(MessageType)); 
			}
			set 
			{ 
				if (value == MessageType.normal)
					RemoveAttribute("type");
				else
					SetAttribute("type", value.ToString()); 
			}
		}

        /// <summary>
        /// Error Child Element
        /// </summary>
        public agsXMPP.protocol.client.Error Error
        {
            get
            {
                return SelectSingleElement(typeof(agsXMPP.protocol.client.Error)) as agsXMPP.protocol.client.Error;

            }
            set
            {
                // set type automatically to error
                Type = MessageType.error;

                if (HasTag(typeof(agsXMPP.protocol.client.Error)))
                    RemoveTag(typeof(agsXMPP.protocol.client.Error));

                if (value != null)
                    this.AddChild(value);
            }
        }

        /// <summary>
        /// The html part of the message if you want to support the html-im Jep. This part of the message is optional.
        /// </summary>
        public Html Html
        {
            get { return (Html) SelectSingleElement(typeof(Html)); }
            set
            {
                RemoveTag(typeof(Html));
                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        /// The event Element for JEP-0022 Message events
        /// </summary>
        public Event XEvent
        {
            get
            {
                return SelectSingleElement(typeof(Event)) as Event;
            }
            set
            {
                if (HasTag(typeof(Event)))
                    RemoveTag(typeof(Event));
                
                if (value != null)
                    this.AddChild(value);
            }
        }


        /// <summary>
        /// The event Element for JEP-0022 Message events
        /// </summary>
        public Delay XDelay
        {
            get
            {
                return SelectSingleElement(typeof(Delay)) as Delay;
            }
            set
            {
                if (HasTag(typeof(Delay)))
                    RemoveTag(typeof(Delay));

                if (value != null)
                    this.AddChild(value);
            }
        }


        /// <summary>
        /// Stanza Headers and Internet Metadata
        /// </summary>
        public Headers Headers
        {
            get
            {
                return SelectSingleElement(typeof(Headers)) as Headers;
            }
            set
            {
                if (HasTag(typeof(Headers)))
                    RemoveTag(typeof(Headers));

                if (value != null)
                    this.AddChild(value);
            }
        }

        /// <summary>
        /// Nickname Element
        /// </summary>
        public Nickname Nickname
        {
            get
            {
                return SelectSingleElement(typeof(Nickname)) as Nickname;
            }
            set
            {
                if (HasTag(typeof(Nickname)))
                    RemoveTag(typeof(Nickname));

                if (value != null)
                    this.AddChild(value);
            }
        }
        
        #region << Chatstate Properties >>   

        public Chatstate Chatstate
        {
            get
            {
                if (HasTag(typeof(Active)))
                    return Chatstate.active;
                else if (HasTag(typeof(Inactive)))
                    return Chatstate.inactive;
                else if (HasTag(typeof(Composing)))
                    return Chatstate.composing;
                else if (HasTag(typeof(Paused)))
                    return Chatstate.paused;
                else if (HasTag(typeof(Gone)))
                    return Chatstate.gone;
                else
                    return Chatstate.None;
            }
            set
            {
                RemoveChatstate();
                switch (value)
                {                    
                    case Chatstate.active:
                        AddChild(new Active());
                        break;
                    case Chatstate.inactive:
                        AddChild(new Inactive());
                        break;
                    case Chatstate.composing:
                        AddChild(new Composing());
                        break;
                    case Chatstate.paused:
                        AddChild(new Paused());
                        break;
                    case Chatstate.gone:
                        AddChild(new Gone());
                        break;
                }
            }
        }

        private void RemoveChatstate()
        {
            RemoveTag(typeof(Active));
            RemoveTag(typeof(Inactive));
            RemoveTag(typeof(Composing));
            RemoveTag(typeof(Paused));
            RemoveTag(typeof(Gone));
        }      
        #endregion

        #endregion

        #region << Methods and Functions >>
#if !CF
        /// <summary>
        /// Create a new unique Thread indendifier
        /// </summary>
        /// <returns></returns>
        public string CreateNewThread()
        {
            string guid = Guid.NewGuid().ToString().ToLower();
            Thread = guid;
            
            return guid;            
        }
#endif        
        #endregion
    }
}