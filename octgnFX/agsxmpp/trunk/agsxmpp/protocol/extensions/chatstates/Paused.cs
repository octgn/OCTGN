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

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.chatstates
{
    /// <summary>
    /// User had been composing but now has stopped.
    /// User was composing but has not interacted with the message input interface for a short period of time (e.g., 5 seconds).
    /// </summary>
    public class Paused : Element
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Paused"/> class.
        /// </summary>
        public Paused()
        {
            TagName    = Chatstate.paused.ToString();
            Namespace  = Uri.CHATSTATES;
        }
    }
}