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

namespace agsXMPP.protocol.x.data
{

	/// <summary>
	/// Form Types
	/// </summary>
	public enum XDataFormType
	{
		/// <summary>
		/// The forms-processing entity is asking the forms-submitting entity to complete a form.
		/// </summary>
		form,
		/// <summary>
		/// The forms-submitting entity is submitting data to the forms-processing entity.
		/// </summary>
		submit,
		/// <summary>
		/// The forms-submitting entity has cancelled submission of data to the forms-processing entity.
		/// </summary>
		cancel,
		/// <summary>
		/// The forms-processing entity is returning data (e.g., search results) to the forms-submitting entity, or the data is a generic data set.
		/// </summary>
		result    
	}

	/// <summary>
	/// Summary for Data.
	/// </summary>
	public class Data : FieldContainer
	{
		/*
		The base syntax for the 'jabber:x:data' namespace is as follows (a formal description can be found in the XML Schema section below):
		
		<x xmlns='jabber:x:data'
		type='{form-type}'>
		<title/>
		<instructions/>
		<field var='field-name'
				type='{field-type}'
				label='description'>
			<desc/>
			<required/>
			<value>field-value</value>
			<option label='option-label'><value>option-value</value></option>
			<option label='option-label'><value>option-value</value></option>
		</field>
		</x>
		
		*/
			
		#region << Constructors >>
		public Data()
		{
			this.TagName	= "x";
			this.Namespace	= Uri.X_DATA;			
		}

		public Data(XDataFormType type) : this()
		{
			this.Type = type;
		}
		#endregion

		#region << Properties >>
		public string Title
		{
			get { return GetTag("title"); }
			set { SetTag("title", value); }
		}

		public string Instructions
		{
			get { return GetTag("instructions"); }
			set { SetTag("instructions", value); }
		}

		/// <summary>
		/// Type of thie XDATA Form.
		/// </summary>
		public XDataFormType Type
		{
			get 
			{ 
				return (XDataFormType) GetAttributeEnum("type", typeof(XDataFormType)); 
			}
			set { SetAttribute("type", value.ToString());}
		}

        public Reported Reported
        {
            get { return SelectSingleElement(typeof(Reported)) as Reported; }
            set 
            {
                RemoveTag(typeof(Reported));
                AddChild(value);
            }
        }

		#endregion

        #region << public Methods >>
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Item AddItem()
        {
            Item i = new Item();
            AddChild(i);
            return i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Item AddItem(Item item)
        {
            AddChild(item);
            return item;
        }

        /// <summary>
        /// Gets a list of all form fields
        /// </summary>
        /// <returns></returns>
        public Item[] GetItems()
        {
            ElementList nl = SelectElements(typeof(Item));
            Item[] items = new Item[nl.Count];
            int i = 0;
            foreach (Element e in nl)
            {
                items[i] = (Item) e;
                i++;
            }
            return items;
        }
        #endregion
    }
}
