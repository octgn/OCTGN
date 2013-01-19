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
    /// Summary description for Iq.
    /// </summary>
    public class IQ : agsXMPP.protocol.client.IQ
    {
        #region << Constructors >>
        public IQ() : base()
        {
            this.Namespace = Uri.ACCEPT;
        }

        public IQ(agsXMPP.protocol.client.IqType type) : base(type)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public IQ(Jid from, Jid to) : base(from, to)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public IQ(agsXMPP.protocol.client.IqType type, Jid from, Jid to) : base(type, from, to)
        {
            this.Namespace = Uri.ACCEPT;
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
