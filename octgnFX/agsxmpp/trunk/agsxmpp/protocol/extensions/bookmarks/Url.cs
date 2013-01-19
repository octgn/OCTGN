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

using agsXMPP.Xml.Dom;

namespace agsXMPP.protocol.extensions.bookmarks
{
    /// <summary>
    /// URLs are fairly simple, as they only need to store a URL and a title, 
    /// and the client then can simply launch the appropriate browser.
    /// </summary>
    public class Url : Element
    {
        /*
            <url name='Complete Works of Shakespeare'
         url='http://the-tech.mit.edu/Shakespeare/'/>
        */
        public Url()
        {
            this.TagName    = "url";
            this.Namespace  = Uri.STORAGE_BOOKMARKS;   
        }

        public Url(string address, string name) : this()
        {
            Address = address;
            Name    = name;
        }

        /// <summary>
        /// A description/name for this bookmark
        /// </summary>
        public string Name
        {
            get { return GetAttribute("name"); }
            set { SetAttribute("name", value); }
        }

        /// <summary>
        /// The url address to store e.g. http://www.ag-software,de/
        /// </summary>
        public string Address
        {
            get { return GetAttribute("url"); }
            set { SetAttribute("url", value); }
        }
    }
}
