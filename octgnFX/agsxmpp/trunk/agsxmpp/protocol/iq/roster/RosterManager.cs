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

using agsXMPP.protocol.client;

namespace agsXMPP.protocol.iq.roster
{
	/// <summary>
	/// Helper class that makes it easier to manage your contact list.
	/// </summary>
	public class RosterManager
	{
		private readonly XmppClientConnection	m_connection;

	    #region << Constructors >>
	    /// <summary>
		/// Constructor
		/// </summary>
		/// <param name="con">The XmppClientConnection on which the RosterManager should send the packets</param>
		public RosterManager(XmppClientConnection con)
		{		
			m_connection = con;
		}

	    #endregion

	    #region << Add Contact >>

	    /// <summary>
	    /// Add a contact to the Roster
	    /// </summary>
	    /// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
	    public void AddRosterItem(Jid jid)
	    {
	        AddRosterItem(jid, null, new string[] {});
	    }

	    /// <summary>
	    /// Add a contact to the Roster
	    /// </summary>
	    /// <param name="jid">The BARE jid of the contact that should be added.</param>
	    /// <param name="nickname">Nickname for the new contact.</param>
	    public void AddRosterItem(Jid jid, string nickname)
	    {
	        AddRosterItem(jid, nickname, new string[] {});
	    }

	    /// <summary>
	    /// Add a contact to the Roster
	    /// </summary>
	    /// <param name="jid">The BARE jid of the contact that should be added.</param>
	    /// <param name="nickname">Nickname for the new contact.</param>
	    /// <param name="group">The group to which the contact should be added.</param>
	    public void AddRosterItem(Jid jid, string nickname, string group)
	    {
	        AddRosterItem(jid, nickname, new string[] {group});
	    }

	    /// <summary>
	    /// Add a contact to the Roster.
	    /// </summary>
	    /// <param name="jid">The BARE jid of the contact that should be added.</param>
	    /// <param name="nickname">Nickname for the contact.</param>
	    /// <param name="group">An Array of groups when you want to add the contact to multiple groups.</param>
	    public void AddRosterItem(Jid jid, string nickname, string[] group)
	    {
	        RosterIq riq = new RosterIq();
	        riq.Type = IqType.set;
				
	        RosterItem ri = new RosterItem();
	        ri.Jid	= jid;
			
	        if (nickname != null)
	            ri.Name	= nickname;
			
	        foreach (string g in group)
	        {
	            ri.AddGroup(g);			
	        }

	        riq.Query.AddRosterItem(ri);
				
	        m_connection.Send(riq);
	    }

	    #endregion
        
        #region << Update contact >>
        /// <summary>
        /// Update a contact
        /// </summary>
        /// <param name="jid"></param>
        public void UpdateRosterItem(Jid jid)
        {
            AddRosterItem(jid, null, new string[] { });
        }

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <param name="jid">The BARE jid of the contact that should be updated.</param>
        /// <param name="nickname">Nickname for the contact to update.</param>
        public void UpdateRosterItem(Jid jid, string nickname)
        {
            AddRosterItem(jid, nickname, new string[] { });
        }

        /// <summary>
        /// Update a contact
        /// </summary>
        /// <param name="jid"></param>
        /// <param name="nickname"></param>
        /// <param name="group"></param>
        public void UpdateRosterItem(Jid jid, string nickname, string group)
        {
            AddRosterItem(jid, nickname, new string[] { group });
        }

        /// <summary>
        /// Update a contact.
        /// </summary>
        /// <param name="jid">The BARE jid of the contact that should be updated.</param>
        /// <param name="nickname">Nickname for the contact to update.</param>
        /// <param name="group">An Array of groups when you want to add the contact to multiple groups.</param>
        public void UpdateRosterItem(Jid jid, string nickname, string[] group)
        {
            AddRosterItem(jid, nickname, group);
        }
        #endregion

	    #region << Remove Contact >>
	    /// <summary>
        /// Removes a contact from the Roster
        /// </summary>
        /// <param name="jid">The BARE jid of the rosteritem that should be removed</param>
        public void RemoveRosterItem(Jid jid)
        {
            RosterIq riq = new RosterIq();
            riq.Type = IqType.set;

            RosterItem ri = new RosterItem();
            ri.Jid = jid;
            ri.Subscription = SubscriptionType.remove;

            riq.Query.AddRosterItem(ri);

            m_connection.Send(riq);
        }
	    #endregion
	}
}
