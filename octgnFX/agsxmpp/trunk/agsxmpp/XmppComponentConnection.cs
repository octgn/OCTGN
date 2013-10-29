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
using agsXMPP.Net;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol;
using agsXMPP.protocol.component;


namespace agsXMPP
{
	/// <summary>
    /// <para>
	/// use this class to write components that connect to a Jabebr/XMPP server
    /// </para>
    /// <para>
    /// http://www.xmpp.org/extensions/xep-0114.html
    /// </para>
	/// </summary>
	public class XmppComponentConnection : XmppConnection
	{
        // This route stuff is old undocumented jabberd(2) stuff. hopefully we can get rid of this one day
        // or somebody writes up and XEP
		public delegate void RouteHandler	(object sender, Route r);

		private bool						m_CleanUpDone;
        private bool						m_StreamStarted;
        
		#region << Constructors >>
		/// <summary>
		/// Creates a new Component Connection to a given server and port
		/// </summary>
		public XmppComponentConnection()
		{
            m_IqGrabber = new IqGrabber(this);
		}

		/// <summary>
		/// Creates a new Component Connection to a given server and port
		/// </summary>
		/// <param name="server">host/ip of the listening server</param>
		/// <param name="port">port the server listens for the connection</param>
		public XmppComponentConnection(string server, int port) : this()
		{		
			base.Server		= server;
			base.Port		= port;
		}
		
		/// <summary>
		/// Creates a new Component Connection to a given server, port and password (secret)
		/// </summary>
		/// <param name="server">host/ip of the listening server</param>
		/// <param name="port">port the server listens for the connection</param>
		/// <param name="password">password</param>
		public XmppComponentConnection(string server, int port, string password) : this(server,port)
		{		
			this.Password	= password;
		}
		#endregion

		#region << Properties and Member Variables >>
		
		private		string			m_Password			= null;		
		private		bool			m_Authenticated		= false;		
		private		Jid				m_ComponentJid		= null;
        private     IqGrabber       m_IqGrabber         = null;        
				
		public string Password
		{
			get { return m_Password; }
			set { m_Password = value; }
		}
		
		/// <summary>
		/// Are we Authenticated to the server? This is readonly and set by the library
		/// </summary>
		public bool Authenticated
		{
			get { return m_Authenticated; }				
		}

		/// <summary>
		/// The Domain of the component.
		/// <para>
		/// eg: <c>jabber.ag-software.de</c>
		/// </para>
		/// </summary>		
		public Jid ComponentJid
		{
			get { return m_ComponentJid; }
			set { m_ComponentJid = value; }
		}

        public IqGrabber IqGrabber
        {
            get { return m_IqGrabber; }
            set { m_IqGrabber = value; }
        }
		#endregion

		#region << Events >>
		// public event ErrorHandler			OnError;
		
		/// <summary>
		/// connection is authenticated now and ready for receiving Route, Log and Xdb Packets
		/// </summary>
		public event ObjectHandler			OnLogin;

		public event ObjectHandler			OnClose;
		
		/// <summary>
		/// handler for incoming routet packtes from the server
		/// </summary>
		public event RouteHandler			OnRoute;
        
        /// <summary>
        /// Event that occurs on authentication errors
        /// e.g. wrong password, user doesnt exist etc...
        /// </summary>
        public event XmppElementHandler     OnAuthError;
        
        /// <summary>
        /// Stream errors &lt;stream:error/&gt;
        /// </summary>
        public event XmppElementHandler     OnStreamError;

        /// <summary>
        /// Event occurs on Socket Errors
        /// </summary>
        public event ErrorHandler           OnSocketError;

        /// <summary>
        /// 
        /// </summary>        
        public event IqHandler              OnIq;

        /// <summary>
        /// We received a message. This could be a chat message, headline, normal message or a groupchat message. 
        /// There are also XMPP extension which are embedded in messages. 
        /// e.g. X-Data forms.
        /// </summary>
        public event MessageHandler         OnMessage;

        /// <summary>
        /// We received a presence from a contact or chatroom.
        /// Also subscriptions is handles in this event.
        /// </summary>
        public event PresenceHandler        OnPresence;

		#endregion

