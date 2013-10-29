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

namespace agsXMPP.protocol.extensions.html
{
    /*
     * <message>
     *      <body>hi!</body>
     *      <html xmlns='http://jabber.org/protocol/xhtml-im'>
     *          <body xmlns='http://www.w3.org/1999/xhtml'>
     *              <p style='font-weight:bold'>hi!</p>
     *          </body>
     *      </html>
     * </message>
     */

    public class Html : Element
    {
        public Html()
        {
            this.TagName    = "html";
            this.Namespace  = Uri.XHTML_IM;
        }
        
        /// <summary>
        /// The Body Element of the XHTML Message
        /// </summary>
        public Body Body
        {
            get
            {
                return SelectSingleElement(typeof(Body)) as Body;

            }
            set
            {
                if (HasTag(typeof(Body)))
                    RemoveTag(typeof(Body));
                
                if (value != null)
                    this.AddChild(value);
            }
        }
    }
}
