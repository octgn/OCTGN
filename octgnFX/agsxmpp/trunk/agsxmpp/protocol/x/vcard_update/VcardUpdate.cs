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

namespace agsXMPP.protocol.x.vcard_update
{
    /*
        <presence>
          <x xmlns='vcard-temp:x:update'>
            <photo/>
          </x>
        </presence>
    */
    public class VcardUpdate : Element
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="VcardUpdate"/> class.
        /// </summary>
        public VcardUpdate()
        {
            this.TagName    = "x";
            this.Namespace  = Uri.VCARD_UPDATE;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="VcardUpdate"/> class.
        /// </summary>
        /// <param name="photo">The photo.</param>
        public VcardUpdate(string photo) : this()
        {
            Photo = photo;
        }

        /// <summary>
        /// SHA1 hash of the avatar image data
        /// <para>if no image/avatar should be advertised, or other clients should be forced
        /// to remove the image set it to a empty string value ("")</para>
        /// <para>if this protocol is supported but you ae not ready o advertise a imaeg yet
        /// set teh value to null.</para>
        /// <para>Otherwise teh value must the SHA1 hash of the image data.</para>
        /// </summary>
        public string Photo
        {
            get { return GetTag("photo"); }
            set
            {
                if (value == null)
                    RemoveTag("photo");
                else
                    SetTag("photo", value);
            }
        }
    }
}
