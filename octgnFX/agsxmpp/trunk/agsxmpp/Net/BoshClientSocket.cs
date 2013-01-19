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
using System.Net;
using System.Text;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;

using agsXMPP.Xml;
using agsXMPP.Xml.Dom;
using agsXMPP.protocol.extensions.bosh;

namespace agsXMPP.Net
{

    public class WebRequestState
    {
        public WebRequestState(WebRequest request)
        {
            m_WebRequest = request;
        }

        DateTime m_Started;

        
        WebRequest m_WebRequest = null;
        Stream m_RequestStream = null;
        string m_Output = null;
        bool m_IsSessionRequest = false;
        Timer m_TimeOutTimer = null;
        private bool m_Aborted = false;

        public int WebRequestId;

        /// <summary>
        /// when was this request started (timestamp)?
        /// </summary>
        public DateTime Started
        {
            get { return m_Started; }
            set { m_Started = value; }
        }

        public bool IsSessionRequest
        {
            get { return m_IsSessionRequest; }
            set { m_IsSessionRequest = value; }
        }

        public string Output
        {
            get { return m_Output; }
            set { m_Output = value; }
        }

        public WebRequest WebRequest
        {
            get { return m_WebRequest; }
            set { m_WebRequest = value; }
        }

        public Stream RequestStream
        {
            get { return m_RequestStream; }
            set { m_RequestStream = value; }
        }

        public Timer TimeOutTimer
        {
            get { return m_TimeOutTimer; }
            set { m_TimeOutTimer = value; }
        }

        public bool Aborted
        {
            get { return m_Aborted; }
            set { m_Aborted = value; }
        }
    } 

    public class BoshClientSocket : BaseSocket
    {
        private const string    CONTENT_TYPE        = "text/xml; charset=utf-8";
        private const string    METHOD              = "POST";
        private const string    BOSH_VERSION        = "1.6";
        private const int       WEBREQUEST_TIMEOUT  = 5000;
        private const int       OFFSET_WAIT         = 5000; 
        
        private string[]        Keys;                                   // Array of keys
        private int             activeRequests = 0;                    // currently active (waiting) WebRequests
        private int             CurrentKeyIdx;                          // index of the currect key
        private Queue           m_SendQueue     = new Queue();          // Queue for stanzas to send
        private bool            streamStarted   = false;                // is the stream started? received stream header?
        private int             polling         = 0;
        private bool            terminate       = false;
        private bool            terminated      = false;
        private DateTime        lastSend        = DateTime.MinValue;    // DateTime of the last activity/response
             
        private bool            m_KeepAlive     = true;
                
        private long            rid;
        private bool            restart         = false;                // stream state, are we currently restarting the stream?
        private string          sid;
        private bool            requestIsTerminating = false;

        private int webRequestId = 1;
        
        public BoshClientSocket(XmppConnection con)
        {
            base.m_XmppCon = con;
        }
        
        private void Init()
        {         
            Keys          = null;
            streamStarted   = false;
            terminate       = false;
            terminated      = false;
        }

        #region << Properties >>
        private Jid             m_To;
        private int             m_Wait          = 300;  // 5 minutes by default, if you think this is to long change it over the public property
        private int             m_Requests      = 2;
        
#if !CF && !CF_2
        private int             m_MinCountKeys  = 1000;
        private int             m_MaxCountKeys  = 9999;        
#else
		// set this lower on embedded devices because the key generation is slow there		
        private int             m_MinCountKeys  = 10;
        private int             m_MaxCountKeys  = 99;
#endif
        private int             m_Hold          = 1;    // should be 1
        private int             m_MaxPause      = 0;
        private WebProxy        m_Proxy         = null;
                
        public Jid To
        {
            get { return m_To; }
            set { m_To = value; }
        }

        /// <summary>
        /// The longest time (in seconds) that the connection manager is allowed to wait before responding to any request during the session.
        /// This enables the client to prevent its TCP connection from expiring due to inactivity, as well as to limit the delay before 
        /// it discovers any network failure.
        /// </summary>
        public int Wait
        {
            get { return m_Wait; }
            set { m_Wait = value; }
        }