		public void Open()
		{
			_Open();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="server"></param>
		/// <param name="port"></param>
		public void Open(string server, int port)
		{
			this.Server	= server;
			this.Port	= port;			
			_Open();
		}

		private void _Open()
		{
            m_CleanUpDone   = false;
            m_StreamStarted = false;

            if (ConnectServer == null)
                SocketConnect(base.Server, base.Port);
            else
                SocketConnect(this.ConnectServer, base.Port);	
		}

		private void SendOpenStream()
		{	
			// <stream:stream
			// xmlns='jabber:component:accept'
			// xmlns:stream='http://etherx.jabber.org/streams'
			// to='shakespeare.lit'>
			StringBuilder sb = new StringBuilder();
			
			//sb.Append("<?xml version='1.0'?>");
			sb.Append("<stream:stream");			
			
			if (m_ComponentJid!=null)
				sb.Append(" to='" + m_ComponentJid.ToString() + "'");
			
			sb.Append(" xmlns='" + Uri.ACCEPT + "'");
			sb.Append(" xmlns:stream='" + Uri.STREAM + "'");			
			
			sb.Append(">");			
			
			Open(sb.ToString());
		}

		private void Login()
		{			
			// Send Handshake
			Send( new Handshake(this.m_Password, this.StreamId) );
		}

		#region << Stream Parser events >>
		public override void StreamParserOnStreamStart(object sender, Node e)
		{
			base.StreamParserOnStreamStart (sender, e);
            
            m_StreamStarted = true;
			
            Login();
		}

		public override void StreamParserOnStreamEnd(object sender, Node e)
		{
			base.StreamParserOnStreamEnd (sender, e);

			if(!m_CleanUpDone)
				CleanupSession();
		}

		public override void StreamParserOnStreamElement(object sender, Node e)
		{
			base.StreamParserOnStreamElement (sender, e);

			if (e is Handshake)
			{
				m_Authenticated = true;
                
				if (OnLogin != null)
					OnLogin(this);

                if (KeepAlive)
                    CreateKeepAliveTimer();
			}
			else if (e is Route)
			{
				if (OnRoute != null)
					OnRoute(this, e as Route);
			}
            else if (e is protocol.Error)
            {
                protocol.Error streamErr = e as protocol.Error;
                switch (streamErr.Condition)
                {
                    // Auth errors are important for the users here, so throw catch auth errors
                    // in a separate event here
                    case agsXMPP.protocol.StreamErrorCondition.NotAuthorized:
                        // Authentication Error
                        if (OnAuthError != null)
                            OnAuthError(this, e as Element);
                        break;
                    default:
                        if (OnStreamError != null)
                            OnStreamError(this, e as Element);
                        break;
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
            else if (e is IQ)
            {
                if (OnIq != null)
                    OnIq(this, e as IQ);
            }
		
		}
               
		private void m_StreamParser_OnStreamError(object sender, Exception ex)
		{
			if(!m_CleanUpDone)
				CleanupSession();
		}
		#endregion

		#region << ClientSocket Events >>
		public override void SocketOnConnect(object sender)
		{
			base.SocketOnConnect (sender);

			SendOpenStream();
		}

		public override void SocketOnDisconnect(object sender)
		{
			base.SocketOnDisconnect (sender);

			if(!m_CleanUpDone)
				CleanupSession();
		}

        public override void SocketOnError(object sender, Exception ex)
        {
            base.SocketOnError(sender, ex);
        
            if (m_StreamStarted && !m_CleanUpDone)
                CleanupSession();            

            if (OnSocketError != null)
                OnSocketError(this, ex);
                   
        }
		#endregion

        public override void Send(Element e)
        {
            // this is a hack to not send the xmlns="jabber:component:accept" with all packets                
            Element dummyEl = new Element("a");
            dummyEl.Namespace = Uri.ACCEPT;

            dummyEl.AddChild(e);
            string toSend = dummyEl.ToString();
                       
            Send(toSend.Substring(35, toSend.Length - 35 - 4));
        }

		private void CleanupSession()
		{
            // This cleanup has only to be done if we were able to connect and teh XMPP Stream was started
            DestroyKeepAliveTimer();			
			m_CleanUpDone = true;
			StreamParser.Reset();

            m_IqGrabber.Clear();
            
			if (OnClose!=null)
				OnClose(this);
		}
	}
}
