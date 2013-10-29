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

namespace agsXMPP.protocol.extensions.filetransfer
{
	/// <summary>
	/// When range is sent in the offer, it should have no attributes. 
	/// This signifies that the sender can do ranged transfers.
	/// When no range element is sent in the Stream Initiation result, the Sender MUST send the complete file starting at offset 0.
	/// More generally, data is sent over the stream byte for byte starting at the offset position for the length specified.
	/// </summary>
	public class Range : Element
	{
		/*
		<range offset='252' length='179'/>		    	
		*/
		public Range()
		{
			this.TagName	= "range";
			this.Namespace	= Uri.SI_FILE_TRANSFER;
		}

		public Range(long offset, long length) : this()
		{
			Offset	= offset;
			Length	= length;
		}

		/// <summary>
		/// Specifies the position, in bytes, to start transferring the file data from.
		/// This defaults to zero (0) if not specified.
		/// </summary>
		public long Offset
		{
			get { return GetAttributeLong("offset"); }
			set { SetAttribute("offset", value.ToString());}
		}
		
		/// <summary>
		/// Specifies the number of bytes to retrieve starting at offset.
		/// This defaults to the length of the file from offset to the end.
		/// </summary>
		public long Length
		{
			get { return GetAttributeLong("length"); }
			set { SetAttribute("length", value.ToString());}
		}
	}
}