        public int Requests
        {
            get { return m_Requests; }
            set { m_Requests = value; }
        }
 
        public int MaxCountKeys
        {
            get { return m_MaxCountKeys; }
            set { m_MaxCountKeys = value; }
        }

        public int MinCountKeys
        {
            get { return m_MinCountKeys; }
            set { m_MinCountKeys = value; }
        }
        
        /// <summary>
        /// This attribute specifies the maximum number of requests the connection manager is allowed to keep waiting 
        /// at any one time during the session. If the client is not able to use HTTP Pipelining then this SHOULD be set to "1".
        /// </summary>
        public int Hold
        {
            get { return m_Hold; }
            set { m_Hold = value; }
        }

        /// <summary>
        /// Keep Alive for HTTP Webrequests, its disables by default because not many BOSH implementations support Keep Alives
        /// </summary>
        public bool KeepAlive
        {
            get { return m_KeepAlive; }
            set { m_KeepAlive = value; }
        }

        /// <summary>
        /// If the connection manager supports session pausing (see Inactivity) then it SHOULD advertise that to the client 
        /// by including a 'maxpause' attribute in the session creation response element. 
        /// The value of the attribute indicates the maximum length of a temporary session pause (in seconds) that a client MAY request.
        /// 0 is the default value and indicated that the connection manager supports no session pausing.
        /// </summary>
        public int MaxPause
        {
            get { return m_MaxPause; }
            set { m_MaxPause = value; }
        }

        public bool SupportsSessionPausing
        {
            get { return !(m_MaxPause == 0); }
        }

        public WebProxy Proxy
        {
            get { return m_Proxy; }
            set { m_Proxy = value; }
        }
        #endregion

        private string DummyStreamHeader
        {
            get
            {
                // <stream:stream xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' id='1075705237'>
                // create dummy stream header
                StringBuilder sb = new StringBuilder();

                sb.Append("<stream:stream");

                sb.Append(" xmlns='");
                sb.Append(Uri.CLIENT);

                sb.Append("' xmlns:stream='");
                sb.Append(Uri.STREAM);

                sb.Append("' id='");
                sb.Append(sid);

                sb.Append("' version='");
                sb.Append("1.0");

                sb.Append("'>");

                return sb.ToString();
            }
        }

        /// <summary>
        /// Generates a bunch of keys
        /// </summary>
        private void GenerateKeys()
        {           
            /*
            13.3 Generating the Key Sequence

            Prior to requesting a new session, the client MUST select an unpredictable counter ("n") and an unpredictable value ("seed").
            The client then processes the "seed" through a cryptographic hash and converts the resulting 160 bits to a hexadecimal string K(1).
            It does this "n" times to arrive at the initial key K(n). The hashing algorithm MUST be SHA-1 as defined in RFC 3174.

            Example 25. Creating the key sequence

                    K(1) = hex(SHA-1(seed))
                    K(2) = hex(SHA-1(K(1)))
                    ...
                    K(n) = hex(SHA-1(K(n-1)))

            */
            int countKeys = GetRandomNumber(m_MinCountKeys, m_MaxCountKeys);

            Keys = new string[countKeys];
            string prev = GenerateSeed();

            for (int i = 0; i < countKeys; i++)
            {
                Keys[i] = Util.Hash.Sha1Hash(prev);
                prev = Keys[i];
            }
            CurrentKeyIdx = countKeys - 1;
        }

        private string GenerateSeed()
        {
            int m_lenght = 10;

#if CF
            util.RandomNumberGenerator rng = util.RandomNumberGenerator.Create();
#else
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
#endif
            byte[] buf = new byte[m_lenght];
            rng.GetBytes(buf);

            return Util.Hash.HexToString(buf);
        }

