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
    /*
     * 
     * Note: If the initiating entity did not understand any of the advertised compression methods, 
     * it SHOULD ignore the compression option and proceed as if no compression methods were advertised.
     * 
     * If the initiating entity requests a stream compression method that is not supported by the 
     * receiving entity, the receiving entity MUST return an <unsupported-method/> error:
     * 
     * Example 3. Receiving Entity Reports That Method is Unsupported
     * <failure xmlns='http://jabber.org/protocol/compress'>
     *  <unsupported-method/>
     * </failure>
     * 
     * If the receiving entity cannot establish compression using the requested method for any 
     * other reason, it MUST return a <setup-failed/> error:
     * 
     * Example 4. Receiving Entity Reports That Compression Setup Failed
     * <failure xmlns='http://jabber.org/protocol/compress'>
     *  <setup-failed/>
     * </failure>
     */

    public class Failure : Element
    {
        public Failure()
        {
            this.TagName    = "failure";
            this.Namespace  = Uri.COMPRESS;
        }
    }
}
