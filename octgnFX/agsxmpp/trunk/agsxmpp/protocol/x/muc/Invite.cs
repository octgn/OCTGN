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
using agsXMPP.protocol.extensions.nickname;

namespace agsXMPP.protocol.x.muc
{
    /*
    <message
        from='crone1@shakespeare.lit/desktop'
        to='darkcave@macbeth.shakespeare.lit'>
      <x xmlns='http://jabber.org/protocol/muc#user'>
        <invite to='hecate@shakespeare.lit'>
          <reason>
            Hey Hecate, this is the place for all good witches!
          </reason>
        </invite>
      </x>
    </message>
    */

    /// <summary>
    /// Invite other users t a chatroom
    /// </summary>
    public class Invite : Invitation
    {
        #region << Constructors >>
        public Invite() : base()
        {
            this.TagName    = "invite";            
        }
        
        public Invite(string reason) : this()
        {
            this.Reason = reason;
        }

        public Invite(Jid to) : this()
        {
            this.To = to;
        }

        public Invite(Jid to, string reason): this()
        {            
            this.To     = to;
            this.Reason = reason;
        }
        #endregion

        /*
            <invite to='wiccarocks@shakespeare.lit/laptop'>
                <reason>This coven needs both wiccarocks and hag66.</reason>
                <continue/>
            </invite>
         */
 
        public bool Continue
        {
            get
            {
                return GetTag("continue") == null ? false : true;
            }
            set
            {
                if (value)
                    SetTag("continue");
                else
                    RemoveTag("continue");
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
        

    }

}
