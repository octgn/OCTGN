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
using System.Collections;

using agsXMPP.Xml.Dom;

namespace agsXMPP.Factory
{
	/// <summary>
	/// Factory class that implements the factory pattern for builing our Elements.
	/// </summary>
	public class ElementFactory
	{		
		/// <summary>
		/// This Hashtable stores Mapping of protocol (tag/namespace) to the agsXMPP objects
		/// </summary>
		private static Hashtable m_table = new Hashtable();

		static ElementFactory()
		{
			AddElementType("iq",				Uri.CLIENT,					typeof(agsXMPP.protocol.client.IQ));
			AddElementType("message",			Uri.CLIENT,					typeof(agsXMPP.protocol.client.Message));
			AddElementType("presence",			Uri.CLIENT,					typeof(agsXMPP.protocol.client.Presence));
			AddElementType("error",				Uri.CLIENT,					typeof(agsXMPP.protocol.client.Error));
						
			AddElementType("agent",				Uri.IQ_AGENTS,				typeof(agsXMPP.protocol.iq.agent.Agent));
			
			AddElementType("item",				Uri.IQ_ROSTER,				typeof(agsXMPP.protocol.iq.roster.RosterItem));
			AddElementType("group",				Uri.IQ_ROSTER,				typeof(agsXMPP.protocol.Base.Group));
			AddElementType("group",				Uri.X_ROSTERX,				typeof(agsXMPP.protocol.Base.Group));

			AddElementType("item",				Uri.IQ_SEARCH,				typeof(agsXMPP.protocol.iq.search.SearchItem));			
			
            // Stream stuff
			AddElementType("stream",			Uri.STREAM,					typeof(agsXMPP.protocol.Stream));			
            AddElementType("error",				Uri.STREAM,					typeof(agsXMPP.protocol.Error));
			
			AddElementType("query",				Uri.IQ_AUTH,				typeof(agsXMPP.protocol.iq.auth.Auth));
			AddElementType("query",				Uri.IQ_AGENTS,				typeof(agsXMPP.protocol.iq.agent.Agents));
			AddElementType("query",				Uri.IQ_ROSTER,				typeof(agsXMPP.protocol.iq.roster.Roster));
			AddElementType("query",				Uri.IQ_LAST,				typeof(agsXMPP.protocol.iq.last.Last));
            AddElementType("query",				Uri.IQ_VERSION,				typeof(agsXMPP.protocol.iq.version.Version));
			AddElementType("query",				Uri.IQ_TIME,				typeof(agsXMPP.protocol.iq.time.Time));
			AddElementType("query",				Uri.IQ_OOB,					typeof(agsXMPP.protocol.iq.oob.Oob));
			AddElementType("query",				Uri.IQ_SEARCH,				typeof(agsXMPP.protocol.iq.search.Search));
			AddElementType("query",				Uri.IQ_BROWSE,				typeof(agsXMPP.protocol.iq.browse.Browse));
			AddElementType("query",				Uri.IQ_AVATAR,				typeof(agsXMPP.protocol.iq.avatar.Avatar));
			AddElementType("query",				Uri.IQ_REGISTER,			typeof(agsXMPP.protocol.iq.register.Register));
			AddElementType("query",				Uri.IQ_PRIVATE,				typeof(agsXMPP.protocol.iq.@private.Private));
            
            // Privacy Lists
            AddElementType("query",             Uri.IQ_PRIVACY,             typeof(agsXMPP.protocol.iq.privacy.Privacy));
            AddElementType("item",              Uri.IQ_PRIVACY,             typeof(agsXMPP.protocol.iq.privacy.Item));
            AddElementType("list",              Uri.IQ_PRIVACY,             typeof(agsXMPP.protocol.iq.privacy.List));
            AddElementType("active",            Uri.IQ_PRIVACY,             typeof(agsXMPP.protocol.iq.privacy.Active));
            AddElementType("default",           Uri.IQ_PRIVACY,             typeof(agsXMPP.protocol.iq.privacy.Default));
                        
			// Browse
			AddElementType("service",			Uri.IQ_BROWSE,				typeof(agsXMPP.protocol.iq.browse.Service));
			AddElementType("item",				Uri.IQ_BROWSE,				typeof(agsXMPP.protocol.iq.browse.BrowseItem));

			// Service Discovery			
			AddElementType("query",				Uri.DISCO_ITEMS,			typeof(agsXMPP.protocol.iq.disco.DiscoItems));			
			AddElementType("query",				Uri.DISCO_INFO,				typeof(agsXMPP.protocol.iq.disco.DiscoInfo));
			AddElementType("feature",			Uri.DISCO_INFO,			    typeof(agsXMPP.protocol.iq.disco.DiscoFeature));
			AddElementType("identity",			Uri.DISCO_INFO,			    typeof(agsXMPP.protocol.iq.disco.DiscoIdentity));			
			AddElementType("item",				Uri.DISCO_ITEMS,			typeof(agsXMPP.protocol.iq.disco.DiscoItem));

			AddElementType("x",					Uri.X_DELAY,				typeof(agsXMPP.protocol.x.Delay));
			AddElementType("x",					Uri.X_AVATAR,				typeof(agsXMPP.protocol.x.Avatar));
			AddElementType("x",					Uri.X_CONFERENCE,			typeof(agsXMPP.protocol.x.Conference));
            AddElementType("x",                 Uri.X_EVENT,                typeof(agsXMPP.protocol.x.Event));
			
			//AddElementType("x",					Uri.STORAGE_AVATAR,	typeof(agsXMPP.protocol.storage.Avatar));
			AddElementType("query",				Uri.STORAGE_AVATAR,			typeof(agsXMPP.protocol.storage.Avatar));

			// XData Stuff
			AddElementType("x",					Uri.X_DATA,					typeof(agsXMPP.protocol.x.data.Data));
			AddElementType("field",				Uri.X_DATA,					typeof(agsXMPP.protocol.x.data.Field));
			AddElementType("option",			Uri.X_DATA,					typeof(agsXMPP.protocol.x.data.Option));
			AddElementType("value",				Uri.X_DATA,					typeof(agsXMPP.protocol.x.data.Value));
            AddElementType("reported",          Uri.X_DATA,                 typeof(agsXMPP.protocol.x.data.Reported));
            AddElementType("item",              Uri.X_DATA,                 typeof(agsXMPP.protocol.x.data.Item));
			
			AddElementType("features",			Uri.STREAM,					typeof(agsXMPP.protocol.stream.Features));

			AddElementType("register",			Uri.FEATURE_IQ_REGISTER,	typeof(agsXMPP.protocol.stream.feature.Register));
            AddElementType("compression",       Uri.FEATURE_COMPRESS,       typeof(agsXMPP.protocol.stream.feature.compression.Compression));
            AddElementType("method",            Uri.FEATURE_COMPRESS,       typeof(agsXMPP.protocol.stream.feature.compression.Method));

			AddElementType("bind",				Uri.BIND,					typeof(agsXMPP.protocol.iq.bind.Bind));
			AddElementType("session",			Uri.SESSION,				typeof(agsXMPP.protocol.iq.session.Session));
			
			// TLS stuff
			AddElementType("failure",			Uri.TLS,					typeof(agsXMPP.protocol.tls.Failure));
			AddElementType("proceed",			Uri.TLS,					typeof(agsXMPP.protocol.tls.Proceed));
			AddElementType("starttls",			Uri.TLS,					typeof(agsXMPP.protocol.tls.StartTls));

			// SASL stuff
			AddElementType("mechanisms",		Uri.SASL,					typeof(agsXMPP.protocol.sasl.Mechanisms));
			AddElementType("mechanism",			Uri.SASL,					typeof(agsXMPP.protocol.sasl.Mechanism));			
			AddElementType("auth",				Uri.SASL,					typeof(agsXMPP.protocol.sasl.Auth));
			AddElementType("response",			Uri.SASL,					typeof(agsXMPP.protocol.sasl.Response));
			AddElementType("challenge",			Uri.SASL,					typeof(agsXMPP.protocol.sasl.Challenge));
            
            // TODO, this is a dirty hacks for the buggy BOSH Proxy
            // BEGIN
            AddElementType("challenge",         Uri.CLIENT,                 typeof(agsXMPP.protocol.sasl.Challenge));
            AddElementType("success",           Uri.CLIENT,                 typeof(agsXMPP.protocol.sasl.Success));
            // END

			AddElementType("failure",			Uri.SASL,					typeof(agsXMPP.protocol.sasl.Failure));
			AddElementType("abort",				Uri.SASL,					typeof(agsXMPP.protocol.sasl.Abort));
			AddElementType("success",			Uri.SASL,					typeof(agsXMPP.protocol.sasl.Success));
            
			// Vcard stuff
			AddElementType("vCard",				Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Vcard));
            AddElementType("TEL",				Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Telephone));
			AddElementType("ORG",				Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Organization));
			AddElementType("N",					Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Name));
			AddElementType("EMAIL",				Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Email));			
			AddElementType("ADR",				Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Address));
