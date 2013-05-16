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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;

using agsXMPP.protocol;
using agsXMPP.protocol.iq;
using agsXMPP.protocol.iq.auth;
using agsXMPP.protocol.iq.agent;
using agsXMPP.protocol.iq.disco;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.iq.register;
using agsXMPP.protocol.iq.version;
using agsXMPP.protocol.stream;
using agsXMPP.protocol.stream.feature.compression;
using agsXMPP.protocol.client;
using agsXMPP.protocol.tls;

using agsXMPP.protocol.extensions.caps;
using agsXMPP.protocol.extensions.compression;

using agsXMPP.Exceptions;

using agsXMPP.Sasl;
using agsXMPP.Net;
using agsXMPP.Net.Dns;


using agsXMPP.Idn;

namespace agsXMPP
{
	public delegate void ObjectHandler		(object sender);	
	public delegate void XmppElementHandler	(object sender, Element e);
	
	/// <summary>
	/// Summary description for XmppClient.
	/// </summary>
	public class XmppClientConnection : XmppConnection
	{       
        
        const string SRV_RECORD_PREFIX = "_xmpp-client._tcp.";

		// Delegates		
		public delegate void RosterHandler				(object sender, RosterItem item);
		public delegate void AgentHandler				(object sender, Agent agent);     
               
		private SaslHandler					m_SaslHandler		= null;
	
		private bool						m_CleanUpDone;
        private bool                        m_StreamStarted;
        
        private SRVRecord[]                 _SRVRecords;
        private SRVRecord                   _currentSRVRecord;
       
        
		#region << Properties and Member Variables >>
        private     string                  m_ClientLanguage    = "en";
        private     string                  m_ServerLanguage    = null;
		private		string					m_Username			= "";
		private		string					m_Password			= "";        
		private		string					m_Resource			= "agsXMPP";		
		private		string					m_Status			= "";
		private		int						m_Priority			= 5;
		private		ShowType				m_Show				= ShowType.NONE;
		private		bool					m_AutoRoster		= true;
		private		bool					m_AutoAgents		= true;
        private     bool                    m_AutoPresence      = true;
#if !(CF || CF_2)
        private     bool                    m_UseSso            = false;
        internal    string                  m_KerberosPrincipal;
#endif
	  
		private		bool					m_UseSSL			= false;
#if (CF || CF_2) && !BCCRYPTO
        private     bool                    m_UseStartTLS       = false;
#else
        private		bool					m_UseStartTLS		= true;
#endif
        private     bool                    m_UseCompression    = false;
		internal	bool					m_Binded			= false;
		private		bool					m_Authenticated		= false;
		
		private		IqGrabber				m_IqGrabber			= null;
		private		MessageGrabber			m_MessageGrabber	= null;
        private     PresenceGrabber         m_PresenceGrabber   = null;
		private		bool					m_RegisterAccount	= false;
		private		PresenceManager			m_PresenceManager;
		private		RosterManager			m_RosterManager;
               

        private     Capabilities            m_Capabilities          = new Capabilities();
        private     string                  m_ClientVersion         = "1.0";
        private     bool                    m_EnableCapabilities    = false;

        private     DiscoInfo               m_DiscoInfo             = new DiscoInfo();
                     

        /// <summary>
        /// The prefered Client Language Attribute
        /// </summary>
        /// <seealso cref="agsXMPP.protocol.Base.XmppPacket.Language"/>
        public string ClientLanguage
        {
            get { return m_ClientLanguage; }
            set { m_ClientLanguage = value; }
        }

        /// <summary>
        /// The language which the server decided to use.
        /// </summary>
        /// <seealso cref="agsXMPP.protocol.Base.XmppPacket.Language"/>
        public string ServerLanguage
        {
            get { return m_ServerLanguage; }            
        }

		/// <summary>
		/// the username that is used to authenticate to the xmpp server
		/// </summary>
		public string Username
		{
			get { return m_Username; }
			set
            {
                // first Encode the user/node
                m_Username = value;

                string tmpUser = Jid.EscapeNode(value);
#if !STRINGPREP
                if (value != null)
				    m_Username = tmpUser.ToLower();
                else
                    m_Username = null;
#else
                if (value != null)
                    m_Username = Stringprep.NodePrep(tmpUser);
                else
                    m_Username = null;
#endif
                
            }                
		}

		/// <summary>
		/// the password that is used to authenticate to the xmpp server
		/// </summary>
		public string Password
		{
			get { return m_Password; }
			set	{ m_Password = value; }
		}
                
		/// <summary>
		/// the resource for this connection each connection to the server with the same jid needs a unique resource.
        /// You can also set <code>Resource = null</code> and the server will assign a random Resource for you.
		/// </summary>
		public string Resource
		{
			get { return m_Resource;  }
			set { m_Resource = value; }
		}
		
		/// <summary>
		/// our XMPP id build from Username, Server and Resource Property (user@server/resourcee)
		/// </summary>
		public Jid MyJID
		{
			get	
			{ 
				return BuildMyJid();               
			}
		}

        /// <summary>
        /// The status message of this connection which is sent with the presence packets.
        /// </summary>
        /// <remarks>
        /// you have to call the method <b>SendMyPresence</b> to send your updated presence to the server.        
        /// </remarks>
		public string Status
		{
			get
			{
				return m_Status;
			}
			set
			{
				m_Status = value;
			}
		}

