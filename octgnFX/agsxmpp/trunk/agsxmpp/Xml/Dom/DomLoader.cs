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
	/// internal class that loads a xml document from a string or stream
	/// </summary>
	internal class DomLoader
	{	
		public static void Load(string xml, Document doc)
		{
            var sp =new StreamParser();

            sp.OnStreamStart += (sender, node) => doc.ChildNodes.Add(node);
            sp.OnStreamElement += (sender, node) => doc.RootElement.ChildNodes.Add(node);
            
            
			byte[] b = System.Text.Encoding.UTF8.GetBytes(xml);
			sp.Push(b, 0, b.Length);
		}

		public static void Load(StreamReader sr, Document doc)
		{
		    Load(sr.ReadToEnd(), doc);
		}
	}
}