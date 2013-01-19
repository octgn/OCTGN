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

namespace agsXMPP.protocol.extensions.pubsub.@event
{
    public class Event : Element
    {
        public Event()
        {
            this.TagName    = "event";
            this.Namespace  = Uri.PUBSUB_EVENT;
        }


        public Delete Delete
        {
            get
            {
                return SelectSingleElement(typeof(Delete)) as Delete;

            }
            set
            {
                if (HasTag(typeof(Delete)))
                    RemoveTag(typeof(Delete));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Purge Purge
        {
            get
            {
                return SelectSingleElement(typeof(Purge)) as Purge;

            }
            set
            {
                if (HasTag(typeof(Purge)))
                    RemoveTag(typeof(Purge));
                
                if (value != null)
                    this.AddChild(value);
            }
        }

        public Items Items
        {
            get
            {
                return SelectSingleElement(typeof(Items)) as Items;
            }
            set
            {
                if (HasTag(typeof(Items)))
                    RemoveTag(typeof(Items));

                if (value != null)
                    this.AddChild(value);
            }
        }
    }
}
