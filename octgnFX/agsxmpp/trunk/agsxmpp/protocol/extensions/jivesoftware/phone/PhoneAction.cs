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
    public class PhoneAction : Element
    {
        /*
         * Actions are sent by the client to perform tasks such as dialing, checking for messages, etc. Actions are sent as IQ's (type set), as with the following child stanza:
         * 
         * <phone-action xmlns="http://jivesoftware.com/xmlns/phone" type="DIAL">
         *    <extension>5035555555</extension>
         * </phone-action>
         *          
         * Currently supported types are DIAL and FORWARD.
         * In most implementations, issuing a dial command will cause the user's phone to ring.
         * Once the user picks up, the specified extension will be dialed.
         * 
         * Dialing can also be performed by jid too. The jid must be dialed must be mapped on the server to an extension
         * 
         * <phone-action type="DIAL">
         *  <jid>andrew@jivesoftware.com</jid>
         * </phone-action>
         * 
         * Issuing a action wth a type FORWARD should transfer a call that has already been 
         * established to a third party. The FORWARD type requires an extension or jid child element
         *
         *  <phone-action xmlns="http://jivesoftware.com/xmlns/phone" type="FORWARD">
         *      <extension>5035555555</extension>
         *  </phone-action>
         *
         */


        #region << Constructors >>
        /// <summary>
        /// 
        /// </summary>
        public PhoneAction()
        {
            this.TagName    = "phone-action";
            this.Namespace  = Uri.JIVESOFTWARE_PHONE;
        }

        public PhoneAction(ActionType type) : this()
        {
            Type = type;
        }

        public PhoneAction(ActionType type, string extension) : this(type)
        {            
            Extension = extension;
        }

        public PhoneAction(ActionType type, Jid jid) : this(type)
        {
            Jid = jid;
        }
        #endregion

        public ActionType Type
        {
            set
            {
                SetAttribute("type", value.ToString());
            }
            get
            {
                return (ActionType)GetAttributeEnum("type", typeof(ActionType));
            }
        }

        public string Extension
        {
            get { return GetTag("extension"); }
            set { SetTag("extension", value); }
        }

        public Jid Jid
        {
            get { return new Jid(GetTag("jid")); }
            set { SetTag("jid", value.ToString()); }
        }

    }
}