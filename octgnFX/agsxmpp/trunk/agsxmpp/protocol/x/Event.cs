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
    /// <summary>
    /// JEP-0022: Message Events
    /// This JEP defines protocol extensions used to request and respond to events relating to the delivery, display, and composition of messages.
    /// </summary>
    public class Event : Element
    {
        public Event()
        {
            this.TagName    = "x";
            this.Namespace  = Uri.X_EVENT;
        }

        /// <summary>
        /// Indicates that the message has been stored offline by the intended recipient's server.
        /// This event is triggered only if the intended recipient's server supports offline storage, 
        /// has that support enabled, and the recipient is offline when the server receives the message for delivery.
        /// </summary>
        public bool Offline
        {
            get { return HasTag("offline"); }
            set 
            {
                RemoveTag("offline");
                if (value)
                    AddTag("offline");
            }
            
        }

        /// <summary>
        /// Indicates that the message has been delivered to the recipient. 
        /// This signifies that the message has reached the recipient's Jabber client, 
        /// but does not necessarily mean that the message has been displayed. 
        /// This event is to be raised by the Jabber client.
        /// </summary>
        public bool Delivered
        {
            get { return HasTag("delivered"); }
            set 
            {
                RemoveTag("delivered");
                if (value)
                    AddTag("delivered");
            }
            
        }
        
        /// <summary>
        /// Once the message has been received by the recipient's Jabber client, 
        /// it may be displayed to the user. 
        /// This event indicates that the message has been displayed, and is to be raised by the Jabber client.
        /// Even if a message is displayed multiple times, this event should be raised only once.
        /// </summary>
        public bool Displayed
        {
            get { return HasTag("displayed"); }
            set 
            {
                RemoveTag("displayed");
                if (value)
                    AddTag("displayed");
            }
            
        }
        
        /// <summary>
        /// In threaded chat conversations, this indicates that the recipient is composing a reply to a message.
        /// The event is to be raised by the recipient's Jabber client. 
        /// A Jabber client is allowed to raise this event multiple times in response to the same request, 
        /// providing the original event is cancelled first.
        /// </summary>
        public bool Composing
        {
            get { return HasTag("composing"); }
            set 
            {
                RemoveTag("composing");
                if (value)
                    AddTag("composing");
            }
            
        }

        /// <summary>
        /// 'id' attribute of the original message to which this event notification pertains.
        /// (If no 'id' attribute was included in the original message, then the id tag must still be included with no 
        /// </summary>
        public string Id
        {
            get { return GetTag("id"); }
            set { SetTag("id", value); }
        }

   
    }
}