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

namespace agsXMPP.protocol.extensions.jivesoftware.phone
{
    /* 
     *
     *
     * <message from="x" id="1137178511.247" to="y">
     *      <phone-event xmlns="http://jivesoftware.com/xmlns/phone" callID="1137178511.247" type="ON_PHONE" device="SIP/3001">
     *          <callerID></callerID>
     *      </phone-event>
     * </message>
     * 
     */

    /// <summary>
    /// Events are sent to the user when their phone is ringing, when a call ends, etc. 
    /// This packet is send within a message packet (subelement of message)
    /// </summary>
    public class PhoneEvent : Element
    {
        #region << Constructors >>
        public PhoneEvent()
        {
            this.TagName    = "phone-event";
            this.Namespace  = Uri.JIVESOFTWARE_PHONE;
        }

        public PhoneEvent(PhoneStatusType status) : this()
        {
            Type = status;
        }

        public PhoneEvent(PhoneStatusType status, string device) : this(status)            
        {
            Device = device;
        }

        public PhoneEvent(PhoneStatusType status, string device, string id) : this(status, device)
        {
            CallId = id;
        }

        public PhoneEvent(PhoneStatusType status, string device, string id, string callerId): this(status, device, id)
        {
            CallerId = callerId;
        }        
        #endregion

        public string CallId
        {
            get { return GetAttribute("callID"); }
            set { SetAttribute("callID", value); }
        }

        public string Device
        {
            get { return GetAttribute("device"); }
            set { SetAttribute("device", value); }
        }

        public PhoneStatusType Type
        {
            set
            {
                SetAttribute("type", value.ToString());
            }
            get
            {
                return (PhoneStatusType)GetAttributeEnum("type", typeof(PhoneStatusType));
            }
        }

        public string CallerId
        {
            get { return GetTag("callerID"); }
            set { SetTag("callerID", value); }
        }

        public string CallerIdName
        {
            get { return GetTag("callerIDName"); }
            set { SetTag("callerIDName", value); }
        }
    }
}