		/// <summary>
		/// The priority of this connection send with the presence packets.
        /// The OPTIONAL priority element contains non-human-readable XML character data that specifies the priority level 
        /// of the resource. The value MUST be an integer between -128 and +127. If no priority is provided, a server 
        /// SHOULD consider the priority to be zero.        
		/// </summary>
        /// <remarks>you have to call the method <b>SendMyPresence</b> to send your updated presence to the server.</remarks>
		public int Priority
		{
			get { return m_Priority; }
			set
            {
                if ((value < -127) || (value > 127))
                    throw new ArgumentException("The value MUST be an integer between -128 and +127");
                
                m_Priority = value;
			}
		}

        /// <summary>
        /// change the showtype. 
        /// </summary>
        /// <remarks>you have to call the method <b>SendMyPresence</b> to send your updated presence to the server.</remarks>
		public ShowType Show
		{
			get { return m_Show; }
			set { m_Show = value; }
		}

        /// <summary>
        /// If set to true then the Roster (contact list) is requested automatically after sucessful login. 
        /// Set this property to false if you don't want to receive your contact list, or request it manual. 
        /// To save bandwidth is makes sense to cache the contact list and don't receive it on each login.
        /// </summary>
        /// <remarks>default value is <b>true</b></remarks>
		public bool AutoRoster
		{
			get	{ return m_AutoRoster; }
			set	{ m_AutoRoster = value;	}
		}

        /// <summary>
        /// Sends the presence Automatically after successful login.
        /// This property works only in combination with AutoRoster (AutoRoster = true).
        /// </summary>
        public bool AutoPresence
        {
            get { return m_AutoPresence; }
            set { m_AutoPresence = value; }
        }
        
		/// <summary>
        /// If set to true then the Agents are requested automatically after sucessful login. 
        /// Set this property to false if you don't use agents at all, or if you request them manual.
		/// </summary>
        /// <remarks>default value is <b>true</b></remarks>
		public bool AutoAgents
		{
			get	{ return m_AutoAgents; }
			set	{ m_AutoAgents = value;	}
        }

#if !(CF || CF_2)
        /// <summary>
        /// Use Single sign on (GSSAPI/KERBEROS)
        /// </summary>
        public bool UseSso
        {
            get { return m_UseSso; }
            set
            {
                if (Util.Runtime.IsMono() && Util.Runtime.IsUnix())
                    throw new NotImplementedException();
                
                m_UseSso = value;
            }
        }

        /// <summary>
        /// Gets the kerberos principal.
        /// </summary>
        /// <value>The kerberos principal.</value>
        public string KerberosPrincipal
        {
            get { return m_KerberosPrincipal; }
            set { m_KerberosPrincipal = value; }
        }
#endif
	    

        /// <summary>
		/// use "old style" ssl for this connection (Port 5223).
		/// </summary>
		public bool UseSSL
		{
			get	{ return m_UseSSL; }

#if SSL
			set
			{
                // Only one of both can be true
				m_UseSSL = value;
                if (value == true)
                    m_UseStartTLS = false;
			}
#endif
		}

		/// <summary>
		/// use Start-TLS on this connection when the server supports it. Make sure UseSSL is false when 
		/// you want to use this feature.
		/// </summary>
		public bool UseStartTLS
		{
			get { return m_UseStartTLS; }

#if SSL || BCCRYPTO || CF_2
			set
			{
                // Only one of both can be true
				m_UseStartTLS = value;
                if (value == true)
                    m_UseSSL = false;
			}
#endif
		}

        /// <summary>
        /// Use Stream compression to save bandwidth?
        /// This should not be used in combination with StartTLS,
        /// because TLS has build in compression (see RFC 2246, http://www.ietf.org/rfc/rfc2246.txt)
        /// </summary>
        public bool UseCompression
        {
            get { return m_UseCompression; }
			set	{ m_UseCompression = value;	}
        }

		/// <summary>
		/// Are we Authenticated to the server? This is readonly and set by the library
		/// </summary>
		public bool Authenticated
		{
			get { return m_Authenticated; }				
		}

		/// <summary>
		/// is the resource binded? This is readonly and set by the library
		/// </summary>
		public bool Binded
		{
			get { return m_Binded; }				
		}

		/// <summary>
		/// Should the library register a new account on the server
		/// </summary>
		public bool RegisterAccount
		{
			get { return m_RegisterAccount; }
			set { m_RegisterAccount = value; }
		}
	
		public IqGrabber IqGrabber
		{
			get { return m_IqGrabber; }
		}

        public MessageGrabber MessageGrabber
		{
			get { return m_MessageGrabber; }
		}

        public PresenceGrabber PresenceGrabber
        {
            get { return m_PresenceGrabber; }
        }
		
		public RosterManager RosterManager
		{
			get { return m_RosterManager; }
		}

		public PresenceManager PresenceManager
		{
			get { return m_PresenceManager; }
		}       
       
        public bool EnableCapabilities
        {
            get { return m_EnableCapabilities; }
            set { m_EnableCapabilities = value; }
        }

        public string ClientVersion
        {
            get { return m_ClientVersion; }
            set { m_ClientVersion = value; }
        }

        public Capabilities Capabilities
        {
            get { return m_Capabilities; }
            set { m_Capabilities = value; }
        }

        /// <summary>
        /// The DiscoInfo object is used to respond to DiscoInfo request if AutoAnswerDiscoInfoRequests == true in DisoManager objects,
        /// it's also used to build the Caps version when EnableCapabilities is set to true.
        /// <remarks>
        /// When EnableCapailities == true call UpdateCapsVersion after each update of the DiscoInfo object
        /// </remarks>
        /// </summary>
        public DiscoInfo DiscoInfo
        {
            get { return m_DiscoInfo; }
            set { m_DiscoInfo = value; }
        }
		#endregion
		
