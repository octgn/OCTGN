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

namespace agsXMPP.protocol.iq.privacy
{
    public class Privacy : Element
    {
        public Privacy()
        {
            this.TagName    = "query";
            this.Namespace  = Uri.IQ_PRIVACY;
        }
        
        /// <summary>
        /// Add a provacy list
        /// </summary>
        /// <param name="list"></param>
        public void AddList(List list)
        {
            this.AddChild(list);
        }

        /// <summary>
        /// Get all Lists
        /// </summary>
        /// <returns>Array of all privacy lists</returns>
        public List[] GetList()
        {
            ElementList el = SelectElements(typeof(List));
            int i = 0;
            List[] result = new List[el.Count];
            foreach (List list in el)
            {
                result[i] = list;
                i++;
            }
            return result;
        }

        /// <summary>
        /// The active list
        /// </summary>
        public Active Active
        {
            get
            {
                return SelectSingleElement(typeof(Active)) as Active;
            }
            set
            {
                if (HasTag(typeof(Active)))
                    RemoveTag(typeof(Active));

                if (value != null)
                    this.AddChild(value);
            }
        }

        /// <summary>
        /// The default list
        /// </summary>
        public Default Default
        {
            get
            {
                return SelectSingleElement(typeof(Default)) as Default;
            }
            set
            {
                if (HasTag(typeof(Default)))
                    RemoveTag(typeof(Default));

                this.AddChild(value);
            }
        }
    }
}
