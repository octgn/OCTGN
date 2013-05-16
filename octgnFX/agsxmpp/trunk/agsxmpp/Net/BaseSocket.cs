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

#if SSL
using System.Net.Security;
#endif
using System.Security.Cryptography.X509Certificates;

namespace agsXMPP.Net
{
	/// <summary>
	/// Base Socket class
	/// </summary>
	public abstract class BaseSocket
	{
		public delegate void OnSocketDataHandler(object sender, byte[] data, int count);

        //public delegate void OnSocketCompressionDebugHandler(object sender, byte[] CompData, int CompCount, byte[] UncompData, int UncompCount);
        
        /*
        // for compression debug statistics
        public event OnSocketCompressionDebugHandler OnIncomingCompressionDebug;
        public event OnSocketCompressionDebugHandler OnOutgoingCompressionDebug;
        
        protected void FireOnInComingCompressionDebug(object sender, byte[] CompData, int CompCount, byte[] UncompData, int UncompCount)
        {
            if (OnIncomingCompressionDebug != null)
                OnIncomingCompressionDebug(sender, CompData, CompCount, UncompData, UncompCount);
        }
        
        protected void FireOnOutgoingCompressionDebug(object sender, byte[] CompData, int CompCount, byte[] UncompData, int UncompCount)
        {
            if (OnOutgoingCompressionDebug != null)
                OnOutgoingCompressionDebug(sender, CompData, CompCount, UncompData, UncompCount);
        }
        */
		
#if SSL
        public event RemoteCertificateValidationCallback    OnValidateCertificate;
#endif
//#if CF_2
//        public delegate bool CertificateValidationCallback(X509Certificate cert);
//        public event CertificateValidationCallback OnValidateCertificate;
//#endif
#if BCCRYPTO
        public delegate bool CertificateValidationCallback(Org.BouncyCastle.Asn1.X509.X509CertificateStructure[] certs);
        public event CertificateValidationCallback OnValidateCertificate;
#endif
		public event OnSocketDataHandler			        OnReceive;
		public event OnSocketDataHandler			        OnSend;
		public event ObjectHandler					        OnConnect;
		public event ObjectHandler					        OnDisconnect;
		public event ErrorHandler					        OnError;

		private string	m_Address		    = null;
		private int		m_Port			    = 0;
        private long    m_ConnectTimeout    = 10000; // 10 seconds is default

        internal XmppConnection  m_XmppCon = null;
        
		public BaseSocket()
		{
		
		}

		public string Address
		{
			get { return m_Address; }
			set { m_Address = value; }
		}

		public int Port
		{
			get { return m_Port; }
			set { m_Port = value; }
		}

		protected void FireOnConnect()
		{
			if (OnConnect != null)
				OnConnect(this);
		} 

		protected void FireOnDisconnect()
		{
			if (OnDisconnect != null)
				OnDisconnect(this);
		} 

		protected void FireOnReceive(byte[] b, int length)
		{
			if (OnReceive != null)
				OnReceive(this, b, length);
		} 
	
		protected void FireOnSend(byte[] b, int length)
		{
			if (OnSend != null)
				OnSend(this, b, length);
		}

		protected void FireOnError(Exception ex)
		{
			if (OnError != null)
				OnError(this, ex);
		}

#if SSL
        // The following method is invoked by the RemoteCertificateValidationDelegate.
        protected bool FireOnValidateCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (OnValidateCertificate != null)
                return OnValidateCertificate(sender, certificate, chain, sslPolicyErrors);
            else
                return true;

            //if (sslPolicyErrors == SslPolicyErrors.None)
            //    return true;

            //Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            //return false;
        }
#endif
#if BCCRYPTO
        protected bool FireOnValidateCertificate(Org.BouncyCastle.Asn1.X509.X509CertificateStructure[] certs)
        {
            if (OnValidateCertificate != null)
                return OnValidateCertificate(certs);
            else
                return true;

        }
#endif

//#if CF_2
//        protected bool FireOnValidateCertificate(X509Certificate cert)
//        {
//            if (OnValidateCertificate != null)
//                return OnValidateCertificate(cert);
//            else
//                return true;
//        }
//#endif
		public virtual bool Connected
		{
			get { return false; }
		}

		public virtual bool SupportsStartTls
		{
			get { return false; }
		}

        public virtual long ConnectTimeout
        {
            get { return m_ConnectTimeout; }
            set { m_ConnectTimeout = value; }
        }

		#region << Methods >>
		public virtual void Connect()
		{

		}

		public virtual void Disconnect()
		{

		}

		public virtual bool StartTls()
		{
		    return true;
		}

        public virtual void StartCompression()
        {

        }

        /// <summary>
        /// Added for Bosh because we have to tell the BoshClientSocket when to reset the stream
        /// </summary>
        public virtual void Reset()
        {

        }

        /// <summary>
		/// 
		/// </summary>
		/// <param name="data"></param>
		public virtual void Send(string data)
		{			
			
		}

		/// <summary>
		/// Send data to the server.
		/// </summary>
		public virtual void Send(byte[] bData)
		{		
			
		}
		#endregion       
    }
}