		#region << Events >>			
		
		/// <summary>
		/// We are authenticated to the server now.
		/// </summary>	
		public event ObjectHandler				OnLogin;
		/// <summary>
		/// This event occurs after the resource was binded
		/// </summary>
		public event ObjectHandler				OnBinded;

        /// <summary>
        /// Event that occurs on bind errors
        /// </summary>
        public event XmppElementHandler         OnBindError;

        /// <summary>
        /// This event is fired when we get register information.
        /// You ca use this event for custom registrations.
        /// </summary>
        public event RegisterEventHandler       OnRegisterInformation;
		
        /// <summary>
		/// This event gets fired after a new account is registered
		/// </summary>
		public event ObjectHandler				OnRegistered;

		/// <summary>
		/// This event ets fired after a ChangePassword Request was successful
		/// </summary>
		public event ObjectHandler				OnPasswordChanged;

		/*
        was never used, comment ot until we need it
		public event XmppElementHandler			OnXmppError;
		*/
         
		/// <summary>
		/// Event that occurs on registration errors
		/// </summary>
		public event XmppElementHandler			OnRegisterError;

        /// <summary>
        /// Event occurs on Xmpp Stream error elements
        /// </summary>
        public event XmppElementHandler         OnStreamError;
                		
		/// <summary>
		/// Event that occurs on authentication errors
		/// e.g. wrong password, user doesnt exist etc...
		/// </summary>
		public event XmppElementHandler			OnAuthError;

        /// <summary>
        /// Event occurs on Socket Errors
        /// </summary>
        public event ErrorHandler               OnSocketError;
        		
		public event ObjectHandler				OnClose;


        /// <summary>
        /// This event is raised when a response to a roster query is received. The roster query contains the contact list.
        /// This lost could be very large and could contain hundreds of contacts. The are all send in a single XML element from 
        /// the server. Normally you show the contact list in a GUI control in you application (treeview, listview). 
        /// When this event occurs you couls Suspend the GUI for faster drawing and show change the mousepointer to the hourglass
        /// </summary>
        /// <remarks>see also OnRosterItem and OnRosterEnd</remarks>
        public event ObjectHandler				OnRosterStart;

        /// <summary>
        /// This event is raised when a response to a roster query is received. It notifies you that all RosterItems (contacts) are
        /// received now.
        /// When this event occurs you could Resume the GUI and show the normal mousepointer again.
        /// </summary>
        /// <remarks>see also OnRosterStart and OnRosterItem</remarks>
        public event ObjectHandler				OnRosterEnd;

        /// <summary>
        /// This event is raised when a response to a roster query is received. This event always contains a single RosterItem. 
        /// e.g. you have 150 friends on your contact list, then this event is called 150 times.
        /// </summary>
        /// <remarks>see also OnRosterItem and OnRosterEnd</remarks>
        public event RosterHandler              OnRosterItem;

        /// <summary>
        /// This event is raised when a response to an agents query which could contain multiple agentitems.
        /// Normally you show the items in a GUI. This event could be used to suspend the UI for faster drawing.
        /// </summary>
        /// <remarks>see also OnAgentItem and OnAgentEnd</remarks>
		public event ObjectHandler				OnAgentStart;

        /// <summary>
        /// This event is raised when a response to an agents query which could contain multiple agentitems.
        /// Normally you show the items in a GUI. This event could be used to resume the suspended userinterface.
        /// </summary>
        /// <remarks>see also OnAgentStart and OnAgentItem</remarks>
        public event ObjectHandler				OnAgentEnd;

        /// <summary>
        /// This event returns always a single AgentItem from a agents query result.
        /// This is from the old jabber protocol. Instead of agents Disco (Service Discovery) should be used in modern
        /// application. But still lots of servers use Agents.
        /// <seealso cref=""/>
        /// </summary>
        /// <remarks>see also OnAgentStart and OnAgentEnd</remarks>
        public event AgentHandler				OnAgentItem;

        /// <summary>
        /// 
        /// </summary>        
        public event IqHandler                  OnIq;	

		/// <summary>
		/// We received a message. This could be a chat message, headline, normal message or a groupchat message. 
        /// There are also XMPP extension which are embedded in messages. 
        /// e.g. X-Data forms.
		/// </summary>
		public event MessageHandler				OnMessage;
		
        /// <summary>
        /// We received a presence from a contact or chatroom.
        /// Also subscriptions is handles in this event.
        /// </summary>
        public event PresenceHandler			OnPresence;
		
        //public event ErrorHandler				OnError;

		public event SaslEventHandler			OnSaslStart;
		public event ObjectHandler				OnSaslEnd;


		#endregion

		#region << Constructors >>
		public XmppClientConnection() : base()
		{			
			m_IqGrabber			= new IqGrabber(this);
			m_MessageGrabber	= new MessageGrabber(this);
            m_PresenceGrabber   = new PresenceGrabber(this);
			m_PresenceManager	= new PresenceManager(this);
			m_RosterManager		= new RosterManager(this);            
		}

		public XmppClientConnection(SocketConnectionType type) : this()
		{
			base.SocketConnectionType = type;
		}

        /// <summary>
        /// create a new XmppClientConnection with the given JabberId and password
        /// </summary>
        /// <param name="jid">JabberId (user@example.com)</param>
        /// <param name="pass">password</param>
        public XmppClientConnection(Jid jid, string pass)
            : this()
        {
            base.Server     = jid.Server;
            this.Username   = jid.User;
            this.Password   = pass;
        }

