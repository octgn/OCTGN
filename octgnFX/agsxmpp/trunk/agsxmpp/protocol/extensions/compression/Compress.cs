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

namespace agsXMPP.protocol.extensions.compression
{
    // <compress xmlns="http://jabber.org/protocol/compress">
    //      <method>zlib</method>
    // </compress>

    public class Compress : Element
    {
        #region << Constructors >>
        public Compress()
        {
            this.TagName = "compress";
            this.Namespace = Uri.COMPRESS;
        }

        /// <summary>
        /// Constructor with a given method/algorithm for Stream compression
        /// </summary>
        /// <param name="method">method/algorithm used to compressing the stream</param>
        public Compress(CompressionMethod method) : this()
        {
            this.Method = method;
        }
        #endregion

        /// <summary>
        /// method/algorithm used to compressing the stream
        /// </summary>
        public CompressionMethod Method
        {
            set 
            {
                if (value != CompressionMethod.Unknown)
					SetTag("method", value.ToString());  
            }
            get 
            {
                return (CompressionMethod) GetTagEnum("method", typeof(CompressionMethod));            
            }
        }
    }
}
