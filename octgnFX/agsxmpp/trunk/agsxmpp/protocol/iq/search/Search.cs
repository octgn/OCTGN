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

using agsXMPP.protocol.x.data;

using agsXMPP.Xml.Dom;

//	Example 1. Requesting Search Fields
//
//	<iq type='get'
//		from='romeo@montague.net/home'
//		to='characters.shakespeare.lit'
//		id='search1'
//		xml:lang='en'>
//		<query xmlns='jabber:iq:search'/>
//	</iq>
//

//	The service MUST then return the possible search fields to the user, and MAY include instructions:
//
//	Example 2. Receiving Search Fields
//
//	<iq type='result'
//		from='characters.shakespeare.lit'
//		to='romeo@montague.net/home'
//		id='search1'
//		xml:lang='en'>
//		<query xmlns='jabber:iq:search'>
//			<instructions>
//			Fill in one or more fields to search
//			for any matching Jabber users.
//			</instructions>
//			<first/>
//			<last/>
//			<nick/>
//			<email/>
//		</query>
//	</iq>    

namespace agsXMPP.protocol.iq.search
{
	/// <summary>
	/// http://www.jabber.org/jeps/jep-0055.html
	/// </summary>
	public class Search : Element
	{
		public Search()
		{
			this.TagName	= "query";
			this.Namespace	= Uri.IQ_SEARCH;
		}

		public string Instructions
		{
			get
			{
				return GetTag("instructions");
			}
			set
			{
				SetTag("instructions", value);
			}
		}

		public string Firstname
		{
			get
			{
				return GetTag("first");
			}
			set
			{
				SetTag("first", value);
			}
		}
		
		public string Lastname
		{
			get
			{
				return GetTag("last");
			}
			set
			{
				SetTag("last", value);
			}
		}

		public string Nickname
		{
			get
			{
				return GetTag("nick");
			}
			set
			{
				SetTag("nick", value);
			}
		}

		public string Email
		{
			get
			{
				return GetTag("email");
			}
			set
			{
				SetTag("email", value);
			}
		}

        /// <summary>
        /// The X-Data Element
        /// </summary>
        public Data Data
        {
            get
            {
                return SelectSingleElement(typeof(Data)) as Data;

            }
            set
            {
                if (HasTag(typeof(Data)))
                    RemoveTag(typeof(Data));

                if (value != null)
                    this.AddChild(value);
            }
        }

		/// <summary>
		/// Retrieve the result items of a search
		/// </summary>
        //public ElementList GetItems
        //{
        //    get
        //    {
        //        return this.SelectElements("item");
        //    }			
        //}

        public SearchItem[] GetItems()
        {
            ElementList nl = SelectElements(typeof(SearchItem));
            SearchItem[] items = new SearchItem[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (SearchItem)e;
                i++;
            }
            return items;
        }
	
	}
}
