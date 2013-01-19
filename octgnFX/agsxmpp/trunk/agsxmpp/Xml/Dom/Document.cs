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

using System.IO;

namespace agsXMPP.Xml.Dom
{
	/// <summary>
	/// 
	/// </summary>
	public class Document : Node
	{
		public Document()
		{
			NodeType = NodeType.Document;
		}

		public Element RootElement
		{
			get
			{
				foreach (Node n in this.ChildNodes)
				{
					if(n.NodeType == NodeType.Element)
						return n as Element;
				}
				return null;
			}
		}

		#region << Properties and Member Variables >>
		private string	m_Encoding	= null;
		private string	m_Version	= null;

		public string Encoding
		{
			get { return m_Encoding; }
			set { m_Encoding = value; }
		}

		public string Version
		{
			get { return m_Version; }
			set { m_Version = value; }
		}
		#endregion

		/// <summary>
		/// Clears the Document
		/// </summary>
		public void Clear()
		{
			this.ChildNodes.Clear();
		}

		#region << Load functions >>		
		public void LoadXml(string xml)
		{
            if (!string.IsNullOrEmpty(xml))
            {
                DomLoader.Load(xml, this);
            }
		}

		public bool LoadFile(string filename)
		{
		    if (File.Exists(filename))
			{
				try
				{
					using(var sr = new StreamReader(filename))
                    {
					    DomLoader.Load(sr, this);
					    sr.Close();					
					    return true;
                    }
				}
				catch
				{
					return false;
				}
			}
		    return false;
		}

	    public bool LoadStream(Stream stream)
        {
            try
            {
                using(var sr = new StreamReader(stream))
                {
                    DomLoader.Load(sr, this);
                    sr.Close();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
		
		public void Save(string filename)
		{
			using (var w = new StreamWriter(filename))
            {
			    w.Write(ToString(System.Text.Encoding.UTF8));
			    w.Flush();
			    w.Close();
            }
		}
		#endregion
	}
}