        /// <summary>
        /// create a new XmppClientConnection with the given server
        /// Username and Password gets set later
        /// </summary>
        /// <param name="server"></param>
		public XmppClientConnection(string server) : this()
		{
			base.Server = server;
		}

        /// <summary>
        /// create a new XmppClientConnection with the given server and port number
        /// Username and Password gets set later
        /// </summary>
        /// <param name="server"></param>
		public XmppClientConnection(string server, int port) : this(server)
		{
			base.Port = port;
		}
		#endregion
                
        /// <summary>
        /// This method open the connections to the xmpp server and authenticates you to ther server.
        /// This method is async, don't assume you are already connected when it returns. You have to wait for the OnLogin Event
        /// </summary>
		public void Open()
		{
			_Open();            
		}       

        /// <summary>
        /// This method open the connections to the xmpp server and authenticates you to ther server.
        /// This method is async, don't assume you are already connected when it returns. You have to wait for the OnLogin Event
        /// </summary>
        /// <param name="username">your username</param>
        /// <param name="password">your password</param>
		public void Open(string username, string password)
		{            
            this.Username   = username;
            this.Password   = password;

			_Open();
		}

        /// <summary>
        /// This method open the connections to the xmpp server and authenticates you to ther server.
        /// This method is async, don't assume you are already connected when it returns. You have to wait for the OnLogin Event
        /// </summary>
        /// <param name="username">your username</param>
        /// <param name="password">your passowrd</param>
        /// <param name="resource">resource for this connection</param>
		public void Open(string username, string password, string resource)
		{
			this.m_Username = username;
			this.m_Password	= password;
			this.m_Resource	= resource;
			_Open();
		}

        /// <summary>
        /// This method open the connections to the xmpp server and authenticates you to ther server.
        /// This method is async, don't assume you are already connected when it returns. You have to wait for the OnLogin Event
        /// </summary>
        /// <param name="username">your username</param>
        /// <param name="password">your password</param>
        /// <param name="resource">resource for this connection</param>
        /// <param name="priority">priority which will be sent with presence packets</param>
		public void Open(string username, string password, string resource, int priority)
		{
			this.m_Username = username;
			this.m_Password	= password;
			this.m_Resource	= resource;
			this.m_Priority	= priority;
			_Open();
		}

        /// <summary>
        /// This method open the connections to the xmpp server and authenticates you to ther server.
        /// This method is async, don't assume you are already connected when it returns. You have to wait for the OnLogin Event
        /// </summary>
        /// <param name="username">your username</param>
        /// <param name="password">your password</param>
        /// <param name="priority">priority which will be sent with presence packets</param>
		public void Open(string username, string password, int priority)
		{
			this.m_Username = username;
			this.m_Password	= password;			
			this.m_Priority	= priority;
			_Open();
		}
            
		#region << Socket handers >>
		public override void SocketOnConnect(object sender)
		{
			base.SocketOnConnect(sender);

            SendStreamHeader(true);
		}

		public override void SocketOnDisconnect(object sender)
		{	
			base.SocketOnDisconnect(sender);

			if(!m_CleanUpDone)
				CleanupSession();
		}

        public override void SocketOnError(object sender, Exception ex)
        {
            base.SocketOnError(sender, ex);

            if ((ex.GetType() == typeof(ConnectTimeoutException) 
                || (ex.GetType() == typeof(SocketException) && ((SocketException)ex).ErrorCode == 10061))
                && _SRVRecords != null
                && _SRVRecords.Length > 1)
            {         
                // connect failed. We are using SRV records and have multiple results.
                // remove the current record
                RemoveSrvRecord(_currentSRVRecord);
                // find and set a new record
                SetConnectServerFromSRVRecords();
                // connect again
                OpenSocket();
            }
            else
            {
                // Fires the socket error
                if (OnSocketError != null)
                    OnSocketError(this, ex);

                // Only cleaneUp Session and raise on close if the stream already has started
                // if teh stream gets closed because of a socket error we have to raise both errors fo course
                if (m_StreamStarted && !m_CleanUpDone)
                    CleanupSession();                
            }
        }		
		#endregion
               
		private void _Open()
		{
            m_CleanUpDone   = false;
            m_StreamStarted = false;

			StreamParser.Reset();
#if SSL
			if (ClientSocket.GetType() == typeof(ClientSocket))
				((ClientSocket) ClientSocket).SSL = m_UseSSL;
#endif		
            // this should start later
            //if (m_KeepAlive)
            //    CreateKeepAliveTimer();
            
            if (SocketConnectionType == SocketConnectionType.Direct && AutoResolveConnectServer)
                ResolveSrv();

            OpenSocket();          
		}

        private void OpenSocket()
        {
            if (ConnectServer == null)
                SocketConnect(base.Server, base.Port);
            else
                SocketConnect(this.ConnectServer, base.Port);
        }

        #region << SRV functions >>
        /// <summary>
        /// Resolves the connection host of a xmpp domain when SRV records are set
        /// </summary>
        private void ResolveSrv()
        {
#if !(CF || CF_2)
            try
            {
                // get the machine's default DNS servers
                var dnsServers = IPConfigurationInformation.DnsServers;

                if (dnsServers.Count > 0)
                {
                    // Take the 1st DNS Server for our query
                    IPAddress dnsServer = dnsServers[0];
                    
                    string queryDomain = SRV_RECORD_PREFIX + Server;
                    
                    _SRVRecords = Resolver.SRVLookup(queryDomain, dnsServer);

                    SetConnectServerFromSRVRecords();   
                }
            }
            catch (Exception ex)
            {
                FireOnError(this, ex);                
            }
#endif
        }

