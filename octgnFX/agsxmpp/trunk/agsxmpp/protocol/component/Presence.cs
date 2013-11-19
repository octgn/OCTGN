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

#region Using directives

using System;

#endregion

namespace agsXMPP.protocol.component
{
    /// <summary>
    /// Summary description for Presence.
    /// </summary>
    public class Presence : agsXMPP.protocol.client.Presence
    {
        #region << Constructors >>
        public Presence() : base()
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Presence(agsXMPP.protocol.client.ShowType show, string status) : this()
        {
            this.Show = show;
            this.Status = status;
        }

        public Presence(agsXMPP.protocol.client.ShowType show, string status, int priority) : this(show, status)
        {
            this.Priority = priority;
        }
        #endregion

        /// <summary>
        /// Error Child Element
        /// </summary>
        public new agsXMPP.protocol.component.Error Error
        {
            get
            {
                return SelectSingleElement(typeof(agsXMPP.protocol.component.Error)) as agsXMPP.protocol.component.Error;

            }
            set
            {
                if (HasTag(typeof(agsXMPP.protocol.component.Error)))
                    RemoveTag(typeof(agsXMPP.protocol.component.Error));

                if (value != null)
                    this.AddChild(value);
            }
        }
    }
}
