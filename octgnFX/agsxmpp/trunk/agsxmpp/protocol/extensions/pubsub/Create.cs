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

namespace agsXMPP.protocol.extensions.pubsub
{
    public class Create : PubSubAction
    {
        /*
        <iq type="set"
            from="pgm@jabber.org"
            to="pubsub.jabber.org"
            id="create1">
            <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <create node="generic/pgm-mp3-player"/>
                <configure/>
            </pubsub>
        </iq>
         
        ...
            <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <create node="test"
	                    type="collection"/>
            </pubsub>
        ...
        
        */

        #region << Constructors >>
        public Create() : base()
        {
            this.TagName = "create";            
        }

        public Create(string node) : this()
        {
            this.Node = node;
        }

        public Create(Type type) : this()
        {
            this.Type = type;
        }

        public Create(string node, Type type) : this(node)
        {
            this.Type = type;
        }
        #endregion

       
    }
}
