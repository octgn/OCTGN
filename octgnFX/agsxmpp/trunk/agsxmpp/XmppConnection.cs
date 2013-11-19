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
using System.Threading;

using agsXMPP.Net;
using agsXMPP.protocol.extensions.bosh;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using agsXMPP.Idn;

namespace agsXMPP
{
	public delegate void XmlHandler			(object sender, string xml);
    public delegate void ErrorHandler       (object sender, Exception ex);

    public delegate void XmppConnectionStateHandler	(object sender, XmppConnectionState state);
	/// <summary>
	/// abstract base class XmppConnection.
	/// </summary>
	public abstract class XmppConnection
	{	

        private Timer m_KeepaliveTimer = null;

		#region << Events >>
		/// <summary>
		/// This event just informs about the current state of the XmppConnection
		/// </summary>
		public event XmppConnectionStateHandler OnXmppConnectionStateChanged;
		
		/// <summary>
		/// a XML packet or text is received. 
		/// This are no winsock events. The Events get generated from the XML parser
		/// </summary>
		public event XmlHandler					OnReadXml;		
		/// <summary>
		/// XML or Text is written to the Socket this includes also the keep alive packages (a single space)		
		/// </summary>
		public event XmlHandler					OnWriteXml;

        public event ErrorHandler               OnError;

        /// <summary>
        /// Data received from the Socket
        /// </summary>
        public event BaseSocket.OnSocketDataHandler OnReadSocketData;
        
        /// <summary>
        /// Data was sent to the socket for sending
        /// </summary>
        public event BaseSocket.OnSocketDataHandler OnWriteSocketData;                      
        
		#endregion

		#region << Constructors >>
		public XmppConnection()
		{
			InitSocket();
			// Streamparser stuff
			m_StreamParser = new StreamParser();
			
            m_StreamParser.OnStreamStart		+= new StreamHandler(StreamParserOnStreamStart);
			m_StreamParser.OnStreamEnd			+= new StreamHandler(StreamParserOnStreamEnd);
			m_StreamParser.OnStreamElement		+= new StreamHandler(StreamParserOnStreamElement);
			m_StreamParser.OnStreamError		+= new StreamError	(StreamParserOnStreamError);
            m_StreamParser.OnError              += new ErrorHandler (StreamParserOnError);            
		}
        
		public XmppConnection(agsXMPP.Net.SocketConnectionType type) : this()
		{
			m_SocketConnectionType = agsXMPP.Net.SocketConnectionType.Direct;
		}
		#endregion

		#region << Properties and Member Variables >>
		private		int						m_Port					    = 5222;
		private		string					m_Server				    = null;
		private		string					m_ConnectServer			    = null;
		private		string					m_StreamId				    = "";
		private		string					m_StreamVersion			    = "1.0";
		private		XmppConnectionState		m_ConnectionState		    = XmppConnectionState.Disconnected;		
		private		BaseSocket				m_ClientSocket			    = null;
		private		StreamParser			m_StreamParser			    = null;
		private		SocketConnectionType	m_SocketConnectionType	    = SocketConnectionType.Direct;
        private     bool                    m_AutoResolveConnectServer  = true;
        private     int                     m_KeepAliveInterval         = 120;
        private     bool                    m_KeepAlive                 = true;
		/// <summary>
		/// The Port of the remote server for the connection
		/// </summary>
		public int Port
		{
			get	{ return m_Port; }
			set { m_Port = value; }
		}

