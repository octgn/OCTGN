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
    /// <summary>
    /// The Body Element of a XHTML message
    /// </summary>
    public class Body : Element
    {
        public Body()
        {
            this.TagName    = "body";
            this.Namespace  = Uri.XHTML;
        }

        /// <summary>
        /// 
        /// </summary>
        public string InnerHtml
        {
            get
            {
                // Thats a HACK
                string xml = this.ToString();
                
                int start   = xml.IndexOf(">");
                int end     = xml.LastIndexOf("</" + this.TagName + ">");

                return xml.Substring(start + 1, end - start -1);
            }
        }
    }
}