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

namespace agsXMPP.protocol.extensions.pubsub
{
    /*
        <xs:attribute name='access' use='optional' default='open'>
            <xs:simpleType>
              <xs:restriction base='xs:NCName'>
                <xs:enumeration value='authorize'/>
                <xs:enumeration value='open'/>
                <xs:enumeration value='presence'/>
                <xs:enumeration value='roster'/>
                <xs:enumeration value='whitelist'/>
              </xs:restriction>
            </xs:simpleType>
        </xs:attribute>
    */
    public enum Access
    {
        NONE        = -1,
        open,
        authorize,
        presence,
        roster,
        whitelist
    }
}