        private void SetConnectServerFromSRVRecords()
        {
            // check we have a response
            if (_SRVRecords != null && _SRVRecords.Length > 0)
            {
                //SRVRecord srv = _SRVRecords[0];
                _currentSRVRecord = PickSRVRecord();

                this.Port           = _currentSRVRecord.Port;
                this.ConnectServer  = _currentSRVRecord.Target;
            }
            else
            {
                // no SRV-Records set
                _currentSRVRecord = null;
                this.ConnectServer = null;
            }
        }

        private void RemoveSrvRecord(SRVRecord rec)
        {
            int i = 0;
            SRVRecord[] recs = new SRVRecord[_SRVRecords.Length - 1];
            foreach (SRVRecord srv in _SRVRecords)
            {
                if (!srv.Equals(rec))
                {
                    recs[i] = srv;
                    i++;
                }
            }
            _SRVRecords = recs;
        }

        /// <summary>
        /// Picks one of the SRV records.
        /// priority and weight are evaluated by the following algorithm.
        /// </summary>
        /// <returns>SRVRecord</returns>
        private SRVRecord PickSRVRecord()
        {
            SRVRecord ret = null;

            // total weight of all servers with the same priority
            int totalWeight = 0;
            
            // ArrayList for the servers with the lowest priority
            ArrayList lowServers = new ArrayList();
            // check we have a response
            if (_SRVRecords != null && _SRVRecords.Length > 0)
            {
                // Find server(s) with the highest priority (could be multiple)
                foreach (SRVRecord srv in _SRVRecords)
                {
                    if (ret == null)
                    {
                        ret = srv;
                        lowServers.Add(ret);
                        totalWeight = ret.Weight;
                    }
                    else
                    {
                        if (srv.Priority == ret.Priority)
                        {
                            lowServers.Add(srv);
                            totalWeight += srv.Weight;
                        }
                        else if (srv.Priority < ret.Priority)
                        {
                            // found a servr with a lower priority
                            // clear the lowServers Array and start with this server
                            lowServers.Clear();
                            lowServers.Add(ret);
                            ret = srv;
                            totalWeight = ret.Weight;
                        }
                        else if (srv.Priority > ret.Priority)
                        {
                            // exit the loop, because servers are already sorted by priority
                            break;
                        }
                    }
                }
            }

            // if we have multiple lowServers then we have to pick a random one
            // BUT we have too involve the weight which can be used for "Load Balancing" here
            if (lowServers.Count > 1)
            {
                if (totalWeight > 0)
                {
                    // Create a random value between 1 - total Weight
                    int rnd = new Random().Next(1, totalWeight);
                    int i = 0;
                    foreach (SRVRecord sr in lowServers)
                    {
                        if (rnd > i && rnd <= (i + sr.Weight))
                        {
                            ret = sr;
                            break;
                        }
                        else
                        {
                            i += sr.Weight;
                        }
                    }
                }
                else
                {
                    // Servers have no weight, they are all equal, pick a random server
                    int rnd = new Random().Next(lowServers.Count);                    
                    ret = (SRVRecord) lowServers[rnd];                    
                }
            }

            return ret;
        }

        #endregion

        private void SendStreamHeader(bool startParser)
        {
            StringBuilder sb = new StringBuilder();


            sb.Append("<stream:stream");
            sb.Append(" to='" + base.Server + "'");
            sb.Append(" xmlns='jabber:client'");
            sb.Append(" xmlns:stream='http://etherx.jabber.org/streams'");

            if (StreamVersion != null)
                sb.Append(" version='" + StreamVersion + "'");

            if (m_ClientLanguage != null)
                sb.Append(" xml:lang='" + m_ClientLanguage + "'");

            // xml:lang="en"<stream:stream to="coversant.net" xmlns="jabber:client" xmlns:stream="http://etherx.jabber.org/streams"  xml:lang="en" version="1.0" >
            // sb.Append(" xml:lang='" + "en" + "' ");


            sb.Append(">");

            Open(sb.ToString());
        }	
               

		/// <summary>
		/// Sends our Presence, the packet is built of Status, Show and Priority
		/// </summary>
		public void SendMyPresence()
		{
            Presence pres = new Presence(m_Show, m_Status, m_Priority);

            // Add client caps when enabled
            if (m_EnableCapabilities)
            {
                if (m_Capabilities.Version == null)
                    UpdateCapsVersion();

                pres.AddChild(m_Capabilities);
            }

            this.Send(pres);
		}

        /// <summary>
        /// Sets the caps version automatically from the DiscoInfo object.
        /// Call this member after each change of the DiscoInfo object
        /// </summary>
        public void UpdateCapsVersion()
        {
            m_Capabilities.SetVersion(m_DiscoInfo);
        }

		internal void RequestLoginInfo()
		{			
			AuthIq iq = new AuthIq(IqType.get, new Jid(base.Server));
			iq.Query.Username = this.m_Username;

			IqGrabber.SendIq(iq, new IqCB(OnGetAuthInfo), null);
		}