        private int GenerateRid()
        {
            int min = 1;
            int max = int.MaxValue;
            
            Random rnd = new Random();
            
            return rnd.Next(min, max);
        }

        private int GetRandomNumber(int min, int max)
        {
            Random rnd = new Random();
            return rnd.Next(min, max);
        }

        public override void Reset()
        {
            base.Reset();

            streamStarted   = false;
            restart         = true;
        }

        public void RequestBoshSession()
        {
            /*
            Example 1. Requesting a BOSH session

            POST /webclient HTTP/1.1
            Host: httpcm.jabber.org
            Accept-Encoding: gzip, deflate
            Content-Type: text/xml; charset=utf-8
            Content-Length: 104

            <body content='text/xml; charset=utf-8'
                  hold='1'
                  rid='1573741820'
                  to='jabber.org'
                  route='xmpp:jabber.org:9999'
                  secure='true'
                  ver='1.6'
                  wait='60'
                  ack='1'
                  xml:lang='en'
                  xmlns='http://jabber.org/protocol/httpbind'/>
             */

            lastSend = DateTime.Now;

            // Generate the keys
            GenerateKeys();
            rid = GenerateRid();
            Body body = new Body();
            /*
             * <body hold='1' xmlns='http://jabber.org/protocol/httpbind' 
             *  to='vm-2k' 
             *  wait='300' 
             *  rid='782052' 
             *  newkey='8e7d6cec12004e2bfcf7fc000310fda87bc8337c' 
             *  ver='1.6' 
             *  xmpp:xmlns='urn:xmpp:xbosh' 
             *  xmpp:version='1.0'/>
             */
            //window='5' content='text/xml; charset=utf-8'
            //body.SetAttribute("window", "5");
            //body.SetAttribute("content", "text/xml; charset=utf-8");

            body.Version        = BOSH_VERSION;
            body.XmppVersion    = "1.0";
            body.Hold           = m_Hold;
            body.Wait           = m_Wait;
            body.Rid            = rid;
            body.Polling        = 0;
            body.Requests       = m_Requests;
            body.To             = new Jid(m_XmppCon.Server);           
           
            body.NewKey         = Keys[CurrentKeyIdx];

            body.SetAttribute("xmpp:xmlns", "urn:xmpp:xbosh");            

            activeRequests++;

            HttpWebRequest req = (HttpWebRequest) CreateWebrequest(Address);
            
            WebRequestState state = new WebRequestState(req);
            state.Started           = DateTime.Now;
            state.Output            = body.ToString();
            state.IsSessionRequest  = true;            

            req.Method          = METHOD;
            req.ContentType     = CONTENT_TYPE;
            req.Timeout         = m_Wait * 1000;
            req.KeepAlive       = m_KeepAlive;
            req.ContentLength   = Encoding.UTF8.GetBytes(state.Output).Length; // state.Output.Length;

            try
            {
                IAsyncResult result = req.BeginGetRequestStream(new AsyncCallback(this.OnGetSessionRequestStream), state);
            }
            catch (Exception)
            {                
            }
        }

        private void OnGetSessionRequestStream(IAsyncResult ar)
        {
            try
            {
                WebRequestState state = ar.AsyncState as WebRequestState;
                HttpWebRequest req = state.WebRequest as HttpWebRequest;

                Stream outputStream = req.EndGetRequestStream(ar);

                byte[] bytes = Encoding.UTF8.GetBytes(state.Output);

                state.RequestStream = outputStream;
                IAsyncResult result = outputStream.BeginWrite(bytes, 0, bytes.Length, OnEndWrite, state);
            }
            catch (WebException ex)
            {
                FireOnError(ex);
                Disconnect();
            }
        }

        private void OnGetSessionRequestResponse(IAsyncResult result)
        {
            // grab the custom state object
            WebRequestState state = (WebRequestState)result.AsyncState;
            HttpWebRequest request = (HttpWebRequest)state.WebRequest;

            //state.TimeOutTimer.Dispose();

            // get the Response
            HttpWebResponse resp = (HttpWebResponse)request.EndGetResponse(result);

            // The server must always return a 200 response code,
            // sending any session errors as specially-formatted identifiers.
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                return;
            }

