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

namespace agsXMPP.protocol.extensions.pubsub.owner
{
    public class PubSub : Element
    {
        public PubSub()
        {
            this.TagName    = "pubsub";
            this.Namespace  = Uri.PUBSUB_OWNER;
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

        public Subscribers Subscribers
        {
            get
            {
                return SelectSingleElement(typeof(Subscribers)) as Subscribers;

            }
            set
            {
                if (HasTag(typeof(Subscribers)))
                    RemoveTag(typeof(Subscribers));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Affiliates Affiliates
        {
            get
            {
                return SelectSingleElement(typeof(Affiliates)) as Affiliates;

            }
            set
            {
                if (HasTag(typeof(Affiliates)))
                    RemoveTag(typeof(Affiliates));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Configure Configure
        {
            get
            {
                return SelectSingleElement(typeof(Configure)) as Configure;

            }
            set
            {
                if (HasTag(typeof(Configure)))
                    RemoveTag(typeof(Configure));

                if (value != null)
                    this.AddChild(value);
            }
        }
       
    }
}