		/// <summary>
		/// domain or ip-address of the remote server for the connection
		/// </summary>
		public string Server
		{
			get { return m_Server; }
            set
            {
#if !STRINGPREP
                if (value != null)
				    m_Server = value.ToLower();
                else
                    m_Server = null;
#else
                if (value != null)
                    m_Server = Stringprep.NamePrep(value);
                else
                    m_Server = null;
#endif
            }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ConnectServer
		{
			get { return m_ConnectServer; }
			set { m_ConnectServer = value; }		
		}
		
		/// <summary>
		/// the id of the current xmpp xml-stream
		/// </summary>
		public string StreamId
		{
			get { return m_StreamId;  }	
			set { m_StreamId = value; }
		}

		/// <summary>
		/// Set to null for old Jabber Protocol without SASL andstream features
		/// </summary>
		public string StreamVersion
		{
			get { return m_StreamVersion; }
			set { m_StreamVersion = value; }
		}

		public XmppConnectionState XmppConnectionState
		{
			get { return m_ConnectionState; }
		}
		
		/// <summary>
		/// Read Online Property ClientSocket
		/// returns the SOcket object used for this connection
		/// </summary>
		public BaseSocket ClientSocket
		{
			get { return m_ClientSocket; }
		}

        /// <summary>
        /// the underlaying XMPP StreamParser. Normally you don't need it, but we make it accessible for
        /// low level access to the stream
        /// </summary>
		public StreamParser StreamParser
		{
			get { return m_StreamParser; }
		}

		public SocketConnectionType SocketConnectionType
		{
			get { return m_SocketConnectionType; }
			set 
			{
				m_SocketConnectionType = value;
				InitSocket();
			}
		}

        public bool AutoResolveConnectServer
        {
            get { return m_AutoResolveConnectServer; }
            set { m_AutoResolveConnectServer = value; }
        }

        /// <summary>
        /// <para>
        /// the keep alive interval in seconds.
        /// Default value is 120
        /// </para>
        /// <para>
        /// Keep alive packets prevent disconenct on NAT and broadband connections which often
        /// disconnect if they are idle.
        /// </para>
        /// </summary>
        public int KeepAliveInterval
        {
            get
            {
                return m_KeepAliveInterval;
            }
            set
            {
                m_KeepAliveInterval = value;
            }
        }
        /// <summary>
        /// Send Keep Alives (for NAT)
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                return m_KeepAlive;
            }
            set
            {
                m_KeepAlive = value;
            }
        }
		#endregion

		#region << Socket handers >>
		public virtual void SocketOnConnect(object sender)
		{
			DoChangeXmppConnectionState(XmppConnectionState.Connected);
		}

		public virtual void SocketOnDisconnect(object sender)
		{
			
		}
        
		public virtual void SocketOnReceive(object sender, byte[] data, int count)
		{
            
            if (OnReadSocketData != null)
                OnReadSocketData(sender, data, count);         
			
            // put the received bytes to the parser
            lock (this)
            {
                StreamParser.Push(data, 0, count);
			}
		}

        public virtual void SocketOnError(object sender, Exception ex)
        {
   
        }
		#endregion

		#region << StreamParser Events >>
		public virtual void StreamParserOnStreamStart		(object sender, Node e)
		{            
            string xml = e.ToString().Trim();
            xml = xml.Substring(0, xml.Length - 2) + ">";

            this.FireOnReadXml(this, xml);

            protocol.Stream st = (protocol.Stream)e;
            if (st != null)
            {
                m_StreamId = st.StreamId;
                m_StreamVersion = st.Version;
            }        
		}

		public virtual void StreamParserOnStreamEnd			(object sender, Node e)
		{
            Element tag = e as Element;
			
			string qName;
			if (tag.Prefix == null)
				qName = tag.TagName;
			else
				qName = tag.Prefix + ":" + tag.TagName;

			string xml = "</" + qName + ">";

			this.FireOnReadXml(this, xml);
		}

		public virtual  void StreamParserOnStreamElement	(object sender, Node e)
		{
			this.FireOnReadXml(this, e.ToString());
		}
		public virtual void StreamParserOnStreamError		(object sender, Exception ex)
		{
		}
        public virtual void StreamParserOnError             (object sender, Exception ex)
        {
            FireOnError(sender, ex);
        }
		#endregion

