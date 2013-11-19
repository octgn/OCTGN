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

namespace agsXMPP.protocol.component
{
    /// <summary>
    /// Summary description for Error.
    /// </summary>
    public class Error : agsXMPP.protocol.client.Error
    {
        public Error() : base()
        {
            this.Namespace = Uri.ACCEPT;
        }
                
        public Error(int code)
            : base(code)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Error(agsXMPP.protocol.client.ErrorCode code)
            : base(code)
        {
            this.Namespace = Uri.ACCEPT;
        }

        public Error(agsXMPP.protocol.client.ErrorType type)
            : base(type)
        {
            this.Namespace = Uri.ACCEPT;
        }

        /// <summary>
        /// Creates an error Element according the the condition
        /// The type attrib as added automatically as decribed in the XMPP specs
        /// This is the prefered way to create error Elements
        /// </summary>
        /// <param name="condition"></param>
        public Error(agsXMPP.protocol.client.ErrorCondition condition)
            : base(condition)
        {
            this.Namespace = Uri.ACCEPT;
        }
    }
}
