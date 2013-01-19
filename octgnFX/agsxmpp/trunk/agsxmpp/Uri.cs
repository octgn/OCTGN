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

namespace agsXMPP
{
	
	public class Uri
	{
		public const string STREAM			= "http://etherx.jabber.org/streams";
		public const string CLIENT			= "jabber:client";		
		public const string SERVER			= "jabber:server"; 
		
		public const string IQ_AGENTS		= "jabber:iq:agents";
		public const string IQ_ROSTER		= "jabber:iq:roster";
		public const string IQ_AUTH			= "jabber:iq:auth";
		public const string IQ_REGISTER		= "jabber:iq:register";
		public const string IQ_OOB			= "jabber:iq:oob";
		public const string IQ_LAST			= "jabber:iq:last";
		public const string IQ_TIME			= "jabber:iq:time";
		public const string IQ_VERSION		= "jabber:iq:version";
		public const string IQ_BROWSE		= "jabber:iq:browse";
		public const string IQ_SEARCH		= "jabber:iq:search";
		public const string IQ_AVATAR		= "jabber:iq:avatar";
		public const string IQ_PRIVATE		= "jabber:iq:private";
        public const string IQ_PRIVACY      = "jabber:iq:privacy";


        
        /// <summary>
        /// JEP-0009: Jabber-RPC
        /// </summary>
        public const string IQ_RPC          = "jabber:iq:rpc";
        
		
		public const string X_DELAY			= "jabber:x:delay";
		public const string X_EVENT			= "jabber:x:event";
		public const string X_AVATAR		= "jabber:x:avatar";
		
        
		public const string X_CONFERENCE	= "jabber:x:conference";
		
        /// <summary>
        /// jabber:x:data
		/// </summary>
        public const string X_DATA			= "jabber:x:data";
		
		/// <summary>
		/// JEP-0144 Roster Item Exchange
		/// </summary>
		public const string X_ROSTERX		= "http://jabber.org/protocol/rosterx";
		

		/// <summary>
        /// Multi User Chat (MUC) JEP-0045
        /// http://jabber.org/protocol/muc
		/// </summary>
		public const string MUC				= "http://jabber.org/protocol/muc";
		/// <summary>
		/// http://jabber.org/protocol/muc#user
		/// </summary>
		public const string MUC_USER		= "http://jabber.org/protocol/muc#user";
		/// <summary>
		/// "http://jabber.org/protocol/muc#admin
		/// </summary>
		public const string MUC_ADMIN		= "http://jabber.org/protocol/muc#admin";
		/// <summary>
		/// http://jabber.org/protocol/muc#owner
		/// </summary>
		public const string MUC_OWNER		= "http://jabber.org/protocol/muc#owner";

		// Service Disovery
		public const string DISCO_ITEMS		= "http://jabber.org/protocol/disco#items";		
		public const string DISCO_INFO		= "http://jabber.org/protocol/disco#info";

		public const string STORAGE_AVATAR	= "storage:client:avatar";

		public const string VCARD			= "vcard-temp";

		// New XMPP Stuff
        /// <summary>
        /// urn:ietf:params:xml:ns:xmpp-streams
        /// </summary>
        public const string STREAMS         = "urn:ietf:params:xml:ns:xmpp-streams";
        public const string STANZAS			= "urn:ietf:params:xml:ns:xmpp-stanzas";
		public const string TLS				= "urn:ietf:params:xml:ns:xmpp-tls";		
		public const string SASL			= "urn:ietf:params:xml:ns:xmpp-sasl";		
		public const string SESSION			= "urn:ietf:params:xml:ns:xmpp-session";		
		public const string BIND			= "urn:ietf:params:xml:ns:xmpp-bind";
        

        /// <summary>
        /// jabber:component:accept
        /// </summary>
		public const string ACCEPT			= "jabber:component:accept";

		// Features
		//<register xmlns='http://jabber.org/features/iq-register'/>
		public const string FEATURE_IQ_REGISTER	= "http://jabber.org/features/iq-register";
        /// <summary>
        /// Stream Compression http://jabber.org/features/compress
        /// </summary>
        public const string FEATURE_COMPRESS    = "http://jabber.org/features/compress";

		// Extensions (JEPs)
		public const string SHIM				= "http://jabber.org/protocol/shim";
		public const string PRIMARY				= "http://jabber.org/protocol/primary";
        /// <summary>
        /// JEP-0172 User nickname
        /// http://jabber.org/protocol/nick
        /// </summary>
        public const string NICK                = "http://jabber.org/protocol/nick";
                
        /// <summary>
        /// JEP-0085 Chat State Notifications
        /// http://jabber.org/protocol/chatstates
        /// </summary>
        public const string CHATSTATES          = "http://jabber.org/protocol/chatstates";

        /// <summary>
        /// JEP-0138: Stream Compression
        /// </summary>
        public const string COMPRESS            = "http://jabber.org/protocol/compress";
		
