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
using agsXMPP.protocol.extensions.compression;

namespace agsXMPP.protocol.stream.feature.compression
{
    public class Method : Element
    {
        /*
         *  <compression xmlns='http://jabber.org/features/compress'>
         *      <method>zlib</method>
         *  </compression>
         * 
         * <stream:features>
         *      <starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls'/>
         *      <compression xmlns='http://jabber.org/features/compress'>
         *          <method>zlib</method>
         *          <method>lzw</method>
         *      </compression>
         * </stream:features>
         */
        #region << Constructors >>
        public Method()
        {
            this.TagName    = "method";
            this.Namespace  = Uri.FEATURE_COMPRESS;
        }

        public Method(CompressionMethod method) : this()
        {
            this.Value      = method.ToString();
        }
        #endregion

        public CompressionMethod CompressionMethod
        {
            get
            {
#if CF
				return (CompressionMethod) util.Enum.Parse(typeof(CompressionMethod), this.Value, true);
#else
                return (CompressionMethod) Enum.Parse(typeof(CompressionMethod), this.Value, true);
#endif
            }
            set { this.Value = value.ToString(); }
        }
    }

}