            Stream rs = resp.GetResponseStream();

            int readlen;
            byte[] readbuf = new byte[1024];
            MemoryStream ms = new MemoryStream();
            while ((readlen = rs.Read(readbuf, 0, readbuf.Length)) > 0)
            {
                ms.Write(readbuf, 0, readlen);
            }

            byte[] recv = ms.ToArray();

            if (recv.Length > 0)
            {
                string body = null;
                string stanzas = null;

                string res = Encoding.UTF8.GetString(recv, 0, recv.Length);

                ParseResponse(res, ref body, ref stanzas);
              
                Document doc = new Document();
                doc.LoadXml(body);
                Body boshBody = doc.RootElement as Body;

                sid         = boshBody.Sid;
                polling     = boshBody.Polling;
                m_MaxPause  = boshBody.MaxPause;

                byte[] bin = Encoding.UTF8.GetBytes(DummyStreamHeader + stanzas);
                
                base.FireOnReceive(bin, bin.Length);

                // cleanup webrequest resources
                ms.Close();
                rs.Close();
                resp.Close();

                activeRequests--;

                if (activeRequests == 0)
                    StartWebRequest();                
            }
        }

        /// <summary>
        /// This is ugly code, but currently all BOSH server implementaions are not namespace correct,
        /// which means we can't use the XML parser here and have to spit it with string functions.
        /// </summary>
        /// <param name="res"></param>
        /// <param name="body"></param>
        /// <param name="stanzas"></param>
        private void ParseResponse(string res, ref string body, ref string stanzas)
        {
            res = res.Trim();
            if (res.EndsWith("/>"))
            {
                // <body ..../>
                // empty response
                body = res;
                stanzas = null;
            }
            else
            {
                /* 
                 * <body .....>
                 *  <message/>
                 *  <presence/>
                 * </body>  
                 */

                // find position of the first closing angle bracket
                int startPos = res.IndexOf(">");
                // find position of the last opening angle bracket
                int endPos = res.LastIndexOf("<");

                body = res.Substring(0, startPos) + "/>";
                stanzas = res.Substring(startPos + 1, endPos - startPos - 1);
            }
        }
        
        #region << Public Methods and Functions >>
        public override void Connect()
        {            
            base.Connect();

            Init();
            FireOnConnect();

            RequestBoshSession();            
        }

        public override void Disconnect()
        {
            base.Disconnect();

            FireOnDisconnect();            
            //m_Connected = false;
        }

        public override void Send(byte[] bData)
        {
            base.Send(bData);

            Send(Encoding.UTF8.GetString(bData, 0, bData.Length));
        }


        public override void Send(string data)
        {
            base.Send(data);

            // This are hacks because we send no stream headers and footer in Bosh
            if (data.StartsWith("<stream:stream"))
            {
                if (!streamStarted && !restart)
                    streamStarted = true;
                else
                {
                    byte[] bin = Encoding.UTF8.GetBytes(DummyStreamHeader);
                    base.FireOnReceive(bin, bin.Length);
                }
                return;
            }
            
            if (data.EndsWith("</stream:stream>"))
            {
                protocol.client.Presence pres = new protocol.client.Presence();
                pres.Type = agsXMPP.protocol.client.PresenceType.unavailable;
                data = pres.ToString(); //= "<presence type='unavailable' xmlns='jabber:client'/>";
                terminate = true;
            }
            //    return;

            lock (m_SendQueue)
            {
                m_SendQueue.Enqueue(data);
            }

            CheckDoRequest();
            return;
        }
        #endregion

        private void CheckDoRequest()
        {
            /*
             * requestIsTerminating is true when a webrequest is ending
             * when the requests ends a new request gets started immedialtely,
             * so we don't have to create another request in the case
             */
            if (!requestIsTerminating)
                Request();
        }