		/// <summary>
		/// Changing the Password. You should use this function only when connected with SSL or TLS
		/// because the password is sent in plain text over the connection.		
		/// </summary>
		/// /// <remarks>
		///		<para>
		///			After this request was successful the new password is set automatically in the Username Property
		///		</para>
		/// </remarks>		
		/// <param name="newPass">value of the new password</param>
		public void ChangePassword(string newPass)
		{
			/*
			
			Example 10. Password Change
			<iq type='set' to='somehost' id='change1'>
			<query xmlns='jabber:iq:register'>
				<username>bill</username>
				<password>newpass</password>
			</query>
			</iq>			    

			Because the password change request contains the password in plain text,
			a client SHOULD NOT send such a request unless the underlying stream is encrypted 
			(using SSL or TLS) and the client has verified that the server certificate is signed 
			by a trusted certificate authority. A given domain MAY choose to disable password 
			changes if the stream is not properly encrypted, or to disable in-band password 
			changes entirely.

			If the user provides an empty password element or a password element that contains 
			no XML character data (i.e., either <password/> or <password></password>),
			the server or service MUST NOT change the password to a null value, 
			but instead MUST maintain the existing password.

			Example 11. Host Informs Client of Successful Password Change

			<iq type='result' id='change1'/>			
			*/

			RegisterIq regIq  = new RegisterIq(IqType.set, new Jid(base.Server));			
			regIq.Query.Username = this.m_Username;
			regIq.Query.Password = newPass;
			
			IqGrabber.SendIq(regIq, new IqCB(OnChangePasswordResult), newPass);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="iq"></param>
		/// <param name="data">contains the new password</param>
		private void OnChangePasswordResult(object sender, IQ iq, object data)
		{
			if (iq.Type == IqType.result)
			{
				if(OnPasswordChanged!=null)
					OnPasswordChanged(this);
				
				// Set the new password in the Password property on sucess
				m_Password = (string) data;
			}
			else if (iq.Type == IqType.error)
			{
				/*
				The server or service SHOULD NOT return the original XML sent in 
				IQ error stanzas related to password changes.

				Example 12. Host Informs Client of Failed Password Change (Bad Request)

				<iq type='error' from='somehost' to='user@host/resource' id='change1'>
				<error code='400' type='modify'>
					<conflict xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
				</error>
				</iq>
				    

				Example 13. Host Informs Client of Failed Password Change (Not Authorized)

				<iq type='error' from='somehost' to='user@host/resource' id='change1'>
				<error code='401' type='cancel'>
					<not-authorized xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
				</error>
				</iq>
				    

				Example 14. Server Informs Client of Failed Password Change (Not Allowed)

				<iq type='error' from='somehost' to='user@host/resource' id='change1'>
				<error code='405' type='cancel'>
					<not-allowed xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
				</error>
				</iq>
								  
				*/

				if(OnRegisterError!=null)
					OnRegisterError(this, iq);
			}
        }

        #region << Register new Account >>
        

        /// <summary>
        /// requests the registration fields
        /// </summary>
        /// <param name="obj">object which contains the features node which we need later for login again</param>
        private void GetRegistrationFields(object data)
        {
            // <iq type='get' id='reg1'>
            //  <query xmlns='jabber:iq:register'/>
            // </iq>

            RegisterIq regIq = new RegisterIq(IqType.get, new Jid(base.Server));
            IqGrabber.SendIq(regIq, new IqCB(OnRegistrationFieldsResult), data);
        }

        private void OnRegistrationFieldsResult(object sender, IQ iq, object data)
        {
            if (iq.Type != IqType.error)
            {
                if (iq.Query is Register)
                {
                    RegisterEventArgs args = new RegisterEventArgs(iq.Query as Register);
                    if (OnRegisterInformation != null)
                        OnRegisterInformation(this, args);

                    DoChangeXmppConnectionState(XmppConnectionState.Registering);

                    IQ regIq = new IQ(IqType.set);
                    regIq.GenerateId();
                    regIq.To = new Jid(base.Server);

                    //RegisterIq regIq = new RegisterIq(IqType.set, new Jid(base.Server));
                    if (args.Auto)
                    {
                        Register reg = new Register(this.m_Username, this.m_Password);
                        regIq.Query = reg;
                    }
                    else
                    {
                        regIq.Query = args.Register;
                    }
                    IqGrabber.SendIq(regIq, new IqCB(OnRegisterResult), data);
                }
            }
            else
            {
                if (OnRegisterError != null)
                    OnRegisterError(this, iq);
            }
        }
        
        private void OnRegisterResult(object sender, IQ iq, object data)
		{
			/*
			Example 6. Host Informs Entity of Failed Registration (Username Conflict)

			<iq type='error' id='reg2'>
			<query xmlns='jabber:iq:register'>
				<username>bill</username>
				<password>m1cro$oft</password>
				<email>billg@bigcompany.com</email>
			</query>
			<error code='409' type='cancel'>
				<conflict xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
			</error>
			</iq>
    

			Example 7. Host Informs Entity of Failed Registration (Some Required Information Not Provided)

			<iq type='error' id='reg2'>
			<query xmlns='jabber:iq:register'>
				<username>bill</username>
				<password>Calliope</password>
			</query>
			<error code='406' type='modify'>
				<not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
			</error>
			</iq>
			*/
            if (iq.Type == IqType.result)
            {
                DoChangeXmppConnectionState(XmppConnectionState.Registered);
                if (OnRegistered != null)
                    OnRegistered(this);

                if (this.StreamVersion != null && this.StreamVersion.StartsWith("1."))
                { 
                    // init sasl login
                    InitSaslHandler();
                    m_SaslHandler.OnStreamElement(this, data as Node);
                }
                else
                {
                    // old jabber style login
                    RequestLoginInfo();
                }
            }
            else if (iq.Type == IqType.error)
            {
                if (OnRegisterError != null)
                    OnRegisterError(this, iq);
            }
        }
        #endregion

        private void OnGetAuthInfo(object sender, IQ iq, object data)
		{
			// We get smth like this and should add password (digest) and ressource
			// Recv:<iq type="result" id="MX_7"><query xmlns="jabber:iq:auth"><username>gnauck</username><password/><digest/><resource/></query></iq>
			// Send:<iq type='set' id='mx_login'>
			//			<query xmlns='jabber:iq:auth'><username>gnauck</username><digest>27c05d464e3f908db3b2ca1729674bfddb28daf2</digest><resource>Office</resource></query>
			//		</iq>
			// Recv:<iq id="mx_login" type="result"/> 
			
			iq.GenerateId();
			iq.SwitchDirection();
			iq.Type = IqType.set;

			Auth auth = (Auth) iq.Query;
			
			auth.Resource = this.m_Resource;
			auth.SetAuth(this.m_Username, this.m_Password, this.StreamId);
			
			IqGrabber.SendIq(iq, new IqCB(OnAuthenticate), null);
		}

		/// <summary>
		/// Refreshes the myJid Member Variable
		/// </summary>
		private Jid BuildMyJid()
		{
            Jid jid = new Jid(null);
            
            jid.m_User = m_Username;
            jid.m_Server = Server;
            jid.m_Resource = m_Resource;
            
            jid.BuildJid();

            return jid;			
		}

		#region << RequestAgents >>
		public void RequestAgents()
		{			
			AgentsIq iq = new AgentsIq(IqType.get, new Jid(base.Server));
			IqGrabber.SendIq(iq, new IqCB(OnAgents), null);
		}

		private void OnAgents(object sender, IQ iq, object data)
		{	
			if (OnAgentStart != null)
				OnAgentStart(this);
						
			Agents agents = iq.Query as Agents;
			if (agents != null)
			{
				foreach (Agent a in agents.GetAgents())
				{
					if (OnAgentItem != null)
						OnAgentItem(this, a);				
				}
			}

			if (OnAgentEnd != null)
				OnAgentEnd(this);			
		}
		#endregion

		#region << RequestRoster >>
		public void RequestRoster()
		{		
			RosterIq iq = new RosterIq(IqType.get);
			Send(iq);
		}
		
		private void OnRosterIQ(IQ iq)
		{			
			// if type == result then it must be the "FullRoster" we requested
			// in this case we raise OnRosterStart and OnRosterEnd
			// 
			// if type == set its a new added r updated rosteritem. Here we dont raise
			// OnRosterStart and OnRosterEnd
			if (iq.Type == IqType.result && OnRosterStart != null)
				OnRosterStart(this);

			Roster r = iq.Query as Roster;
			if (r != null)
			{
				foreach (RosterItem i in r.GetRoster())
				{
					if (OnRosterItem != null)
						OnRosterItem(this, i);
				}
			}

            if (iq.Type == IqType.result && OnRosterEnd != null)            
                OnRosterEnd(this);
            
            if (m_AutoPresence && iq.Type == IqType.result)
                SendMyPresence();
		}
		#endregion       

		private void OnAuthenticate(object sender, IQ iq, object data)
		{			
			if (iq.Type == IqType.result)
			{
                m_Authenticated = true;
                RaiseOnLogin();                
			}
			else if(iq.Type == IqType.error)
			{
				/* 
				 * <iq xmlns="jabber:client" id="agsXMPP_2" type="error">
				 *		<query xmlns="jabber:iq:auth">
				 *			<username>test</username>
				 *			<digest sid="842070264">dc7e472abb95b65c2b75129ade607170be478b16</digest>
				 *			<resource>MiniClient</resource>
				 *		</query>
				 *		<error code="401">Unauthorized</error>
				 * </iq>
				 * 
				 */
                if (OnAuthError!=null)
					OnAuthError(this, iq);
			}
			
		}

		internal void FireOnAuthError(Element e)
		{
			if (OnAuthError!=null)
				OnAuthError(this, e);
		}
        
		#region << StreamParser Events >>
		public override void StreamParserOnStreamStart(object sender, Node e)
		{
			base.StreamParserOnStreamStart(this, e);

            m_StreamStarted = true;

			//m_CleanUpDone = false; moved that to _Open();
                            
            protocol.Stream st = (protocol.Stream)e;
            if (st == null)
                return;

            // Read the server language string
            m_ServerLanguage = st.Language;               
        

			// Auth stuff
			if (!RegisterAccount)
			{
				if (this.StreamVersion != null && this.StreamVersion.StartsWith("1."))
				{
					if (!Authenticated)
					{
						// we assume server supports SASL here, because it advertised a StreamVersion 1.X
						// and wait for the stream features and initialize the SASL Handler
                        InitSaslHandler();						
					}				
				}
				else
				{
					// old auth stuff
					RequestLoginInfo();
				}
			}
			else
			{
                // Register on "old" jabber servers without stream features
                if (this.StreamVersion == null)
                    GetRegistrationFields(null);
			}
			
		}

        private void InitSaslHandler()
        {
            if (m_SaslHandler == null)
            {
                m_SaslHandler = new SaslHandler(this);
                m_SaslHandler.OnSaslStart += new SaslEventHandler(m_SaslHandler_OnSaslStart);
                m_SaslHandler.OnSaslEnd += new ObjectHandler(m_SaslHandler_OnSaslEnd);
            }
        }

		public override void StreamParserOnStreamEnd(object sender, Node e)
		{
			base.StreamParserOnStreamEnd(sender, e);			
			
			if (!m_CleanUpDone)
				CleanupSession();
		}

		public override void StreamParserOnStreamElement(object sender, Node e)
		{
			base.StreamParserOnStreamElement(sender, e);

			if (e is IQ)
			{
				if (OnIq != null)
					OnIq(this, e as IQ);
					
				IQ iq = e as IQ;
				if ( iq != null && iq.Query != null)
				{
					// Roster
                    if (iq.Query is Roster)
                        OnRosterIQ(iq);                   
				}	
			}
			else if (e is Message)
			{
				if (OnMessage != null)
					OnMessage(this, e as Message);
			}
			else if (e is Presence)
			{
				if (OnPresence != null)
					OnPresence(this, e as Presence);
			}
			else if (e is Features)
			{
				// Stream Features
				// StartTLS stuff
				Features f = e as Features;
#if SSL || BCCRYPTO || CF_2
				if (f.SupportsStartTls && m_UseStartTLS)
				{
					DoChangeXmppConnectionState(XmppConnectionState.Securing);
					Send(new StartTls());
				}
                else
#endif
                if (m_UseCompression &&
                    f.SupportsCompression &&
                    f.Compression.SupportsMethod(CompressionMethod.zlib))
                {
                    // Check for Stream Compression
                    // we support only ZLIB because its a free algorithm without patents
                    // yes ePatents suck                                       
                    DoChangeXmppConnectionState(XmppConnectionState.StartCompression);
                    Send(new Compress(CompressionMethod.zlib));                    
                }

                else if (m_RegisterAccount)
                {
                    // Do registration after TLS when possible
                    if (f.SupportsRegistration)
                        GetRegistrationFields(e);
                    else
                    {
                        // registration is not enabled on this server                        
                        FireOnError(this, new RegisterException("Registration is not allowed on this server"));
                        Close();
                        // Close the stream
                    }
                }
            }
#if SSL || BCCRYPTO || CF_2
            else if (e is Proceed)
			{	
				StreamParser.Reset();	
		        if (ClientSocket.StartTls())
                {
				    SendStreamHeader(false);
				    DoChangeXmppConnectionState(XmppConnectionState.Authenticating);
                }
            }
#endif
            else if (e is Compressed)
			{
                //DoChangeXmppConnectionState(XmppConnectionState.StartCompression);
				StreamParser.Reset();
                ClientSocket.StartCompression();                
                // Start new Stream Header compressed.
                SendStreamHeader(false);

                DoChangeXmppConnectionState(XmppConnectionState.Compressed);
			}
            else if (e is agsXMPP.protocol.Error)
            {
                if (OnStreamError != null)
                    OnStreamError(this, e as Element);
            }

		}

		public override void StreamParserOnStreamError(object sender, Exception ex)
		{
			base.StreamParserOnStreamError(sender, ex);

			SocketDisconnect();
			CleanupSession();
			
			//this._NetworkStream.Close();
			
			FireOnError(this, ex);

            if (!m_CleanUpDone)
                CleanupSession();           
		}                
        #endregion

       

        public override void Send(Element e)
        {
            if (!(ClientSocket is BoshClientSocket))
            {
                // this is a hack to not send the xmlns="jabber:client" with all packets
                Element dummyEl = new Element("a");
                dummyEl.Namespace = Uri.CLIENT;

                dummyEl.AddChild(e);
                string toSend = dummyEl.ToString();

                Send(toSend.Substring(25, toSend.Length - 25 - 4));
            }
            else
                base.Send(e);
        }
		
		/// <summary>
		/// Does the Clieanup of the Session and sends the OnClose Event
		/// </summary>
		private void CleanupSession()
		{		
			m_CleanUpDone = true;
           
            // TODO, check if this is always OK
            if (ClientSocket.Connected)
			    ClientSocket.Disconnect();
			
            DoChangeXmppConnectionState(XmppConnectionState.Disconnected);
			
			StreamParser.Reset();
			
			m_IqGrabber.Clear();
			m_MessageGrabber.Clear();

			if (m_SaslHandler != null)
			{
				m_SaslHandler.Dispose();
				m_SaslHandler = null;
			}

			m_Authenticated		= false;
			m_Binded			= false;

			DestroyKeepAliveTimer();
			
			if (OnClose!=null)
				OnClose(this);			
		}

		internal void Reset()
		{
            // tell also the socket that we need to reset the stream, this is needed for BOSH
            ClientSocket.Reset();

            StreamParser.Reset();
            SendStreamHeader(false);        
		}
		
		internal void DoRaiseEventBinded()
		{
			if (OnBinded!=null)
				OnBinded(this);
		}

        internal void DoRaiseEventBindError(Element iq)
        {
            if (OnBindError != null)
                OnBindError(this, iq);
        }
        
		#region << SASL Handler Events >>
		private void m_SaslHandler_OnSaslStart(object sender, SaslEventArgs args)
		{
			// This acts only as a tunnel to the client
			if (OnSaslStart!=null)
				OnSaslStart(this, args);
		}

		internal void RaiseOnLogin()
		{
            if (KeepAlive)
                CreateKeepAliveTimer();
                       
			if (OnLogin!=null)
				OnLogin(this);
				
			if(m_AutoAgents)
				RequestAgents();
				
			if (m_AutoRoster)
				RequestRoster();
		}

		private void m_SaslHandler_OnSaslEnd(object sender)
		{
			if (OnSaslEnd!=null)
				OnSaslEnd(this);
					
			m_Authenticated = true;
		}
		#endregion        
	}
}