		internal void DoChangeXmppConnectionState(XmppConnectionState state)
		{
			m_ConnectionState = state;
			
			if (OnXmppConnectionStateChanged!=null)
				OnXmppConnectionStateChanged(this, state);
		}

		private void InitSocket()
		{
			m_ClientSocket = null;

			// Socket Stuff
			if (m_SocketConnectionType == agsXMPP.Net.SocketConnectionType.HttpPolling)
				m_ClientSocket= new PollClientSocket();            
            else if (m_SocketConnectionType == agsXMPP.Net.SocketConnectionType.Bosh)
                m_ClientSocket = new BoshClientSocket(this);
            else
                m_ClientSocket = new ClientSocket();
		
			m_ClientSocket.OnConnect	+= new ObjectHandler(SocketOnConnect);
			m_ClientSocket.OnDisconnect	+= new ObjectHandler(SocketOnDisconnect);
			m_ClientSocket.OnReceive	+= new ClientSocket.OnSocketDataHandler(SocketOnReceive);
            m_ClientSocket.OnError      += new ErrorHandler(SocketOnError);
		}        

		/// <summary>
		/// Starts connecting of the socket
		/// </summary>
		public virtual void SocketConnect()
		{
			DoChangeXmppConnectionState(XmppConnectionState.Connecting);
			ClientSocket.Connect();
		}
		
		public void SocketConnect(string server, int port)
		{	
			ClientSocket.Address	= server;
			ClientSocket.Port		= port;
			SocketConnect();				
		}

		public void SocketDisconnect()
		{
			m_ClientSocket.Disconnect();
		}

		/// <summary>
		/// Send a xml string over the XmppConnection
		/// </summary>
		/// <param name="xml"></param>
		public void Send(string xml)
		{
            FireOnWriteXml(this, xml);
			m_ClientSocket.Send(xml);

            if (OnWriteSocketData != null)
                OnWriteSocketData(this, Encoding.UTF8.GetBytes(xml), xml.Length);

            // reset keep alive timer if active to make sure the interval is always idle time from the last 
            // outgoing packet
            if (m_KeepAlive && m_KeepaliveTimer != null)
                m_KeepaliveTimer.Change(m_KeepAliveInterval * 1000, m_KeepAliveInterval * 1000);
		}

		/// <summary>
		/// Send a xml element over the XmppConnection
		/// </summary>
		/// <param name="e"></param>
		public virtual void Send(Element e)
		{
			Send(e.ToString());
		}

		public void Open(string xml)
		{
			Send(xml);			
		}

		/// <summary>
		/// Send the end of stream
		/// </summary>
		public virtual void Close()
		{
			Send("</stream:stream>");
		}
        		
		protected void FireOnReadXml(object sender, string xml)
		{
			if (OnReadXml != null)
				OnReadXml(sender, xml);
		} 

		protected void FireOnWriteXml(object sender, string xml)
		{
			if (OnWriteXml != null)
				OnWriteXml(sender, xml);
		}

        protected void FireOnError(object sender, Exception ex)
        {
            if (OnError != null)
                OnError(sender, ex);
        }

        #region << Keepalive Timer functions >>
        protected void CreateKeepAliveTimer()
        {
            // Create the delegate that invokes methods for the timer.
            TimerCallback timerDelegate = new TimerCallback(KeepAliveTick);
            int interval = m_KeepAliveInterval * 1000;
            // Create a timer that waits x seconds, then invokes every x seconds.
            m_KeepaliveTimer = new Timer(timerDelegate, null, interval, interval);
        }

        protected void DestroyKeepAliveTimer()
        {
            if (m_KeepaliveTimer == null)
                return;

            m_KeepaliveTimer.Dispose();
            m_KeepaliveTimer = null;
        }

        private void KeepAliveTick(Object state)
        {
            // Send a Space for Keep Alive
            Send(" ");
        }
        #endregion
	}
}