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

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.iq.privacy
{
    /// <summary>
    /// This class represents a rule which is used for blocking communication
    /// </summary>
    public class Item : Element
    {

        #region << Constructors >>
        /// <summary>
        /// Default Contructor
        /// </summary>
        public Item()
        {
            this.TagName    = "item";
            this.Namespace  = Uri.IQ_PRIVACY;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="order"></param>
        public Item(Action action, int order) : this()
        {
            Action  = action;
            Order   = order;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="order"></param>
        /// <param name="block"></param>
        public Item(Action action, int order, Stanza stanza) : this(action, order)
        {
            Stanza = stanza;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="order"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public Item(Action action, int order, Type type, string value) : this(action, order)
        {
            Type    = type;
            Val     = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="order"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <param name="block"></param>
        public Item(Action action, int order, Type type, string value, Stanza stanza) : this(action, order, type, value)
        {
            Stanza = stanza;
        }
        #endregion

        public Action Action
		{
			get 
			{
                return (Action)GetAttributeEnum("action", typeof(Action)); 
			}
			set 
			{ 
				SetAttribute("action", value.ToString()); 
			}
		}

        public Type Type
        {
            get
            {
                return (Type)GetAttributeEnum("type", typeof(Type));
            }
            set
            {
                if (value != Type.NONE)
                    SetAttribute("type", value.ToString());
                else
                    RemoveAttribute("type");
            }
        }

        /// <summary>
        /// The order of this rule
        /// </summary>
        public int Order
        {
            get { return GetAttributeInt("order"); }
            set { SetAttribute("order", value); }
        }

        /// <summary>
        /// The value to match of this rule
        /// </summary>
        public string Val
        {
            get { return GetAttribute("value"); }
            set { SetAttribute("value", value); }
        }

        /// <summary>
        /// Block Iq stanzas
        /// </summary>
        public bool BlockIq
        {
            get { return HasTag("iq"); }
            set
            {
                if (value)
                    this.SetTag("iq");
                else
                    this.RemoveTag("iq");
            }
        }

        /// <summary>
        /// Block messages
        /// </summary>
        public bool BlockMessage
        {
            get { return HasTag("message"); }
            set
            {
                if (value)
                    this.SetTag("message");
                else
                    this.RemoveTag("message");
            }
        }

        /// <summary>
        /// Block incoming presence
        /// </summary>
        public bool BlockIncomingPresence
        {
            get { return HasTag("presence-in"); }
            set
            {
                if (value)
                    this.SetTag("presence-in");
                else
                    this.RemoveTag("presence-in");
            }
        }

        /// <summary>
        /// Block outgoing presence
        /// </summary>
        public bool BlockOutgoingPresence
        {
            get { return HasTag("presence-out"); }
            set
            {
                if (value)
                    this.SetTag("presence-out");
                else
                    this.RemoveTag("presence-out");
            }
        }
        
        /// <summary>
        /// which stanzas should be blocked?
        /// </summary>
        public Stanza Stanza
        {
            get
            {
                Stanza result = Stanza.All;

                if (BlockIq)
                    result |= Stanza.Iq;
                if (BlockMessage) 
                    result |= Stanza.Message;
                if (BlockIncomingPresence)
                    result |= Stanza.IncomingPresence;
                if (BlockOutgoingPresence)
                    result |= Stanza.OutgoingPresence;
                
                return result;
            }
            set
            {
                if (value == Stanza.All)
                {
                    // Block All Communications
                    BlockIq                 = false;
                    BlockMessage            = false;
                    BlockIncomingPresence   = false;
                    BlockOutgoingPresence   = false;
                }
                else
                {
                    BlockIq                 = ((value & Stanza.Iq) == Stanza.Iq);
                    BlockMessage            = ((value & Stanza.Message) == Stanza.Message);
                    BlockIncomingPresence   = ((value & Stanza.IncomingPresence) == Stanza.IncomingPresence);
                    BlockOutgoingPresence   = ((value & Stanza.OutgoingPresence) == Stanza.OutgoingPresence);
                }
            }
        }
    }
}