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

namespace agsXMPP.protocol.extensions.pubsub
{
    public class PubSub : Element
    {
        public PubSub()
        {
            this.TagName    = "pubsub";
            this.Namespace  = Uri.PUBSUB;
        }
        
        /// <summary>
        /// the Create Element of the Pubsub Element 
        /// </summary>
        public Create Create
        {
            get
            {
                return SelectSingleElement(typeof(Create)) as Create;
            }
            set
            {
                if (HasTag(typeof(Create)))
                    RemoveTag(typeof(Create));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Publish Publish
        {
            get
            {
                return SelectSingleElement(typeof(Publish)) as Publish;

            }
            set
            {
                if (HasTag(typeof(Publish)))
                    RemoveTag(typeof(Publish));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Retract Retract
        {
            get
            {
                return SelectSingleElement(typeof(Retract)) as Retract;

            }
            set
            {
                if (HasTag(typeof(Retract)))
                    RemoveTag(typeof(Retract));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Subscribe Subscribe
        {
            get
            {
                return SelectSingleElement(typeof(Subscribe)) as Subscribe;

            }
            set
            {
                if (HasTag(typeof(Subscribe)))
                    RemoveTag(typeof(Subscribe));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Unsubscribe Unsubscribe
        {
            get
            {
                return SelectSingleElement(typeof(Unsubscribe)) as Unsubscribe;

            }
            set
            {
                if (HasTag(typeof(Unsubscribe)))
                    RemoveTag(typeof(Unsubscribe));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Subscriptions Subscriptions
        {
            get
            {
                return SelectSingleElement(typeof(Subscriptions)) as Subscriptions;

            }
            set
            {
                if (HasTag(typeof(Subscriptions)))
                    RemoveTag(typeof(Subscriptions));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Affiliations Affiliations
        {
            get
            {
                return SelectSingleElement(typeof(Affiliations)) as Affiliations;

            }
            set
            {
                if (HasTag(typeof(Affiliations)))
                    RemoveTag(typeof(Affiliations));

                if (value != null)
                    this.AddChild(value);
            }
        }

        public Options Options
        {
            get
            {
                return SelectSingleElement(typeof(Options)) as Options;

            }
            set
            {
                if (HasTag(typeof(Options)))
                    RemoveTag(typeof(Options));

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

        /// <summary>
        /// The Configure Element of the PunSub Element
        /// </summary>
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