		/// <summary>
		/// JEP-0020: Feature Negotiation http://jabber.org/protocol/feature-neg
		/// </summary>
		public const string FEATURE_NEG			= "http://jabber.org/protocol/feature-neg";

		/// <summary>
		/// JEO-0095 http://jabber.org/protocol/si
		/// </summary>
		public const string SI					= "http://jabber.org/protocol/si";
		/// <summary>
		/// JEO-0096 http://jabber.org/protocol/si/profile/file-transfer
		/// </summary>
		public const string SI_FILE_TRANSFER	= "http://jabber.org/protocol/si/profile/file-transfer";

        /// <summary>
        /// JEP-0065 SOCKS5 bytestreams
        /// http://jabber.org/protocol/bytestreams
        /// </summary>
        public const string BYTESTREAMS         = "http://jabber.org/protocol/bytestreams";

		// JEP-0083
		public const string ROSTER_DELIMITER	= "roster:delimiter";

        // Jive Software Namespaces

        /// <summary>
        /// Jivesoftware asterisk-im extension (http://jivesoftware.com/xmlns/phone);
        /// </summary>
        public const string JIVESOFTWARE_PHONE  = "http://jivesoftware.com/xmlns/phone";

        /// <summary>
        /// JEP-0071: XHTML-IM (http://jivesoftware.com/xmlns/phone)
        /// </summary>
        public const string XHTML_IM            = "http://jabber.org/protocol/xhtml-im";
        public const string XHTML			    = "http://www.w3.org/1999/xhtml";

        
        /// <summary>
        /// XEP-0115: Entity Capabilities (http://jabber.org/protocol/caps)
        /// </summary>
        public const string CAPS                = "http://jabber.org/protocol/caps";

        /// <summary>
        /// Jingle http://jabber.org/protocol/jingle
        /// </summary>
        public const string JINGLE                  = "http://jabber.org/protocol/jingle";

        /// <summary>
        /// Jingle audio format description http://jabber.org/protocol/jingle/description/audio
        /// </summary>
        public const string JINGLE_AUDIO_DESCRIPTION = "http://jabber.org/protocol/jingle/description/audio";

        /// <summary>
        /// Jingle Info audio http://jabber.org/protocol/jingle/info/audio;
        /// </summary>
        public const string JINGLE_AUDIO_INFO        = "http://jabber.org/protocol/jingle/info/audio";


        public const string JINGLE_VIDEO_DESCRIPTION = "http://jabber.org/protocol/jingle/description/video";

        /// <summary>
        /// GeoLoc (http://jabber.org/protocol/geoloc)
        /// </summary>
        public const string GEOLOC              = "http://jabber.org/protocol/geoloc";

        /// <summary>
        /// <para>XMPP ping</para>
        /// <para>Namespace: urn:xmpp:ping</para>
        /// <para><seealso cref="http://www.xmpp.org/extensions/xep-0199.html">http://www.xmpp.org/extensions/xep-0199.html</seealso></para>
        /// </summary>
        public const string PING                = "urn:xmpp:ping";
                
        /// <summary>
        /// Ad-Hoc Commands (http://jabber.org/protocol/commands)
        /// </summary>
        public const string COMMANDS            = "http://jabber.org/protocol/commands";

        // Pubsub stuff
        public const string PUBSUB              = "http://jabber.org/protocol/pubsub";
        public const string PUBSUB_EVENT        = "http://jabber.org/protocol/pubsub#event";        
        public const string PUBSUB_OWNER        = "http://jabber.org/protocol/pubsub#owner";

        // Http-Binding XEP-0124
        public const string HTTP_BIND           = "http://jabber.org/protocol/httpbind";
        
        /// <summary>
        /// <para>XEP-0184: Message Receipts</para>
        /// <para>urn:xmpp:receipts</para>        
        /// </summary>
        public const string MSG_RECEIPT         = "urn:xmpp:receipts";

        /// <summary>
        /// <para>XEP-0048: Bookmark Storage</para>
        /// <para>storage:bookmarks</para>
        /// </summary>
        public const string STORAGE_BOOKMARKS   = "storage:bookmarks";

        /// <summary>
        /// <para>XEP-0047: In-Band Bytestreams (IBB)</para>
        /// <para>http://jabber.org/protocol/ibb</para>
        /// </summary>
        public const string IBB                 = "http://jabber.org/protocol/ibb";

        /// <summary>
        /// <para></para>
        /// <para>http://jabber.org/protocol/amp</para>
        /// </summary>
        public const string AMP                 = "http://jabber.org/protocol/amp";

        /// <summary>
        /// <para>XEP-0153: vCard-Based Avatars</para>
        /// <para>vcard-temp:x:update</para>
        /// </summary>
        public const string VCARD_UPDATE        = "vcard-temp:x:update";

        public const string URN_TIME            = "urn:xmpp:time";

        /// <summary>
        /// <para>XEP-0145 Annotations</para>
        /// <para>storage:rosternotes</para>
        /// </summary>
        public const string STORAGE_ROSTERNOTES = "storage:rosternotes";
	}	
}