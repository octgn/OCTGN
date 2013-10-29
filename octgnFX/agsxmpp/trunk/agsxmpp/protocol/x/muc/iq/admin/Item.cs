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

namespace agsXMPP.protocol.x.muc.iq.admin
{
    public class Item : agsXMPP.protocol.x.muc.Item
    {
        /// <summary>
        /// 
        /// </summary>
        public Item() : base()
        {
            this.Namespace = Uri.MUC_ADMIN;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliation"></param>
        public Item(Affiliation affiliation) : this()
        {
            this.Affiliation = affiliation;
        }

        public Item(Affiliation affiliation, Jid jid) : this(affiliation)
        {
            this.Jid = jid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        public Item(Role role) : this()
        {
            this.Role = role;
        }

        public Item(Role role, Jid jid) : this(role)
        {
            this.Jid = jid;
        }

        public Item(Jid jid) : this()
        {
            this.Jid = jid;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliation"></param>
        /// <param name="role"></param>
        public Item(Affiliation affiliation, Role role) : this(affiliation)
        {
            this.Role = role;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliation"></param>
        /// <param name="role"></param>
        /// <param name="jid"></param>
        public Item(Affiliation affiliation, Role role, Jid jid) : this(affiliation, role)
        {
            this.Jid = jid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliation"></param>
        /// <param name="role"></param>
        /// <param name="reason"></param>
        public Item(Affiliation affiliation, Role role, string reason) : this(affiliation, role)
        {
            this.Reason = reason;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affiliation"></param>
        /// <param name="role"></param>
        /// <param name="jid"></param>
        /// <param name="reason"></param>
        public Item(Affiliation affiliation, Role role, Jid jid, string reason) : this(affiliation, role, jid)
        {
            this.Reason = reason;
        }

    }
}
