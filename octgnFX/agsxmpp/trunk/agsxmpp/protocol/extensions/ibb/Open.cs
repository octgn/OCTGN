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

namespace agsXMPP.protocol.extensions.ibb
{
    /*
        <open sid='mySID' 
            block-size='4096'
            xmlns='http://jabber.org/protocol/ibb'/>
     
       <xs:element name='open'>
         <xs:complexType>
          <xs:simpleContent>
            <xs:extension base='empty'>
              <xs:attribute name='sid' type='xs:string' use='required'/>
              <xs:attribute name='block-size' type='xs:string' use='required'/>
            </xs:extension>
          </xs:simpleContent>
         </xs:complexType>
       </xs:element>
    */
    public class Open : Base
    {
        /// <summary>
        /// 
        /// </summary>
        public Open()
        {
            this.TagName    = "open";            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="blocksize"></param>
        public Open(string sid, long blocksize) : this()
        {
            this.Sid        = sid;
            this.BlockSize  = blocksize;
        }  

        /// <summary>
        /// Block size
        /// </summary>
        public long BlockSize
        {
            get { return GetAttributeLong("block-size"); }
            set { SetAttribute("block-size", value); }
        }
    }
}