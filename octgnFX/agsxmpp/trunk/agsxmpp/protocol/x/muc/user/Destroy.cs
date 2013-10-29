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

namespace agsXMPP.protocol.x.muc.user
{
    public class Destroy : agsXMPP.protocol.x.muc.owner.Destroy
    {
        #region << Constructor >>
        public Destroy() : base()
        {
            this.Namespace = Uri.MUC_USER;
        }

        public Destroy(string reason)
            : this()
        {
            Reason = reason;
        }

        public Destroy(Jid altVenue)
            : this()
        {
            AlternateVenue = altVenue;
        }

        public Destroy(string reason, Jid altVenue)
            : this()
        {
            Reason = reason;
            AlternateVenue = altVenue;
        }
        #endregion  
    }
}
