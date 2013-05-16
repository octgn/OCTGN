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

namespace agsXMPP.protocol.extensions.commands
{
    /*
        <note type='info'>Service 'httpd' has been configured.</note>
        
        <xs:element name='note'>
            <xs:complexType>
              <xs:simpleContent>
                <xs:extension base='xs:string'>
                  <xs:attribute name='type' use='required'>
                    <xs:simpleType>
                      <xs:restriction base='xs:NCName'>
                        <xs:enumeration value='error'/>
                        <xs:enumeration value='info'/>
                        <xs:enumeration value='warn'/>
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:attribute>
                </xs:extension>
              </xs:simpleContent>
            </xs:complexType>
        </xs:element>
    */

    public class Note : Element
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Note()
        {
            this.TagName    = "note";
            this.Namespace  = Uri.COMMANDS;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public Note(NoteType type) : this()
        {
            this.Type = type;   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        public Note(string text, NoteType type) : this(type)
        {
            this.Value = text;            
        }

        public NoteType Type
        {
            get { return (NoteType)GetAttributeEnum("type", typeof(NoteType)); }
            set { SetAttribute("type", value.ToString()); }
        }

        
    }
}
