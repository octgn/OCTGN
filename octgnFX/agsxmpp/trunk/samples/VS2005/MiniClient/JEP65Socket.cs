using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


using agsXMPP;
using agsXMPP.net;

namespace MiniClient
{

    /// <summary>
    /// Use async sockets to connect, send and receive data over TCP sockets.
    /// </summary>
    public class JEP65Socket : BaseSocket
    {       
        private const int BUFFERSIZE = 1024;

        private System.Timers.Timer connectTimeoutTimer = new System.Timers.Timer();
        private bool                m_bTimeout          = false;
        private bool                m_SocksConnected    = false;
        private Socket              _socket;
        //private long                m_ConnectTimeout    = 10000; // 10 seconds is default

        private bool                m_SyncConnect       = false;
                      
        private byte[] m_ReadBuffer = null;
        
               
        private string m_SID = null;        
        private Jid m_Initiator;
        private Jid m_Target;

        

        /// <summary>
        /// Object for synchronizing threads
        /// </summary>
        private Object m_Lock = new Object();

        public string SID
        {
            get { return m_SID; }
            set { m_SID = value; }
        }

        public Jid Initiator
        {
            get { return m_Initiator; }
            set { m_Initiator = value; }
        }        

        public Jid Target
        {
            get { return m_Target; }
            set { m_Target = value; }
        }

        //public long ConnectTimeout
        //{
        //    get { return m_ConnectTimeout; }
        //    set { m_ConnectTimeout = value; }
        //}

        public Socket Socket
        {
            get { return _socket; }
        }

        /// <summary>
        /// SHA1 Hash of: (SID + Initiator JID + Target JID)
        /// </summary>
        /// <returns></returns>
        private string BuildHash()
        {           
            return agsXMPP.util.Hash.Sha1Hash(m_SID + m_Initiator.ToString() + m_Target.ToString());            
        }

        #region << Constructor >>
        public JEP65Socket()
        {

        }
        #endregion

        public bool SSL
        {
            get { return false; }
        }
        
        public override bool SupportsStartTls
        {

			get
			{
				return false;
			}
        }

        /// <summary>
        /// Returns true if the socket is connected to the server. The property 
        /// Socket.Connected does not always indicate if the socket is currently 
        /// connected, this polls the socket to determine the latest connection state.
        /// </summary>
        public override bool Connected
        {
            get
            {
                return m_SocksConnected;
            }
        }

        public void Listen(int port)
        {        
            IPEndPoint lep = new IPEndPoint(IPAddress.Any, port);
            Socket s = new Socket(lep.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Bind(lep);
                s.Listen(0);
                                   
                Console.WriteLine("Waiting for a connection...");
                s.BeginAccept(new AsyncCallback(EndAccept), s);                   
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }               

        protected void EndAccept(IAsyncResult ar)
        {

	        // Retrieve the state object and the handler socket
	        // from the async state object.
            Console.WriteLine("Socket Accepted");

            // Get the socket that handles the client request.
            Socket listener = (Socket) ar.AsyncState;
            _socket = listener.EndAccept(ar);
            
            //listener.Shutdown(SocketShutdown.Both);
            listener.Close(); 
            
            m_ReadBuffer = null;
            m_ReadBuffer = new byte[BUFFERSIZE];

            _socket.BeginReceive(m_ReadBuffer, 0, BUFFERSIZE, SocketFlags.None, new AsyncCallback(OnAuthReceiveServer), null);            
        }

        #region << server side stuff >>
        private void OnAuthReceiveServer(IAsyncResult ar)
        {
            // Receive 3 byte Auth
            int rec = _socket.EndReceive(ar);
            
            // VER
            if (m_ReadBuffer[0] != 0x05)
            {	
                throw new Exception("wrong proxy version");
            }

            // NMETHODS (number of methods)
            int nMethods = m_ReadBuffer[1];

            Dictionary<int, int> Methods = new Dictionary<int, int>();
                        
            
            // Parse METHODS
            for (int i = 2; i < nMethods + 2; i++)
            {
                Methods.Add((int)m_ReadBuffer[i], (int)m_ReadBuffer[i]);
            }
                        
            // Send response if everything is OK
            // Send 2 byte response...
            byte[] buffer = new Byte[2];
            buffer[0] = 5;
            if (Methods.ContainsKey(0))
                buffer[1] = 0;            
            else
                buffer[1] = 0xFF;

            _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnSendAuthReply), null);
        }

