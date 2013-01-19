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

namespace agsXMPP.protocol.extensions.bytestreams
{
    public class Activate : Element
    {
        public Activate()
        {
            this.TagName    = "activate";
            this.Namespace  = Uri.BYTESTREAMS;
        }

        public Activate(Jid jid) : this()
        {
            Jid = jid;
        }

        /// <summary>
        /// the full JID of the Target to activate
        /// </summary>
        public Jid Jid
        {
            get
            {
                if (Value == null)
                    return null;
                else
                    return new Jid(Value);
            }
            set
            {
                if (value != null)
                    Value = value.ToString();
                else
                    Value = null;
            }
        }
    }
}