#if !CF
			AddElementType("PHOTO",				Uri.VCARD,					typeof(agsXMPP.protocol.iq.vcard.Photo));
#endif
            // Server stuff
            //AddElementType("stream",            Uri.SERVER,                 typeof(agsXMPP.protocol.server.Stream));
            //AddElementType("message",           Uri.SERVER,                 typeof(agsXMPP.protocol.server.Message));

			// Component stuff
			AddElementType("handshake",			Uri.ACCEPT,					typeof(agsXMPP.protocol.component.Handshake));
			AddElementType("log",				Uri.ACCEPT,					typeof(agsXMPP.protocol.component.Log));
			AddElementType("route",				Uri.ACCEPT,					typeof(agsXMPP.protocol.component.Route));
			AddElementType("iq",				Uri.ACCEPT,					typeof(agsXMPP.protocol.component.IQ));
            AddElementType("message",           Uri.ACCEPT,                 typeof(agsXMPP.protocol.component.Message));
            AddElementType("presence",          Uri.ACCEPT,                 typeof(agsXMPP.protocol.component.Presence));
            AddElementType("error",             Uri.ACCEPT,                 typeof(agsXMPP.protocol.component.Error));

			//Extensions (JEPS)
			AddElementType("header",			Uri.SHIM,					typeof(agsXMPP.protocol.extensions.shim.Header));
			AddElementType("headers",			Uri.SHIM,					typeof(agsXMPP.protocol.extensions.shim.Headers));
			AddElementType("roster",			Uri.ROSTER_DELIMITER,		typeof(agsXMPP.protocol.iq.roster.Delimiter));
			AddElementType("p",					Uri.PRIMARY,				typeof(agsXMPP.protocol.extensions.primary.Primary));
            AddElementType("nick",              Uri.NICK,                   typeof(agsXMPP.protocol.extensions.nickname.Nickname));

			AddElementType("item",				Uri.X_ROSTERX,				typeof(agsXMPP.protocol.x.rosterx.RosterItem));
			AddElementType("x",					Uri.X_ROSTERX,				typeof(agsXMPP.protocol.x.rosterx.RosterX));

            // Filetransfer stuff
			AddElementType("file",				Uri.SI_FILE_TRANSFER,		typeof(agsXMPP.protocol.extensions.filetransfer.File));
			AddElementType("range",				Uri.SI_FILE_TRANSFER,		typeof(agsXMPP.protocol.extensions.filetransfer.Range));

            // FeatureNeg
            AddElementType("feature",           Uri.FEATURE_NEG,            typeof(agsXMPP.protocol.extensions.featureneg.FeatureNeg));

            // Bytestreams
            AddElementType("query",             Uri.BYTESTREAMS,            typeof(agsXMPP.protocol.extensions.bytestreams.ByteStream));
            AddElementType("streamhost",        Uri.BYTESTREAMS,            typeof(agsXMPP.protocol.extensions.bytestreams.StreamHost));
            AddElementType("streamhost-used",   Uri.BYTESTREAMS,            typeof(agsXMPP.protocol.extensions.bytestreams.StreamHostUsed));
            AddElementType("activate",          Uri.BYTESTREAMS,            typeof(agsXMPP.protocol.extensions.bytestreams.Activate));
            AddElementType("udpsuccess",        Uri.BYTESTREAMS,            typeof(agsXMPP.protocol.extensions.bytestreams.UdpSuccess));
            

			AddElementType("si",				Uri.SI,						typeof(agsXMPP.protocol.extensions.si.SI));
            
            AddElementType("html",              Uri.XHTML_IM,               typeof(agsXMPP.protocol.extensions.html.Html));
            AddElementType("body",              Uri.XHTML,                  typeof(agsXMPP.protocol.extensions.html.Body));
            
            AddElementType("compressed",        Uri.COMPRESS,               typeof(agsXMPP.protocol.extensions.compression.Compressed));
            AddElementType("compress",          Uri.COMPRESS,               typeof(agsXMPP.protocol.extensions.compression.Compress));
            AddElementType("failure",           Uri.COMPRESS,               typeof(agsXMPP.protocol.extensions.compression.Failure));
                    
            // MUC (JEP-0045 Multi User Chat)
            AddElementType("x",                 Uri.MUC,                    typeof(agsXMPP.protocol.x.muc.Muc));
            AddElementType("x",                 Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.User));
            AddElementType("item",              Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.Item));
            AddElementType("status",            Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.Status));
            AddElementType("invite",            Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.Invite));
            AddElementType("decline",           Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.Decline));
            AddElementType("actor",             Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.Actor));
            AddElementType("history",           Uri.MUC,                    typeof(agsXMPP.protocol.x.muc.History));
            AddElementType("query",             Uri.MUC_ADMIN,              typeof(agsXMPP.protocol.x.muc.iq.admin.Admin));
            AddElementType("item",              Uri.MUC_ADMIN,              typeof(agsXMPP.protocol.x.muc.iq.admin.Item));
            AddElementType("query",             Uri.MUC_OWNER,              typeof(agsXMPP.protocol.x.muc.iq.owner.Owner));
            AddElementType("destroy",           Uri.MUC_OWNER,              typeof(agsXMPP.protocol.x.muc.owner.Destroy));
            AddElementType("destroy",           Uri.MUC_USER,               typeof(agsXMPP.protocol.x.muc.user.Destroy));
            

            //Jabber RPC JEP 0009            
            AddElementType("query",             Uri.IQ_RPC,                 typeof(agsXMPP.protocol.iq.rpc.Rpc));
            AddElementType("methodCall",        Uri.IQ_RPC,                 typeof(agsXMPP.protocol.iq.rpc.MethodCall));
            AddElementType("methodResponse",    Uri.IQ_RPC,                 typeof(agsXMPP.protocol.iq.rpc.MethodResponse));

            // Chatstates Jep-0085
            AddElementType("active",            Uri.CHATSTATES,             typeof(agsXMPP.protocol.extensions.chatstates.Active));
            AddElementType("inactive",          Uri.CHATSTATES,             typeof(agsXMPP.protocol.extensions.chatstates.Inactive));
            AddElementType("composing",         Uri.CHATSTATES,             typeof(agsXMPP.protocol.extensions.chatstates.Composing));
            AddElementType("paused",            Uri.CHATSTATES,             typeof(agsXMPP.protocol.extensions.chatstates.Paused));
            AddElementType("gone",              Uri.CHATSTATES,             typeof(agsXMPP.protocol.extensions.chatstates.Gone));

            // Jivesoftware Extenstions
            AddElementType("phone-event",       Uri.JIVESOFTWARE_PHONE,     typeof(agsXMPP.protocol.extensions.jivesoftware.phone.PhoneEvent));
            AddElementType("phone-action",      Uri.JIVESOFTWARE_PHONE,     typeof(agsXMPP.protocol.extensions.jivesoftware.phone.PhoneAction));
            AddElementType("phone-status",      Uri.JIVESOFTWARE_PHONE,     typeof(agsXMPP.protocol.extensions.jivesoftware.phone.PhoneStatus));

            // Jingle stuff is in heavy development, we commit this once the most changes on the Jeps are done            
            //AddElementType("jingle",            Uri.JINGLE,                 typeof(agsXMPP.protocol.extensions.jingle.Jingle));
            //AddElementType("candidate",         Uri.JINGLE,                 typeof(agsXMPP.protocol.extensions.jingle.Candidate));

            AddElementType("c",                 Uri.CAPS,                   typeof(agsXMPP.protocol.extensions.caps.Capabilities));

            AddElementType("geoloc",            Uri.GEOLOC,                 typeof(agsXMPP.protocol.extensions.geoloc.GeoLoc));

            // Xmpp Ping
            AddElementType("ping",              Uri.PING,                   typeof(agsXMPP.protocol.extensions.ping.Ping));

            //Ad-Hock Commands
            AddElementType("command",           Uri.COMMANDS,               typeof(agsXMPP.protocol.extensions.commands.Command));
            AddElementType("actions",           Uri.COMMANDS,               typeof(agsXMPP.protocol.extensions.commands.Actions));
            AddElementType("note",              Uri.COMMANDS,               typeof(agsXMPP.protocol.extensions.commands.Note));

            // **********
            // * PubSub *
            // **********
            // Owner namespace
            AddElementType("affiliate",         Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Affiliate));
            AddElementType("affiliates",        Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Affiliates));
            AddElementType("configure",         Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Configure));
            AddElementType("delete",            Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Delete));
            AddElementType("pending",           Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Pending));
            AddElementType("pubsub",            Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.PubSub));
            AddElementType("purge",             Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Purge));
            AddElementType("subscriber",        Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Subscriber));
            AddElementType("subscribers",       Uri.PUBSUB_OWNER,           typeof(agsXMPP.protocol.extensions.pubsub.owner.Subscribers));

            // Event namespace
            AddElementType("delete",            Uri.PUBSUB_EVENT,           typeof(agsXMPP.protocol.extensions.pubsub.@event.Delete));
            AddElementType("event",             Uri.PUBSUB_EVENT,           typeof(agsXMPP.protocol.extensions.pubsub.@event.Event));
            AddElementType("item",              Uri.PUBSUB_EVENT,           typeof(agsXMPP.protocol.extensions.pubsub.@event.Item));
            AddElementType("items",             Uri.PUBSUB_EVENT,           typeof(agsXMPP.protocol.extensions.pubsub.@event.Items));
            AddElementType("purge",             Uri.PUBSUB_EVENT,           typeof(agsXMPP.protocol.extensions.pubsub.@event.Purge));

            // Main Pubsub namespace
            AddElementType("affiliation",       Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Affiliation));
            AddElementType("affiliations",      Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Affiliations));
            AddElementType("configure",         Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Configure));
            AddElementType("create",            Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Create));
            AddElementType("configure",         Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Configure));
            AddElementType("item",              Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Item));
            AddElementType("items",             Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Items));
            AddElementType("options",           Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Options));
            AddElementType("publish",           Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Publish));
            AddElementType("pubsub",            Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.PubSub));
            AddElementType("retract",           Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Retract));
            AddElementType("subscribe",         Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Subscribe));
            AddElementType("subscribe-options", Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.SubscribeOptions));
            AddElementType("subscription",      Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Subscription));
            AddElementType("subscriptions",     Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Subscriptions));
            AddElementType("unsubscribe",       Uri.PUBSUB,                 typeof(agsXMPP.protocol.extensions.pubsub.Unsubscribe));           

            // HTTP Binding XEP-0124
            AddElementType("body",              Uri.HTTP_BIND,              typeof(agsXMPP.protocol.extensions.bosh.Body));

            // Message receipts XEP-0184
            AddElementType("received",          Uri.MSG_RECEIPT,            typeof(agsXMPP.protocol.extensions.msgreceipts.Received));
            AddElementType("request",           Uri.MSG_RECEIPT,            typeof(agsXMPP.protocol.extensions.msgreceipts.Request));

            // Bookmark storage XEP-0048         
            AddElementType("storage",           Uri.STORAGE_BOOKMARKS,      typeof(agsXMPP.protocol.extensions.bookmarks.Storage));
            AddElementType("url",               Uri.STORAGE_BOOKMARKS,      typeof(agsXMPP.protocol.extensions.bookmarks.Url));
            AddElementType("conference",        Uri.STORAGE_BOOKMARKS,      typeof(agsXMPP.protocol.extensions.bookmarks.Conference));
            
            // XEP-0047: In-Band Bytestreams (IBB)
            AddElementType("open",              Uri.IBB,                    typeof(agsXMPP.protocol.extensions.ibb.Open));
            AddElementType("data",              Uri.IBB,                    typeof(agsXMPP.protocol.extensions.ibb.Data));
            AddElementType("close",             Uri.IBB,                    typeof(agsXMPP.protocol.extensions.ibb.Close));
                    
            // XEP-0153: vCard-Based Avatars
            AddElementType("x",                 Uri.VCARD_UPDATE,           typeof(agsXMPP.protocol.x.vcard_update.VcardUpdate));

            // AMP
            AddElementType("amp",               Uri.AMP,                    typeof(agsXMPP.protocol.extensions.amp.Amp));
            AddElementType("rule",              Uri.AMP,                    typeof(agsXMPP.protocol.extensions.amp.Rule));

            // Urn Time
            AddElementType("time",              Uri.URN_TIME,               typeof(agsXMPP.protocol.time.Time));

            // XEP-0145 Annotations
            AddElementType("storage",           Uri.STORAGE_ROSTERNOTES,    typeof(agsXMPP.protocol.extensions.bookmarks.RosterNotes));
            AddElementType("note",              Uri.STORAGE_ROSTERNOTES,    typeof(agsXMPP.protocol.extensions.bookmarks.RosterNote));
		}		
		
		/// <summary>
		/// Adds new Element Types to the Hashtable
		/// Use this function also to register your own created Elements.
        /// If a element is already registered it gets overwritten. This behaviour is also useful if you you want to overwrite
        /// classes and add your own derived classes to the factory.
		/// </summary>
		/// <param name="tag">FQN</param>
		/// <param name="ns"></param>
		/// <param name="t"></param>
		public static void AddElementType(string tag, string ns, System.Type t)
		{
            ElementType et = new ElementType(tag, ns);
            string key = et.ToString();
            // added thread safety on a user request
            lock (m_table)
            {
                if (m_table.ContainsKey(key))
                    m_table[key] = t;
                else
                    m_table.Add(et.ToString(), t);
            }
		}
        
		/// <summary>
		/// 
		/// </summary>
		/// <param name="prefix"></param>
		/// <param name="tag"></param>
		/// <param name="ns"></param>
		/// <returns></returns>
		public static Element GetElement(string prefix, string tag, string ns)
		{
			if (ns == null)
				ns = "";

			ElementType et = new ElementType(tag, ns);			
			System.Type t = (System.Type) m_table[et.ToString()];

			Element ret;			
			if (t != null)
				ret = (Element) System.Activator.CreateInstance(t);				
			else
			    ret = new Element(tag);				
			
			ret.Prefix = prefix;

			if (ns!="")
				ret.Namespace = ns;
			
			return ret;
		}		
	}  
}