        private void OnSendAuthReply(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
                // setup the next receive callback
                _socket.BeginReceive(m_ReadBuffer, 0, BUFFERSIZE, SocketFlags.None, new AsyncCallback(OnRequestProxyConnectionReceiveServer), null);
            }
            catch (Exception ex)
            {
                //    ProtocolComplete(e);
                //    return;
            }
        }
        
        private void OnRequestProxyConnectionReceiveServer(IAsyncResult ar)
        {
            /*
               6.  Replies

               The SOCKS request information is sent by the client as soon as it has
               established a connection to the SOCKS server, and completed the
               authentication negotiations.  The server evaluates the request, and
               returns a reply formed as follows:

                    +----+-----+-------+------+----------+----------+
                    |VER | REP |  RSV  | ATYP | BND.ADDR | BND.PORT |
                    +----+-----+-------+------+----------+----------+
                    | 1  |  1  | X'00' |  1   | Variable |    2     |
                    +----+-----+-------+------+----------+----------+

                 Where:

                      o  VER    protocol version: X'05'
                      o  REP    Reply field:
                         o  X'00' succeeded
                         o  X'01' general SOCKS server failure
                         o  X'02' connection not allowed by ruleset
                         o  X'03' Network unreachable
                         o  X'04' Host unreachable
                         o  X'05' Connection refused
                         o  X'06' TTL expired
                         o  X'07' Command not supported
                         o  X'08' Address type not supported
                         o  X'09' to X'FF' unassigned
                      o  RSV    RESERVED
                      o  ATYP   address type of following address
                         o  IP V4 address: X'01'
                         o  DOMAINNAME: X'03'
                         o  IP V6 address: X'04'
                      o  BND.ADDR       server bound address
                      o  BND.PORT       server bound port in network octet order

               Fields marked RESERVED (RSV) must be set to X'00'.

               If the chosen method includes encapsulation for purposes of
               authentication, integrity and/or confidentiality, the replies are
               encapsulated in the method-dependent encapsulation.
            */ 
            
            int rec = _socket.EndReceive(ar);
            /*
                00000000  05 01 00 03 28 66 37 32  61 32 33 65 65 30 31 35   ....(f72 a23ee015 
                00000010  32 35 31 38 31 65 31 63  66 36 62 35 32 33 35 33   25181e1c f6b52353 
                00000020  39 30 39 30 32 65 31 38  39 66 30 63 32 00 00      90902e18 9f0c2..


                [24.04.2006 21:30:36:031]


                00000000  05 00 00 03 28 66 37 32  61 32 33 65 65 30 31 35   ....(f72 a23ee015 
                00000010  32 35 31 38 31 65 31 63  66 36 62 35 32 33 35 33   25181e1c f6b52353 
                00000020  39 30 39 30 32 65 31 38  39 66 30 63 32 00 00 25   90902e18 9f0c2..% 
                00000030  50 44 46 2D 31 2E 33 0A  25 C7 EC 8F A2 0A 35 20   PDF-1.3. %Çì¢.5  

                ... removed ...
                 
                00006880  45 4F 46 0A                                        EOF.
            */

            // *****************
            // * parse request *
            // *****************            
            // Version
            if (m_ReadBuffer[0] != 5)
            {
                throw new Exception("wrong proxy version");
            }

            // CONNECT
            if (m_ReadBuffer[1] != 1)
            {
                throw new Exception("");
            }

            // RESERVERD
            if (m_ReadBuffer[2] != 0)
            {
                throw new Exception("");
            }

            // DOMAINNAME
            if (m_ReadBuffer[3] != 0x03)
            {
                throw new Exception("");
            }

            int lengthVariable = m_ReadBuffer[4]; // Note: MUST be 0x28 == Dec. 40

            string Hash = Encoding.ASCII.GetString(m_ReadBuffer, 5, lengthVariable);
            string Hash2 = BuildHash();

            if (string.Compare(Hash, Hash2, true) == 0)
            {

            }
            else
            {
                throw new Exception("Hash does not match");
            }

            //buffer[4] = (byte)length;
            //Encoding.ASCII.GetBytes(Hash, 0, length, buffer, 5);
            //buffer[5 + length] = 0;
            //buffer[6 + length] = 0;


            byte[] buffer = new Byte[7 + lengthVariable];
            buffer[0] = 5; // protocol version.
            buffer[1] = 0; // succeeded
            buffer[2] = 0; // reserved.
            buffer[3] = 3; // DOMAINNAME
            buffer[4] = (byte)lengthVariable;
            Encoding.ASCII.GetBytes(Hash, 0, lengthVariable, buffer, 5);
            buffer[5 + lengthVariable] = 0;
            buffer[6 + lengthVariable] = 0;

            _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnSendRequestProxyConnectionReply), null);
        }

        private void OnSendRequestProxyConnectionReply(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
                // proxy connection is established sucessfully
                // Setup the NormalizationForm receive callback
                m_SocksConnected = true;
                // TODO, fire OnAccept
                FireOnConnect();

                Receive();
            }
            catch (Exception ex)
            {
                //    ProtocolComplete(e);
                //    return;
            }
        }

        public void SendFile(string filename)
        {
            _socket.BeginSendFile(filename, new AsyncCallback(OnFileSend), null);            
        }

        private void OnFileSend(IAsyncResult ar)
        {
            _socket.EndSendFile(ar);
        }
        #endregion

        public bool SyncConnect()
        {
            lock (m_Lock)
            {
                m_SyncConnect   = true;
                m_bTimeout      = false;
                
                Connect();

                // Timeout
                connectTimeoutTimer.Interval = ConnectTimeout;
                connectTimeoutTimer.Elapsed += new System.Timers.ElapsedEventHandler(connectTimeoutTimer_Elapsed);
                connectTimeoutTimer.Start();

                Monitor.Wait(m_Lock);
                Console.WriteLine("Release Lock");
                Console.WriteLine("sock commected:" + m_SocksConnected.ToString());
                
                return m_SocksConnected;
            }
        }

        public override void Connect()
        {
            base.Connect();

            m_ReadBuffer = null;
            m_ReadBuffer = new byte[BUFFERSIZE];
            
            m_SocksConnected = false;

            IPHostEntry ipHostInfo = Dns.Resolve(Address);
            //IPHostEntry ipHostInfo = Dns.GetHostByAddress(Address);
            //Dns.GetHostEntry
            IPAddress ipAddress = ipHostInfo.AddressList[0];// IPAddress.Parse(address);
            IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            _socket.BeginConnect(endPoint, new AsyncCallback(EndConnect), null);
        }

        /// <summary>
        /// Connect Timeout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connectTimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_bTimeout = true;
            connectTimeoutTimer.Stop();
            _socket.Close();            
        }

        private void SendAuth()
        {
            byte[] buffer = new Byte[3];
            buffer[0] = 0x05;
            buffer[1] = 0x01;
            buffer[2] = 0x00;

            _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnAuthSent), null);
        }

        private void OnAuthSent(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception ex)
            {
            //    ProtocolComplete(e);
            //    return;
            }                    

            _socket.BeginReceive(m_ReadBuffer, 0, m_ReadBuffer.Length, SocketFlags.None, new AsyncCallback(OnAuthReceive), null);            
        }

        private void OnAuthReceive(IAsyncResult ar)
        {
            // Receive 2 byte response...
            int rec = _socket.EndReceive(ar);
            if (rec != 2)
                throw new Exception("Bad response received from proxy server.");

            if (m_ReadBuffer[1] == 0xFF)
            {	// No authentication method was accepted close the socket.
                _socket.Close();
                throw new Exception("None of the authentication method was accepted by proxy server.");
            }

            if (m_ReadBuffer[1] == 0x02)
            {
                //Username Password Authentication
            }

            RequestProxyConnection();
        }

        private void RequestProxyConnection()
        {
            //00000000  05 01 00 03 28 66 37 32  61 32 33 65 65 30 31 35   ....(f72 a23ee015 
            //00000010  32 35 31 38 31 65 31 63  66 36 62 35 32 33 35 33   25181e1c f6b52353 
            //00000020  39 30 39 30 32 65 31 38  39 66 30 63 32 00 00      90902e18 9f0c2..


            string Hash = BuildHash();
            int length = Hash.Length;

            byte[] buffer = new Byte[7 + length];
            buffer[0] = 5; // protocol version.
            buffer[1] = 1; // connect
            buffer[2] = 0; // reserved.
            buffer[3] = 3; // DOMAINNAME
            buffer[4] = (byte)length;
            Encoding.ASCII.GetBytes(Hash, 0, length, buffer, 5);
            buffer[5 + length] = 0;
            buffer[6 + length] = 0;

            //Debug.WriteLine("sending request to proxy to " + RemoteAddress);
            _socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnRequestProxyConnectionSent), null);                     
            
        }

        private void OnRequestProxyConnectionSent(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception ex)
            {
               
            }            
            _socket.BeginReceive(m_ReadBuffer, 0, m_ReadBuffer.Length, SocketFlags.None, new AsyncCallback(OnReadVariableResponseReceive), null);            
        }

        private void OnReadVariableResponseReceive(IAsyncResult ar)
        {
            lock (m_Lock)
            {
                // Read variable response
                long rec = _socket.EndReceive(ar);

                if (m_ReadBuffer[0] != 5)
                {
                    Debug.WriteLine("bogus version in reply from proxy: " + m_ReadBuffer[0]);
                    //return false;
                }
                if (m_ReadBuffer[1] != 0)
                {
                    Debug.WriteLine("request failed on proxy: " + m_ReadBuffer[1]);
                    //return false;
                }

                Debug.WriteLine("proxy complete");
                m_SocksConnected = true;
                connectTimeoutTimer.Stop();

                FireOnConnect();

                // Start async receive
                Receive();

                if (m_SyncConnect)
                    Monitor.Pulse(m_Lock);
            }
        }
        
        private void EndConnect(IAsyncResult ar)
        {
            lock (m_Lock)
            {
                if (m_bTimeout)
                {
                    if (m_SyncConnect)
                        Monitor.Pulse(m_Lock);
                }
                else
                {
                    try
                    {
                        // pass connection status with event
                        _socket.EndConnect(ar);
                        SendAuth();
                    }
                    catch (Exception ex)
                    {
                        base.FireOnError(ex);
                    }
                }
            }
        }     

        /// <summary>
        /// Disconnect from the server.
        /// </summary>
        public override void Disconnect()
        {
            base.Disconnect();

            // return right away if have not created socket
            if (_socket == null)
                return;

            try
            {
                // first, shutdown the socket
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch { }

            try
            {
                // next, close the socket which terminates any pending
                // async operations
                _socket.Close();
            }
            catch { }
            
            m_SocksConnected = false;
            FireOnDisconnect();
        }

        #region << Async Send >>
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public override void Send(string data)
        {
            Send(Encoding.UTF8.GetBytes(data));
        }
        
        /// <summary>
        /// Send data to the server.
        /// </summary>
        public override void Send(byte[] bData)
        {
            base.FireOnSend(bData, bData.Length);
                 
            _socket.BeginSend(bData, 0, bData.Length, SocketFlags.None, new AsyncCallback(EndSend), null);
        }
                

        private void EndSend(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception ex)
            {
            }
        }    
        #endregion

        #region << Async Receive >>
        /// <summary>
        /// Read data from server.
        /// </summary>
        private void Receive()
        {
            _socket.BeginReceive(m_ReadBuffer, 0, BUFFERSIZE, SocketFlags.None, new AsyncCallback(EndReceive), null);
        }

        private void EndReceive(IAsyncResult ar)
        {
            try
            {
                int nBytes;
                nBytes = _socket.EndReceive(ar);
                if (nBytes > 0)
                {                    
                    base.FireOnReceive(m_ReadBuffer, nBytes);
                    
                    // Setup next Receive Callback
                    if (this.Connected)
                        this.Receive();
                }
                else
                {
                    Disconnect();
                }
            }
            catch (ObjectDisposedException)
            {
                //object already disposed, just exit
                return;
            }
            catch (System.IO.IOException ex)
            {
                Console.WriteLine("\nSocket Exception: " + ex.Message);
                Disconnect();
            }
        }
        #endregion      
    
    }
}