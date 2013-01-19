/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *																					 *
 * Copyright (c) 2005-2009 by AG-Software												 *
 * All Rights Reserved.																 *
 *																					 *
 * You should have received a copy of the AG-Software Shared Source License			 *
 * along with this library; if not, email gnauck@ag-software.de to request a copy.   *
 *																					 *
 * For general enquiries, email gnauck@ag-software.de or visit our website at:		 *
 * http://www.ag-software.de														 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;

namespace agsXMPP.ui.xdata
{
	/// <summary>
	/// Summary description for XDataListItem.
	/// </summary>
	public class XDataListItem : Object
	{
		private string m_Label		= null;
		private string m_Value		= null;

		public XDataListItem()
		{		
		}
		
		public XDataListItem(string label, string val)
		{
			m_Value = val;
			m_Label	= label;
		}

		public string Label
		{
			get { return m_Label; }
			set { m_Label = value; }
		}

		public string Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}

		public override string ToString()
		{
			return m_Label;
		}
	}
}