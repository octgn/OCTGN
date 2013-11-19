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

using System.Collections.Generic;
using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.x.muc
{
	/// <summary>
	/// Summary description for MucUser.
	/// </summary>
	public class User : Element
	{
        /*
        <x xmlns='http://jabber.org/protocol/muc#user'>
             <item affiliation='admin' role='moderator'/>
        </x>
         
        <message from='darkcave@macbeth.shakespeare.lit'
                 to='hag66@shakespeare.lit/pda'
                 type='groupchat'>
            <body>This room is not anonymous.</body>
            <x xmlns='http://jabber.org/protocol/muc#user'>
                <status code='100'/>
            </x>
        </message>
         
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
         
        <message
            from='darkcave@macbeth.shakespeare.lit'
            to='hecate@shakespeare.lit'>
          <body>You have been invited to darkcave@macbeth by crone1@shakespeare.lit.</body>
          <x xmlns='http://jabber.org/protocol/muc#user'>
            <invite from='crone1@shakespeare.lit'>
              <reason>
                Hey Hecate, this is the place for all good witches!
              </reason>
            </invite>
            <password>cauldron</password>
          </x>
          <x jid='darkcave@macbeth.shakespeare.lit' xmlns='jabber:x:conference'>
            Hey Hecate, this is the place for all good witches!
          </x>
        </message>
        
        */
        public User()
		{
			TagName	= "x";
			Namespace	= Uri.MUC_USER;
		}

        public Item Item
        {
            get
            {
                return SelectSingleElement(typeof(Item)) as Item;
            }
            set
            {
                RemoveTag(typeof(Item));
                AddChild(value);
            }
        }
        
        /// <summary>
        /// Gets or sets the status codes.
        /// </summary>
        /// <value>The status codes.</value>
        public List<Status> StatusCodes
        {
            get { return SelectElements<Status>(); }
            set
            {
                RemoveTags<Status>();
                foreach (Status status in value)
                    AddChild(status);
            }
        }

        /// <summary>
        /// The Status Element
        /// </summary>
        public Status Status
        {
            get { return SelectSingleElement(typeof(Status)) as Status; }
            set
            {
                if (HasTag(typeof(Status)))
                    RemoveTag(typeof(Status));
                
                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        /// The Invite Element
        /// </summary>
        public Invite Invite
        {
            get { return SelectSingleElement(typeof(Invite)) as Invite; }
            set
            {
                if (HasTag(typeof(Invite)))
                    RemoveTag(typeof(Invite));

                if (value != null)
                    AddChild(value);
            }
        }

        /// <summary>
        /// The Decline Element
        /// </summary>
        public Decline Decline
        {
            get { return SelectSingleElement(typeof(Decline)) as Decline; }
            set
            {
                if (HasTag(typeof(Decline)))
                    RemoveTag(typeof(Decline));

                if (value != null)
                    AddChild(value);
            }
        }

        public string Password
        {
            set { SetTag("password", value); }
            get { return GetTag("password"); }
        }
	}
}