        private void Request()
        {
            if (activeRequests < 2)
                StartWebRequest();
        }

        private string BuildPostData()
        {
            CurrentKeyIdx--;
            rid++;

            StringBuilder sb = new StringBuilder();
            
            Body body = new Body();
            
            body.Rid        = rid;            
            body.Key        = Keys[CurrentKeyIdx];

            if (CurrentKeyIdx == 0)
            {
                // this is our last key
                // Generate a new key sequence
                GenerateKeys();
                body.NewKey = Keys[CurrentKeyIdx];
            }

            body.Sid        = sid;
            //body.Polling    = 0;
            body.To = new Jid(m_XmppCon.Server);

            if (restart)
            {
                body.XmppRestart = true;
                restart = false;               
            }

            lock (m_SendQueue)
            {
                if (terminate && m_SendQueue.Count == 1)
                    body.Type = BoshType.terminate;
            
                if (m_SendQueue.Count > 0)
                {
                    sb.Append(body.StartTag());

                    while (m_SendQueue.Count > 0)
                    {
                        string data = m_SendQueue.Dequeue() as string;
                        sb.Append(data);
                    }

                    sb.Append(body.EndTag());

                    return sb.ToString();
                }
                else
                    return body.ToString();
            }
        }

        private void StartWebRequest()
        {
            StartWebRequest(false, null);
        }

