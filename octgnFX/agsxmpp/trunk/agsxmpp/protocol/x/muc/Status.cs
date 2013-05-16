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

namespace agsXMPP.protocol.x.muc
{
    /*
    <x xmlns='http://jabber.org/protocol/muc#user'>
        <status code='100'/>
    </x>    
    */

    /// <summary>
    /// Summary description for MucUser.
    /// </summary>
    public class Status : Element
    {
        #region << Constructors >>
        public Status()
        {
            this.TagName    = "status";
            this.Namespace  = Uri.MUC_USER;
        }

        public Status(StatusCode code) : this()
        {
            this.Code = code;
        }

        public Status(int code) : this()
        {
            SetAttribute("code", code);
        }
        #endregion

        public StatusCode Code
		{
			get 
			{
                return (StatusCode)GetAttributeEnum("code", typeof(StatusCode)); 
			}
			set 
			{ 
				SetAttribute("code", value.ToString()); 
			}
		}
    }

}