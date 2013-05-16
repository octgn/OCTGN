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

using agsXMPP;
using agsXMPP.protocol.client;

namespace agsXMPP.protocol.extensions.pubsub
{
    public class PubSubManager
    {
        private XmppClientConnection	m_connection	= null;

        #region << Constructors >>
        public PubSubManager(XmppClientConnection con)
        {
            m_connection = con;
        }
        #endregion

        #region << Create Instant Node >>
        /*
            Example 6. Client requests an instant node

            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="create2">
                <pubsub xmlns="http://jabber.org/protocol/pubsub">
                    <create/>
                </pubsub>
            </iq>
        */
        
        public void CreateInstantNode(Jid to)
        {
            CreateInstantNode(to, null, null, null);
        }

        public void CreateInstantNode(Jid to, Jid from)
        {
            CreateInstantNode(to, from, null, null);
        }

        public void CreateInstantNode(Jid to, Jid from, IqCB cb)
        {
            CreateInstantNode(to, from, cb, null);
        }

        public void CreateInstantNode(Jid to, IqCB cb)
        {
            CreateInstantNode(to, null, cb, null);
        }

        public void CreateInstantNode(Jid to, Jid from, IqCB cb,object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Create = new Create();

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Create Node >>
        /*
            Example 1. Entity requests a new node with default configuration.

            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="create1">
                <pubsub xmlns="http://jabber.org/protocol/pubsub">
                    <create node="generic/pgm-mp3-player"/>
                    <configure/>
                </pubsub>
            </iq>
        */
        /// <summary>
        /// Create a Node with default configuration
        /// </summary>
        /// <param name="to"></param>
        /// <param name="node"></param>
        public void CreateNode(Jid to, string node)
        {
            CreateNode(to, null, node, true, null, null);
        }

        public void CreateNode(Jid to, Jid from, string node)
        {
            CreateNode(to, from, node, true, null, null);
        }

        /// <summary>
        /// Create a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="defaultConfig"></param>
        public void CreateNode(Jid to, Jid from, string node, bool defaultConfig)
        {
            CreateNode(to, from, node, defaultConfig, null, null);
        }

        public void CreateNode(Jid to, string node, bool defaultConfig, IqCB cb)
        {
            CreateNode(to, null, node, defaultConfig, cb, null);
        }

        public void CreateNode(Jid to, string node, bool defaultConfig, IqCB cb, object cbArgs)
        {
            CreateNode(to, null, node, defaultConfig, cb, cbArgs);
        }

        public void CreateNode(Jid to, Jid from, string node, bool defaultConfig, IqCB cb)
        {
            CreateNode(to, from, node, defaultConfig, cb, null);
        }

        public void CreateNode(Jid to, Jid from, string node, bool defaultConfig, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Create = new Create(node);

            if (defaultConfig)
                pubsubIq.PubSub.Configure = new Configure();

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << CreateCollection Node >>
        /*
            To create a new collection node, the requesting entity MUST specify a type of "collection" when asking the service to create the node. [20]

            Example 185. Entity requests a new collection node

            <iq type='set'
                from='bard@shakespeare.lit/globe'
                to='pubsub.shakespeare.lit'
                id='create3'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <create node='announcements' type='collection'/>
              </pubsub>
            </iq>
                
            Example 186. Service responds with success

            <iq type='result'
                from='pubsub.shakespeare.lit'
                to='bard@shakespeare.lit/globe'
                id='create3'/>               
         
        */
        public void CreateCollectionNode(Jid to, string node, bool defaultConfig)
        {
            CreateCollectionNode(to, null, node, defaultConfig, null, null);
        }

        public void CreateCollectionNode(Jid to, string node, bool defaultConfig, IqCB cb)
        {
            CreateCollectionNode(to, null, node, defaultConfig, cb, null);
        }

        public void CreateCollectionNode(Jid to, string node, bool defaultConfig, IqCB cb, object cbArgs)
        {
            CreateCollectionNode(to, null, node, defaultConfig, cb, cbArgs);
        }


        public void CreateCollectionNode(Jid to, Jid from, string node, bool defaultConfig)
        {
            CreateCollectionNode(to, from, node, defaultConfig, null, null);
        }

        public void CreateCollectionNode(Jid to, Jid from, string node, bool defaultConfig, IqCB cb)
        {
            CreateCollectionNode(to, from, node, defaultConfig, cb, null);
        }

        public void CreateCollectionNode(Jid to, Jid from, string node, bool defaultConfig, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Create = new Create(node, Type.collection);

            if (defaultConfig)
                pubsubIq.PubSub.Configure = new Configure();

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Delete Node >>
        /*
            Example 133. Owner deletes a node

            <iq type='set'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='delete1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <delete node='blogs/princely_musings'/>
              </pubsub>
            </iq>                
        */

        public void DeleteNode(Jid to, string node)
        {
            DeleteNode(to, null, node, null, null);
        }

        public void DeleteNode(Jid to, string node, IqCB cb)
        {
            DeleteNode(to, null, node, cb, null);
        }
        
        public void DeleteNode(Jid to, string node, IqCB cb, object cbArgs)
        {
            DeleteNode(to, null, node, cb, cbArgs);
        }

        public void DeleteNode(Jid to, Jid from, string node)
        {
            DeleteNode(to, from, node, null, null);
        }

        public void DeleteNode(Jid to, Jid from, string node, IqCB cb)
        {
            DeleteNode(to, from, node, cb, null);
        }

        public void DeleteNode(Jid to, Jid from, string node, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Delete = new owner.Delete(node);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Purge Node >>
        /*
            Example 139. Owner purges all items from a node

            <iq type='set'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='purge1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <purge node='blogs/princely_musings'/>
              </pubsub>
            </iq>                
        */

        public void PurgeNode(Jid to, string node)
        {
            PurgeNode(to, null, node, null, null);
        }

        public void PurgeNode(Jid to, string node, IqCB cb)
        {
            PurgeNode(to, null, node, cb, null);
        }

        public void PurgeNode(Jid to, string node, IqCB cb, object cbArgs)
        {
            PurgeNode(to, null, node, cb, cbArgs);
        }

        public void PurgeNode(Jid to, Jid from, string node)
        {
            PurgeNode(to, from, node, null, null);
        }

        public void PurgeNode(Jid to, Jid from, string node, IqCB cb)
        {
            PurgeNode(to, from, node, cb, null);
        }

        public void PurgeNode(Jid to, Jid from, string node, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Purge = new owner.Purge(node);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Publish to a Node >>
        /*
            Example 9. Entity publishes an item with an ItemID

            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="publish1">
              <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <publish node="generic/pgm-mp3-player">
                  <item id="current">
                    <tune xmlns="http://jabber.org/protocol/tune">
                      <artist>Ralph Vaughan Williams</artist>
                      <title>Concerto in F for Bass Tuba</title>
                      <source>Golden Brass: The Collector's Edition</source>
                    </tune>
                  </item>
                </publish>
              </pubsub>
            </iq>
        */

        /// <summary>
        /// Publish a payload to a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="node"></param>
        /// <param name="payload"></param>
        public void PublishItem(Jid to, string node, Item payload)
        {
            PublishItem(to, null, node, payload, null, null);
        }

        /// <summary>
        /// Publish a payload to a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="node"></param>
        /// <param name="payload"></param>
        /// <param name="cb"></param>
        public void PublishItem(Jid to, string node, Item payload, IqCB cb)
        {
            PublishItem(to, null, node, payload, cb, null);
        }

        /// <summary>
        /// Publish a payload to a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="node"></param>
        /// <param name="payload"></param>
        /// <param name="cb"></param>
        /// <param name="cbArgs"></param>
        public void PublishItem(Jid to, string node, Item payload, IqCB cb, object cbArgs)
        {
            PublishItem(to, null, node, payload, cb, cbArgs);
        }

        /// <summary>
        /// Publish a payload to a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="node"></param>
        /// <param name="payload"></param>
        public void PublishItem(Jid to, Jid from, string node, Item payload)
        {
            PublishItem(to, from, node, payload, null, null);
        }

        /// <summary>
        /// Publish a payload to a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="node"></param>
        /// <param name="payload"></param>
        /// <param name="cb"></param>
        public void PublishItem(Jid to, Jid from, string node, Item payload, IqCB cb)
        {
            PublishItem(to, from, node, payload, cb, null);
        }

        /// <summary>
        /// Publish a payload to a Node
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="node"></param>
        /// <param name="payload"></param>
        /// <param name="cb"></param>
        /// <param name="cbArgs"></param>
        public void PublishItem(Jid to, Jid from, string node, Item payload, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            Publish publish = new Publish(node);
            publish.AddItem(payload);
            
            pubsubIq.PubSub.Publish = publish;

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }

        #endregion

        #region << Retract >>
        /*
            <iq type="set"
                from="pgm@jabber.org"
                to="pubsub.jabber.org"
                id="deleteitem1">
              <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <retract node="generic/pgm-mp3-player">
                  <item id="current"/>
                </retract>
              </pubsub>
            </iq>
        */

        public void RetractItem(Jid to, string node, string id)
        {
            RetractItem(to, null, node, id, null, null);
        }

        public void RetractItem(Jid to, string node, string id, IqCB cb)
        {
            RetractItem(to, null, node, id, cb, null);
        }

        public void RetractItem(Jid to, string node, string id, IqCB cb, object cbArgs)
        {
            RetractItem(to, null, node, id, cb, cbArgs);
        }


        public void RetractItem(Jid to, Jid from, string node, string id)
        {
            RetractItem(to, from, node, id, null, null);
        }

        public void RetractItem(Jid to, Jid from, string node, string id, IqCB cb)
        {
            RetractItem(to, from, node, id, cb, null);
        }

        public void RetractItem(Jid to, Jid from, string node, string id, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;


            pubsubIq.PubSub.Retract = new Retract(node, id);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Subscribe >>
        /*
            <iq type="set"
                from="sub1@foo.com/home"
                to="pubsub.jabber.org"
                id="sub1">
              <pubsub xmlns="http://jabber.org/protocol/pubsub">
                <subscribe
                    node="generic/pgm-mp3-player"
                    jid="sub1@foo.com"/>
              </pubsub>
            </iq>
        */
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="to">Jid of the Publish Subscribe Service</param>
        /// <param name="subscribe">Jid which should be subscribed</param>
        /// <param name="node">node to which we want to subscribe</param>
        public void Subscribe(Jid to, Jid subscribe, string node)
        {
            Subscribe(to, null, subscribe, node, null, null);
        }

        public void Subscribe(Jid to, Jid subscribe, string node, IqCB cb)
        {
            Subscribe(to, null, subscribe, node, cb, null);
        }

        public void Subscribe(Jid to, Jid subscribe, string node, IqCB cb, object cbArgs)
        {
            Subscribe(to, null, subscribe, node, cb, cbArgs);
        }

        public void Subscribe(Jid to, Jid from, Jid subscribe, string node)
        {
            Subscribe(to, from, subscribe, node, null, null);
        }

        public void Subscribe(Jid to, Jid from, Jid subscribe, string node, IqCB cb)
        {
            Subscribe(to, from, subscribe, node, cb, null);
        }

        public void Subscribe(Jid to, Jid from, Jid subscribe, string node, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;
            
            pubsubIq.PubSub.Subscribe = new Subscribe(node, subscribe);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }

        #endregion

        #region << Unsubscribe >>
        /*
            Example 38. Entity unsubscribes from a node

            <iq type='set'
                from='francisco@denmark.lit/barracks'
                to='pubsub.shakespeare.lit'
                id='unsub1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                 <unsubscribe
                     node='blogs/princely_musings'
                     jid='francisco@denmark.lit'/>
              </pubsub>
            </iq>
    
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="to">Jid of the Publish Subscribe Service</param>
        /// <param name="subscribe">Jid which should be subscribed</param>
        /// <param name="node">node to which we want to subscribe</param>
        public void Unsubscribe(Jid to, Jid unsubscribe, string node)
        {
            Unsubscribe(to, null, unsubscribe, node, null, null);
        }

        public void Unsubscribe(Jid to, Jid unsubscribe, string node, string subid)
        {
            Unsubscribe(to, null, unsubscribe, node, subid, null, null);
        }

        public void Unsubscribe(Jid to, Jid unsubscribe, string node, IqCB cb)
        {
            Unsubscribe(to, null, unsubscribe, node, cb, null);
        }

        public void Unsubscribe(Jid to, Jid unsubscribe, string node, string subid, IqCB cb)
        {
            Unsubscribe(to, null, unsubscribe, node, subid, cb, null);
        }

        public void Unsubscribe(Jid to, Jid unsubscribe, string node, IqCB cb, object cbArgs)
        {
            Unsubscribe(to, null, unsubscribe, node, cb, cbArgs);
        }

        public void Unsubscribe(Jid to, Jid unsubscribe, string node, string subid, IqCB cb, object cbArgs)
        {
            Unsubscribe(to, null, unsubscribe, node, subid, cb, cbArgs);
        }

        public void Unsubscribe(Jid to, Jid from, Jid unsubscribe, string node)
        {
            Unsubscribe(to, from, unsubscribe, node, null, null);
        }

        public void Unsubscribe(Jid to, Jid from, Jid unsubscribe, string node, string subid)
        {
            Unsubscribe(to, from, unsubscribe, node, subid, null, null);
        }

        public void Unsubscribe(Jid to, Jid from, Jid unsubscribe, string node, IqCB cb)
        {
            Unsubscribe(to, from, unsubscribe, node, cb, null);
        }

        public void Unsubscribe(Jid to, Jid from, Jid unsubscribe, string node, string subid, IqCB cb)
        {
            Unsubscribe(to, from, unsubscribe, node, subid, cb, null);
        }

        public void Unsubscribe(Jid to, Jid from, Jid unsubscribe, string node, IqCB cb, object cbArgs)
        {
            Unsubscribe(to, from, unsubscribe, node, null, cb, cbArgs);
        }

        public void Unsubscribe(Jid to, Jid from, Jid unsubscribe, string node, string subid, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            Unsubscribe unsub = new Unsubscribe(node, unsubscribe);
            if (subid != null)
                unsub.SubId = subid;

            pubsubIq.PubSub.Unsubscribe = unsub;

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }

        #endregion

        #region << Request Subscriptions >>>
        /*
            <iq type='get'
                from='francisco@denmark.lit/barracks'
                to='pubsub.shakespeare.lit'
                id='subscriptions1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <subscriptions/>
              </pubsub>
            </iq>
        */
        public void RequestSubscriptions(Jid to)
        {
            RequestSubscriptions(to, null, null, null);
        }

        public void RequestSubscriptions(Jid to, IqCB cb)
        {
            RequestSubscriptions(to, null, cb, null);
        }

        public void RequestSubscriptions(Jid to, IqCB cb, object cbArgs)
        {
            RequestSubscriptions(to, null, cb, cbArgs);
        }

        public void RequestSubscriptions(Jid to, Jid from)
        {
            RequestSubscriptions(to, from, null, null);
        }

        public void RequestSubscriptions(Jid to, Jid from, IqCB cb)
        {
            RequestSubscriptions(to, from, cb, null);
        }

        public void RequestSubscriptions(Jid to, Jid from, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.get, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Subscriptions = new Subscriptions();

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Owner Request Affiliations >>
        /*
            <iq type='get'
                from='francisco@denmark.lit/barracks'
                to='pubsub.shakespeare.lit'
                id='affil1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <affiliations/>
              </pubsub>
            </iq>
        */
        public void RequestAffiliations(Jid to)
        {
            RequestAffiliations(to, null, null, null);
        }

        public void RequestAffiliations(Jid to, IqCB cb)
        {
            RequestAffiliations(to, null, cb, null);
        }

        public void RequestAffiliations(Jid to, IqCB cb, object cbArgs)
        {
            RequestAffiliations(to, null, cb, cbArgs);
        }

        public void RequestAffiliations(Jid to, Jid from)
        {
            RequestAffiliations(to, from, null, null);
        }

        public void RequestAffiliations(Jid to, Jid from, IqCB cb)
        {
            RequestAffiliations(to, from, cb, null);
        }

        public void RequestAffiliations(Jid to, Jid from, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.get, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Affiliations = new Affiliations();

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Request Subscription Options >>
        /*
            <iq type='get'
                from='francisco@denmark.lit/barracks'
                to='pubsub.shakespeare.lit'
                id='options1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub'>
                <options node='blogs/princely_musings' jid='francisco@denmark.lit'/>
              </pubsub>
            </iq>
        */

        public void RequestSubscriptionOptions(Jid to, Jid subscribe, string node)
        {
            RequestSubscriptionOptions(to, null, subscribe, node, null, null);
        }

        public void RequestSubscriptionOptions(Jid to, Jid subscribe, string node, IqCB cb)
        {
            RequestSubscriptionOptions(to, null, subscribe, node, cb, null);
        }

        public void RequestSubscriptionOptions(Jid to, Jid subscribe, string node, IqCB cb, object cbArgs)
        {
            RequestSubscriptionOptions(to, null, subscribe, node, cb, cbArgs);
        }

        public void RequestSubscriptionOptions(Jid to, Jid from, Jid subscribe, string node)
        {
            RequestSubscriptionOptions(to, from, subscribe, node, null, null);
        }

        public void RequestSubscriptionOptions(Jid to, Jid from, Jid subscribe, string node, IqCB cb)
        {
            RequestSubscriptionOptions(to, from, subscribe, node, cb, null);
        }

        public void RequestSubscriptionOptions(Jid to, Jid from, Jid subscribe, string node, IqCB cb, object cbArgs)
        {
            PubSubIq pubsubIq = new PubSubIq(IqType.get, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Options = new Options(subscribe, node);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Request All Subscribers >>
        /*
            <iq type='get'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='subman1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <subscribers node='blogs/princely_musings'/>
              </pubsub>
            </iq>
        */

        public void OwnerRequestSubscribers(Jid to, string node)
        {
            OwnerRequestSubscribers(to, null, node, null, null);
        }

        public void OwnerRequestSubscribers(Jid to, string node, IqCB cb)
        {
            OwnerRequestSubscribers(to, null, node, cb, null);
        }

        public void OwnerRequestSubscribers(Jid to, string node, IqCB cb, object cbArgs)
        {
            OwnerRequestSubscribers(to, null, node, cb, cbArgs);
        }

        public void OwnerRequestSubscribers(Jid to, Jid from, string node)
        {
            OwnerRequestSubscribers(to, from, node, null, null);
        }

        public void OwnerRequestSubscribers(Jid to, Jid from, string node, IqCB cb)
        {
            OwnerRequestSubscribers(to, from, node, cb, null);
        }

        public void OwnerRequestSubscribers(Jid to, Jid from, string node, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.get, to);

            if (from != null)
                pubsubIq.From = from;

            pubsubIq.PubSub.Subscribers = new agsXMPP.protocol.extensions.pubsub.owner.Subscribers(node);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Modifying single Subscription State >>
        /*
            Upon receiving the subscribers list, the node owner MAY modify subscription states. 
            The owner MUST send only modified subscription states (i.e., a "delta"), not the complete list.
            (Note: If the 'subscription' attribute is not specified in a modification request, then the value
            MUST NOT be changed.)

            Example 163. Owner modifies subscriptions

            <iq type='set'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='subman3'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <subscribers node='blogs/princely_musings'>
                  <subscriber jid='polonius@denmark.lit' subscription='none'/>                  
                </subscribers>
              </pubsub>
            </iq>
    
        */

        public void OwnerModifySubscriptionState(Jid to, string node, Jid subscriber, SubscriptionState state)
        {
            OwnerModifySubscriptionState(to, null, node, subscriber, state, null, null);
        }

        public void OwnerModifySubscriptionState(Jid to, string node, Jid subscriber, SubscriptionState state, IqCB cb)
        {
            OwnerModifySubscriptionState(to, null, node, subscriber, state, cb, null);
        }

        public void OwnerModifySubscriptionState(Jid to, string node, Jid subscriber, SubscriptionState state, IqCB cb, object cbArgs)
        {
            OwnerModifySubscriptionState(to, null, node, subscriber, state, cb, cbArgs);
        }


        public void OwnerModifySubscriptionState(Jid to, Jid from, string node, Jid subscriber, SubscriptionState state)
        {
            OwnerModifySubscriptionState(to, from, node, subscriber, state, null, null);
        }

        public void OwnerModifySubscriptionState(Jid to, Jid from, string node, Jid subscriber, SubscriptionState state, IqCB cb)
        {
            OwnerModifySubscriptionState(to, from, node, subscriber, state, cb, null);
        }

        public void OwnerModifySubscriptionState(Jid to, Jid from, string node, Jid subscriber, SubscriptionState state, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            owner.Subscribers subs = new owner.Subscribers(node);
            subs.AddSubscriber(new owner.Subscriber(subscriber, state));
            
            pubsubIq.PubSub.Subscribers = subs;

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Modifying multiple Subscription States >>
        /*
            <iq type='set'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='subman3'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <subscribers node='blogs/princely_musings'>
                    <subscriber jid='polonius@denmark.lit' subscription='none'/>
                    <subscriber jid='bard@shakespeare.lit' subscription='subscribed'/>                 
                </subscribers>
              </pubsub>
            </iq>
        */

        public void OwnerModifySubscriptionStates(Jid to, string node, owner.Subscriber[] subscribers)
        {
            OwnerModifySubscriptionStates(to, null, node, subscribers, null, null);
        }

        public void OwnerModifySubscriptionStates(Jid to, string node, owner.Subscriber[] subscribers, IqCB cb)
        {
            OwnerModifySubscriptionStates(to, null, node, subscribers, cb, null);
        }

        public void OwnerModifySubscriptionStates(Jid to, string node, owner.Subscriber[] subscribers, IqCB cb, object cbArgs)
        {
            OwnerModifySubscriptionStates(to, null, node, subscribers, cb, cbArgs);
        }


        public void OwnerModifySubscriptionStates(Jid to, Jid from, string node, owner.Subscriber[] subscribers)
        {
            OwnerModifySubscriptionStates(to, from, node, subscribers, null, null);
        }

        public void OwnerModifySubscriptionStates(Jid to, Jid from, string node, owner.Subscriber[] subscribers, IqCB cb)
        {
            OwnerModifySubscriptionStates(to, from, node, subscribers, cb, null);
        }

        public void OwnerModifySubscriptionStates(Jid to, Jid from, string node, owner.Subscriber[] subscribers, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            owner.Subscribers subs = new owner.Subscribers(node);
            subs.AddSubscribers(subscribers);

            pubsubIq.PubSub.Subscribers = subs;

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Owner Request Affiliations >>
        /*
            Example 168. Owner requests all affiliated entities

            <iq type='get'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='ent1'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <affiliates node='blogs/princely_musings'/>
              </pubsub>
            </iq>                
        */

        public void OwnerRequestAffiliations(Jid to, string node)
        {
            OwnerRequestAffiliations(to, null, node, null, null);
        }

        public void OwnerRequestAffiliations(Jid to, string node, IqCB cb)
        {
            OwnerRequestAffiliations(to, null, node, cb, null);
        }

        public void OwnerRequestAffiliations(Jid to, string node, IqCB cb, object cbArgs)
        {
            OwnerRequestAffiliations(to, null, node, cb, cbArgs);
        }


        public void OwnerRequestAffiliations(Jid to, Jid from, string node)
        {
            OwnerRequestAffiliations(to, from, node, null, null);
        }

        public void OwnerRequestAffiliations(Jid to, Jid from, string node, IqCB cb)
        {
            OwnerRequestAffiliations(to, from, node, cb, null);
        }

        public void OwnerRequestAffiliations(Jid to, Jid from, string node, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.get, to);

            if (from != null)
                pubsubIq.From = from;
            
            pubsubIq.PubSub.Affiliates = new owner.Affiliates(node);

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Owner Set/Modify Affiliation >>
        /*
            Owner modifies a single affiliation

            <iq type='set'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='ent2'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <affiliates node='blogs/princely_musings'/>
                  <affiliate jid='hamlet@denmark.lit' affiliation='owner'/>                 
                </affiliates>
              </pubsub>
            </iq>
    
        */
        
        public void OwnerModifyAffiliation(Jid to, string node, Jid affiliate, AffiliationType affiliation)
        {
            OwnerModifyAffiliation(to, null, node, affiliate, affiliation, null, null);
        }
        
        public void OwnerModifyAffiliation(Jid to, string node, Jid affiliate, AffiliationType affiliation, IqCB cb)
        {
            OwnerModifyAffiliation(to, null, node, affiliate, affiliation, cb, null);
        }

        public void OwnerModifyAffiliation(Jid to, string node, Jid affiliate, AffiliationType affiliation, IqCB cb, object cbArgs)
        {
            OwnerModifyAffiliation(to, null, node, affiliate, affiliation, cb, cbArgs);
        }


        public void OwnerModifyAffiliation(Jid to, Jid from, string node, Jid affiliate, AffiliationType affiliation)
        {
            OwnerModifyAffiliation(to, from, node, affiliate, affiliation, null, null);
        }
        
        public void OwnerModifyAffiliation(Jid to, Jid from, string node, Jid affiliate, AffiliationType affiliation, IqCB cb)
        {
            OwnerModifyAffiliation(to, from, node, affiliate, affiliation, cb, null);
        }
        
        public void OwnerModifyAffiliation(Jid to, Jid from, string node, Jid affiliate, AffiliationType affiliation, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            owner.Affiliates aff = new owner.Affiliates(node);
            aff.AddAffiliate(new owner.Affiliate(affiliate, affiliation));

            pubsubIq.PubSub.Affiliates = aff;

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion

        #region << Owner Modify Affiliations >>
        /*
            Owner modifies a single affiliation

            <iq type='set'
                from='hamlet@denmark.lit/elsinore'
                to='pubsub.shakespeare.lit'
                id='ent2'>
              <pubsub xmlns='http://jabber.org/protocol/pubsub#owner'>
                <affiliates node='blogs/princely_musings'/>
                  <affiliate jid='hamlet@denmark.lit' affiliation='owner'/>
                  <affiliate jid='polonius@denmark.lit' affiliation='none'/>
                  <affiliate jid='bard@shakespeare.lit' affiliation='publisher'/>
                </affiliates>
              </pubsub>
            </iq>
        */

        public void OwnerModifyAffiliations(Jid to, string node, owner.Affiliate[] affiliates)
        {
            OwnerModifyAffiliations(to, null, node, affiliates, null, null);
        }

        public void OwnerModifyAffiliations(Jid to, string node, owner.Affiliate[] affiliates, IqCB cb)
        {
            OwnerModifyAffiliations(to, null, node, affiliates, cb, null);
        }

        public void OwnerModifyAffiliations(Jid to, string node, owner.Affiliate[] affiliates, IqCB cb, object cbArgs)
        {
            OwnerModifyAffiliations(to, null, node, affiliates, cb, cbArgs);
        }


        public void OwnerModifyAffiliations(Jid to, Jid from, string node, owner.Affiliate[] affiliates)
        {
            OwnerModifyAffiliations(to, from, node, affiliates, null, null);
        }

        public void OwnerModifyAffiliations(Jid to, Jid from, string node, owner.Affiliate[] affiliates, IqCB cb)
        {
            OwnerModifyAffiliations(to, from, node, affiliates, cb, null);
        }

        public void OwnerModifyAffiliations(Jid to, Jid from, string node, owner.Affiliate[] affiliates, IqCB cb, object cbArgs)
        {
            owner.PubSubIq pubsubIq = new owner.PubSubIq(IqType.set, to);

            if (from != null)
                pubsubIq.From = from;

            owner.Affiliates affs = new owner.Affiliates(node);
            affs.AddAffiliates(affiliates);

            pubsubIq.PubSub.Affiliates = affs;

            if (cb == null)
                m_connection.Send(pubsubIq);
            else
                m_connection.IqGrabber.SendIq(pubsubIq, cb, cbArgs);
        }
        #endregion
    }
    
}