        private void StartWebRequest(bool retry, string content)
        {
            lock (this)
            {
                webRequestId++;
            }           
                      
            activeRequests++;

            lastSend = DateTime.Now;

            HttpWebRequest req = (HttpWebRequest) CreateWebrequest(Address);;

            WebRequestState state = new WebRequestState(req);
            state.Started = DateTime.Now;
            state.WebRequestId = webRequestId;

            if (!retry)
                state.Output = BuildPostData();
            else
                state.Output = content;

            req.Method          = METHOD;
            req.ContentType     = CONTENT_TYPE;
            req.Timeout         = m_Wait * 1000;
            req.KeepAlive       = m_KeepAlive;       
            req.ContentLength   = Encoding.UTF8.GetBytes(state.Output).Length;
            
            // Create the delegate that invokes methods for the timer.            
            TimerCallback timerDelegate = TimeOutGetRequestStream;
            Timer timeoutTimer = new Timer(timerDelegate, state, WEBREQUEST_TIMEOUT, WEBREQUEST_TIMEOUT);
            state.TimeOutTimer = timeoutTimer;
            
            try
            {
                req.BeginGetRequestStream(OnGetRequestStream, state);                
            }
            catch(Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        public void TimeOutGetRequestStream(Object stateObj)
        {

            //Console.WriteLine("Web Request timed out");

            WebRequestState state = stateObj as WebRequestState;
            state.TimeOutTimer.Dispose();
            state.Aborted = true;
            state.WebRequest.Abort();
        }

        private void OnGetRequestStream(IAsyncResult ar)
        {
            try
            {
                WebRequestState state = ar.AsyncState as WebRequestState;

                if (state.Aborted)
                {
                    activeRequests--;
                    StartWebRequest(true, state.Output);
                }
                else
                {
                    state.TimeOutTimer.Dispose();
                    HttpWebRequest req = state.WebRequest as HttpWebRequest;

                    Stream requestStream = req.EndGetRequestStream(ar);
                    state.RequestStream = requestStream;
                    byte[] bytes = Encoding.UTF8.GetBytes(state.Output);
                    requestStream.BeginWrite(bytes, 0, bytes.Length, OnEndWrite, state);
                }
            }
            catch (Exception ex)
            {
                activeRequests--;

                WebRequestState state = ar.AsyncState as WebRequestState;
                StartWebRequest(true, state.Output);
            }
        }

        private void OnEndWrite(IAsyncResult ar)
        {
            WebRequestState state = ar.AsyncState as WebRequestState;

            HttpWebRequest req      = state.WebRequest as HttpWebRequest;
            Stream requestStream    = state.RequestStream;

            requestStream.EndWrite(ar);
            requestStream.Close();
            
            IAsyncResult result;
            
            try
            {
                if (state.IsSessionRequest)
                    req.BeginGetResponse(OnGetSessionRequestResponse, state);            
                else
                    req.BeginGetResponse(OnGetResponse, state);     
               
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        private void OnGetResponse(IAsyncResult ar)
        {
            try
            {
                requestIsTerminating = true;
                // grab the custom state object
                WebRequestState state = (WebRequestState)ar.AsyncState;
                HttpWebRequest request = (HttpWebRequest)state.WebRequest;               
                HttpWebResponse resp = null;

                if (request.HaveResponse)
                {                   
                    // TODO, its crashing mostly here
                    // get the Response
                    try
                    {
                        resp = (HttpWebResponse) request.EndGetResponse(ar);
                    }
                    catch (WebException ex)
                    {
                        activeRequests--;
                        requestIsTerminating = false;
                        if (ex.Response == null)
                        {
                            StartWebRequest();
                        }
                        else
                        {
                            HttpWebResponse res = ex.Response as HttpWebResponse;
                            if (res.StatusCode == HttpStatusCode.NotFound)
                            {
                                TerminateBoshSession();
                            }
                        }
                        return;
                    }

                    // The server must always return a 200 response code,
                    // sending any session errors as specially-formatted identifiers.
                    if (resp.StatusCode != HttpStatusCode.OK)
                    {
                        activeRequests--;
                        requestIsTerminating = false;
                        if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            //Console.WriteLine("Not Found");
                            TerminateBoshSession();
                        }
                        return;
                    }
                }
                else
                {
                    //Console.WriteLine("No response");
                }

                Stream rs = resp.GetResponseStream();

                int readlen;
                byte[] readbuf = new byte[1024];
                MemoryStream ms = new MemoryStream();
                while ((readlen = rs.Read(readbuf, 0, readbuf.Length)) > 0)
                {
                    ms.Write(readbuf, 0, readlen);
                }

                byte[] recv = ms.ToArray();

                if (recv.Length > 0)
                {
                    string sbody = null;
                    string stanzas = null;

                    ParseResponse(Encoding.UTF8.GetString(recv, 0, recv.Length), ref sbody, ref stanzas);

                    if (stanzas != null)
                    {
                        byte[] bStanzas = Encoding.UTF8.GetBytes(stanzas);
                        base.FireOnReceive(bStanzas, bStanzas.Length);
                    }
                    else
                    {
                        if (sbody != null)
                        {
                            var doc = new Document();
                            doc.LoadXml(sbody);
                            if (doc.RootElement != null)
                            {
                                var body = doc.RootElement as Body;
                                if (body.Type == BoshType.terminate)
                                    TerminateBoshSession();
                            }
                        }

                        if (terminate && !terminated)
                        {
                            // empty teminate response
                            TerminateBoshSession();
                        }
                    }
                }

                // cleanup webrequest resources
                ms.Close();
                rs.Close();
                resp.Close();

                activeRequests--;
                requestIsTerminating = false;

                //if (activeRequests == 0 && !terminated)
                if ( (activeRequests == 0 && !terminated)
                    || (activeRequests == 1 && m_SendQueue.Count > 0) )
                {                    
                    StartWebRequest();
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private WebRequest CreateWebrequest(string address)
        {
            WebRequest webReq = WebRequest.Create(address);
#if !CF_2
            if (m_Proxy != null)
                webReq.Proxy = m_Proxy;
            else
            {
                if (webReq.Proxy != null)
                    webReq.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            
#endif
            return webReq;
        } 

        private void TerminateBoshSession()
        {
            // empty teminate response
            byte[] bStanzas = Encoding.UTF8.GetBytes("</stream:stream>");
            base.FireOnReceive(bStanzas, bStanzas.Length);
            terminated = true;
        }
    }    
}