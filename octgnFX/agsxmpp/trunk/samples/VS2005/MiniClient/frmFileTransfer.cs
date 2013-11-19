using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using agsXMPP;
using agsXMPP.net;
using agsXMPP.protocol;
using agsXMPP.protocol.client;
using agsXMPP.protocol.iq;
using agsXMPP.protocol.extensions.si;
using agsXMPP.protocol.extensions.filetransfer;
using agsXMPP.protocol.extensions.featureneg;
using agsXMPP.protocol.extensions.bytestreams;
using agsXMPP.protocol.x.data;

using agsXMPP.Xml;

namespace MiniClient
{
    public partial class frmFileTransfer : Form
    {
        // Add here your file transfer proxy, or disover it with service discovery
        // DONT USE THIS PROXY FOR PRODUCTION. THIS PROXY IS FOR RESTING ONLY. THIS PROXY IS ALSO NOT RUNNING ALL THE TIME
        // Install your own server with bytestream proxy or the external proxy65 module
        const string PROXY = "proxy.ag-software.de";
        //const string PROXY = "proxy.netlab.cz";
        //const string PROXY = "proxy.jabber.org";        

        /// <summary>
        /// SID of the filetransfer
        /// </summary>
        private     string                  m_Sid;
        private     Jid                     m_From;
        private     Jid                     m_To;
        private     DateTime                m_startDateTime;
        private     string                  m_FileName = null;
        
        private     JEP65Socket             _proxySocks5Socket;
        private     JEP65Socket             _p2pSocks5Socket;

        private     XmppClientConnection    m_XmppCon;
        private     bool                    m_DescriptionChanged    = false;

        private     long                    m_lFileLength;
        private     long                    m_bytesTransmitted      = 0;
        private     FileStream              m_FileStream;
        private     DateTime                m_lastProgressUpdate;


        agsXMPP.protocol.extensions.filetransfer.File file;
        agsXMPP.protocol.extensions.si.SI si;
        IQ siIq;
        

        public frmFileTransfer(XmppClientConnection XmppCon, IQ iq)
        {
            InitializeComponent();
            cmdSend.Enabled = false;
            this.Text = "Receive File from " + iq.From.ToString();

            siIq = iq;
            si = iq.SelectSingleElement(typeof(agsXMPP.protocol.extensions.si.SI)) as agsXMPP.protocol.extensions.si.SI;
            // get SID for file transfer
            m_Sid = si.Id;
            m_From = iq.From;
            
            file = si.File;
            
            if (file != null)
            {
                m_lFileLength = file.Size;

                this.lblDescription.Text    = file.Description;
                this.lblFileName.Text       = file.Name;
                this.lblFileSize.Text       = HRSize(m_lFileLength);
                this.txtDescription.Visible = false;
            }

            m_XmppCon = XmppCon;
                       
                        
            this.progress.Maximum = 100;
            //this.Text += iq.From.ToString();
            
            //this.tbFileSize.Text = FileTransferUtils.ConvertToByteString(m_lFileLength);

            XmppCon.OnIq += new IqHandler(XmppCon_OnIq);
        }

        /// <summary>
        /// this constructor is used for outgoing file transfers
        /// </summary>
        /// <param name="XmppCon"></param>
        /// <param name="to"></param>
        public frmFileTransfer(XmppClientConnection XmppCon, Jid to)
        {
            InitializeComponent();
            this.Text = "Send File to " + to.ToString();
            
            m_To = to;
            m_XmppCon = XmppCon;
            
            // disable commadn buttons we don't need for sending a file
            cmdAccept.Enabled = false;
            cmdRefuse.Enabled = false;

            ChooseFileToSend();            
        }

        private void ChooseFileToSend()
        {
            OpenFileDialog of = new OpenFileDialog();
            if (of.ShowDialog() == DialogResult.OK)
            {
                m_FileName = of.FileName;

                lblFileName.Text = m_FileName;
                System.IO.FileInfo fi = new FileInfo(m_FileName);
                lblFileSize.Text = this.HRSize(fi.Length);
                lblFileName.Text = fi.Name;                
                lblDescription.Visible  = false;

                cmdSend.Enabled = true;
            }            
        }

        private void SendSiIq()
        {
            /*            
            Recv: 
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" 
            type="set" id="aab4a"> <si xmlns="http://jabber.org/protocol/si" 
            profile="http://jabber.org/protocol/si/profile/file-transfer" id="s5b_405645b6f2b7c681"> <file 
            xmlns="http://jabber.org/protocol/si/profile/file-transfer" size="719" name="Kunden.dat"> <range /> 
            </file> <feature xmlns="http://jabber.org/protocol/feature-neg"> <x xmlns="jabber:x:data" type="form"> 
            <field type="list-single" var="stream-method"> <option> 
            <value>http://jabber.org/protocol/bytestreams</value> </option> </field> </x> </feature> </si> </iq> 

            Send: <iq xmlns="jabber:client" id="agsXMPP_5" to="gnauck@jabber.org/Psi" type="set">
             <si xmlns="http://jabber.org/protocol/si" id="af5a2e8d-58d4-4038-8732-7fb348ff767f">
             <file xmlns="http://jabber.org/protocol/si/profile/file-transfer" name="ALEX1.JPG" size="22177"><range /></file>
             <feature xmlns="http://jabber.org/protocol/feature-neg"><x xmlns="jabber:x:data" type="form">
            <field type="list-single" var="stream-method"><option>http://jabber.org/protocol/bytestreams</option></field></x></feature></si></iq>
           

            Send:
            <iq xmlns="jabber:client" id="aab4a" to="gnauck@jabber.org/Psi" type="result"><si 
            xmlns="http://jabber.org/protocol/si" id="s5b_405645b6f2b7c681"><feature 
            xmlns="http://jabber.org/protocol/feature-neg"><x xmlns="jabber:x:data" type="submit"><field 
            var="stream-
            method"><value>http://jabber.org/protocol/bytestreams</value></field></x></feature></si></iq> 


            Recv:
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" 
            type="set" id="aab6a"> <query xmlns="http://jabber.org/protocol/bytestreams" sid="s5b_405645b6f2b7c681" 
            mode="tcp"> <streamhost port="8010" jid="gnauck@jabber.org/Psi" host="192.168.74.142" /> <streamhost 
            port="7777" jid="proxy.ag-software.de" host="82.165.34.23"> <proxy 
            xmlns="http://affinix.com/jabber/stream" /> </streamhost> <fast xmlns="http://affinix.com/jabber/stream" 
            /> </query> </iq> 


            Send:
            <iq xmlns="jabber:client" type="result" to="gnauck@jabber.org/Psi" id="aab6a"><query 
            xmlns="http://jabber.org/protocol/bytestreams"><streamhost-used jid="gnauck@jabber.org/Psi" 
            /></query></iq>            
            */

            SIIq iq = new SIIq();
            iq.To = m_To;
            iq.Type = IqType.set;

            m_lFileLength = new FileInfo(m_FileName).Length; 

            agsXMPP.protocol.extensions.filetransfer.File file;
            file =new agsXMPP.protocol.extensions.filetransfer.File(
                        Path.GetFileName(m_FileName), m_lFileLength);
            if (m_DescriptionChanged)
                file.Description = txtDescription.Text;
            file.Range = new Range();
            
            
            FeatureNeg fNeg = new FeatureNeg();
            
            Data data = new Data(XDataFormType.form);
            Field f = new Field(FieldType.List_Single);
            f.Var = "stream-method";
            f.AddOption().SetValue(agsXMPP.Uri.BYTESTREAMS);
            data.AddField(f);

            fNeg.Data = data;

            iq.SI.File = file;
            iq.SI.FeatureNeg = fNeg;
            iq.SI.Profile = agsXMPP.Uri.SI_FILE_TRANSFER;
                        
            m_Sid = Guid.NewGuid().ToString();
            iq.SI.Id = m_Sid;

            m_XmppCon.IqGrabber.SendIq(iq, new IqCB(SiIqResult), null);
        }

        private void SiIqResult(object sender, IQ iq, object data)
        {
            // Parse the result of the form
            if (iq.Type == IqType.result)
            {
                // <iq xmlns="jabber:client" id="aab4a" to="gnauck@jabber.org/Psi" type="result"><si 
                // xmlns="http://jabber.org/protocol/si" id="s5b_405645b6f2b7c681"><feature 
                // xmlns="http://jabber.org/protocol/feature-neg"><x xmlns="jabber:x:data" type="submit"><field 
                // var="stream-
                // method"><value>http://jabber.org/protocol/bytestreams</value></field></x></feature></si></iq> 
                SI si = iq.SelectSingleElement(typeof(SI)) as SI;
                if (si != null)
                {
                    FeatureNeg fNeg = si.FeatureNeg;
                    if ( SelectedByteStream(fNeg) )               
                    {
                        SendStreamHosts();
                    }
                }
            }
            else if(iq.Type == IqType.error)
            {
                agsXMPP.protocol.client.Error err = iq.Error;
                if (err != null)
                {
                    switch ((int) err.Code)
                    {
                        case 403:
                            MessageBox.Show("The file was rejected by the remote user", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;                       
                    }
                }
            }

        }
        
        void _socket_OnDisconnect(object sender)
        {
            
        }

        void _socket_OnConnect(object sender)
        {

        }

        #region << Send Streamhosts >>
        private void SendStreamHosts()
        {
            /*
             Recv:
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" 
            type="set" id="aab6a"> <query xmlns="http://jabber.org/protocol/bytestreams" sid="s5b_405645b6f2b7c681" 
            mode="tcp"> <streamhost port="8010" jid="gnauck@jabber.org/Psi" host="192.168.74.142" />
            <streamhost port="7777" jid="proxy.ag-software.de" host="82.165.34.23">
                <proxy xmlns="http://affinix.com/jabber/stream" />
            </streamhost>
            <fast xmlns="http://affinix.com/jabber/stream" /> </query> </iq> 
            */

            ByteStreamIq bsIq = new ByteStreamIq();
            bsIq.To = m_To;
            bsIq.Type = IqType.set;

            bsIq.Query.Sid = m_Sid;
            
            string hostname = System.Net.Dns.GetHostName();          

            System.Net.IPHostEntry iphe = System.Net.Dns.Resolve(hostname);

            for (int i = 0; i < iphe.AddressList.Length; i++)
            {
                Console.WriteLine("IP address: {0}", iphe.AddressList[i].ToString());
                //bsIq.Query.AddStreamHost(m_XmppCon.MyJID, iphe.AddressList[i].ToString(), 1000);
            } 
            
            bsIq.Query.AddStreamHost(new Jid(PROXY), PROXY, 7777);
            
            _p2pSocks5Socket = new JEP65Socket();
            _p2pSocks5Socket.Initiator = m_XmppCon.MyJID;
            _p2pSocks5Socket.Target = m_To;
            _p2pSocks5Socket.SID = m_Sid;
            _p2pSocks5Socket.OnConnect += new ObjectHandler(_socket_OnConnect);
            _p2pSocks5Socket.OnDisconnect += new ObjectHandler(_socket_OnDisconnect);
            _p2pSocks5Socket.Listen(1000);


            m_XmppCon.IqGrabber.SendIq(bsIq, new IqCB(SendStreamHostsResult), null);
        }

        private void SendStreamHostsResult(object sender, IQ iq, object data)
        {
            //  <iq xmlns="jabber:client" type="result" to="gnauck@jabber.org/Psi" id="aab6a">
            //      <query xmlns="http://jabber.org/protocol/bytestreams">
            //          <streamhost-used jid="gnauck@jabber.org/Psi" />
            //      </query>
            //  </iq>          
            if (iq.Type == IqType.result)
            {
                ByteStream bs = iq.Query as ByteStream;
                if (bs != null)
                {
                    Jid sh = bs.StreamHostUsed.Jid;
                    if (sh != null & sh.Equals(m_XmppCon.MyJID, new agsXMPP.Collections.FullJidComparer()))
                    {
                        // direct connection
                        SendFile(null);
                    }
                    if (sh != null & sh.Equals(new Jid(PROXY), new agsXMPP.Collections.FullJidComparer()))
                    {                        
                        _p2pSocks5Socket = new JEP65Socket();
                        _p2pSocks5Socket.Address = PROXY;
                        _p2pSocks5Socket.Port = 7777;
                        _p2pSocks5Socket.Target = m_To;
                        _p2pSocks5Socket.Initiator = m_XmppCon.MyJID;
                        _p2pSocks5Socket.SID = m_Sid;
                        _p2pSocks5Socket.ConnectTimeout = 5000;
                        _p2pSocks5Socket.SyncConnect();

                        if (_p2pSocks5Socket.Connected)                       
                            ActivateBytestream(new Jid(PROXY));                        
                    }

                }
            }
        }
        #endregion

        #region << Activate ByteStream >>
        /*
            4.9 Activation of Bytestream

            In order for the bytestream to be used, it MUST first be activated by the StreamHost. If the StreamHost is the Initiator, this is straightforward and does not require any in-band protocol. However, if the StreamHost is a Proxy, the Initiator MUST send an in-band request to the StreamHost. This is done by sending an IQ-set to the Proxy, including an <activate/> element whose XML character data specifies the full JID of the Target.

            Example 17. Initiator Requests Activation of Bytestream

            <iq type='set' 
                from='initiator@host1/foo' 
                to='proxy.host3' 
                id='activate'>
              <query xmlns='http://jabber.org/protocol/bytestreams' sid='mySID'>
                <activate>target@host2/bar</activate>
              </query>
            </iq>
                

            Using this information, with the SID and from address on the packet, the Proxy is able to activate the stream by hashing the SID + Initiator JID + Target JID. This provides a reasonable level of trust that the activation request came from the Initiator.

            If the Proxy can fulfill the request, it MUST then respond to the Initiator with an IQ-result.

            Example 18. Proxy Informs Initiator of Activation

            <iq type='result' 
                from='proxy.host3' 
                to='initiator@host1/foo' 
                id='activate'/>   
         
        */
        
        private void ActivateBytestream(Jid streamHost)
        {
            ByteStreamIq bsIq = new ByteStreamIq();
            
            bsIq.To     = streamHost;
            bsIq.Type   = IqType.set;

            bsIq.Query.Sid = m_Sid;
            bsIq.Query.Activate = new Activate(m_To);

            m_XmppCon.IqGrabber.SendIq(bsIq, new IqCB(ActivateBytestreamResult), null);
        }

        private void ActivateBytestreamResult(object sender, IQ iq, object dat)
        {
            if (iq.Type == IqType.result)
            {
                SendFile(null);
            }
        }
        #endregion
        /// <summary>
        /// Sends the file Async
        /// </summary>
        /// <param name="ar"></param>
        private void SendFile(IAsyncResult ar)
        {
            const int BUFFERSIZE = 1024;
            byte[] buffer = new byte[BUFFERSIZE];
            FileStream fs;
            // AsyncResult is null when we call this function the 1st time
            if (ar == null)
            {
               m_startDateTime = DateTime.Now;
               fs = new FileStream(m_FileName, FileMode.Open);
            }
            else
            {

                if (_p2pSocks5Socket.Socket.Connected)
                    _p2pSocks5Socket.Socket.EndReceive(ar);
                
                fs = ar.AsyncState as FileStream;
                
                // Windows Forms are not Thread Safe, we need to invoke this :(
                // We're not in the UI thread, so we need to call BeginInvoke
                // to udate the progress bar
	            TimeSpan ts = DateTime.Now - m_lastProgressUpdate;
                if (ts.Seconds >= 1)
                {
                    BeginInvoke(new ObjectHandler(UpdateProgress), new object[] { this });
                }
            }            

            int len = fs.Read(buffer, 0, BUFFERSIZE);
            m_bytesTransmitted += len;
            
            if (len > 0)
            {            
                _p2pSocks5Socket.Socket.BeginSend(buffer, 0, len, System.Net.Sockets.SocketFlags.None, SendFile, fs);
            }
            else
            {
                // Update Pogress when finished
                BeginInvoke(new ObjectHandler(UpdateProgress), new object[] { this });
                fs.Close();
                fs.Dispose();
                if (_p2pSocks5Socket != null && _p2pSocks5Socket.Connected)
                    _p2pSocks5Socket.Disconnect();
            }
        }       
       
        void XmppCon_OnIq(object sender, agsXMPP.protocol.client.IQ iq)
        {
            //if (InvokeRequired)
            //{
            //    // Windows Forms are not Thread Safe, we need to invoke this :(
            //    // We're not in the UI thread, so we need to call BeginInvoke				
            //    BeginInvoke(new StreamHandler(XmppCon_OnIq), new object[] { sender, e });
            //    return;
            //}

            // <iq xmlns="jabber:client" from="gnauck@jabber.org/Psi" to="gnauck@ag-software.de/SharpIM" type="set" id="aac9a">
            //  <query xmlns="http://jabber.org/protocol/bytestreams" sid="s5b_8596bde0de321957" mode="tcp">
            //   <streamhost port="8010" jid="gnauck@jabber.org/Psi" host="192.168.74.142" />
            //   <streamhost port="7777" jid="proxy.ag-software.de" host="82.165.34.23"> 
            //   <proxy xmlns="http://affinix.com/jabber/stream" /> </streamhost> 
            //   <fast xmlns="http://affinix.com/jabber/stream" /> 
            //  </query> 
            // </iq> 

            
            if (iq.Query != null && iq.Query.GetType() == typeof(agsXMPP.protocol.extensions.bytestreams.ByteStream))
            {
                agsXMPP.protocol.extensions.bytestreams.ByteStream bs = iq.Query as agsXMPP.protocol.extensions.bytestreams.ByteStream;
                // check is this is for the correct file
                if (bs.Sid == m_Sid)
                {
                    Thread t = new Thread(
                        delegate() { HandleStreamHost(bs, iq); }
                    );
                    t.Name = "LoopStreamHosts";
                    t.Start();                 
                }
                
            }            
        }
            

        private void HandleStreamHost(ByteStream bs, IQ iq)
        //private void HandleStreamHost(object obj)
        {
            //IQ iq = obj as IQ;
            //ByteStream bs = iq.Query as agsXMPP.protocol.extensions.bytestreams.ByteStream;;
            //ByteStream bs = iq.Query as ByteStream;
            if (bs != null)
            {
                _proxySocks5Socket = new JEP65Socket();
                _proxySocks5Socket.OnConnect += new ObjectHandler(m_s5Sock_OnConnect);
                _proxySocks5Socket.OnReceive += new agsXMPP.net.BaseSocket.OnSocketDataHandler(m_s5Sock_OnReceive);
                _proxySocks5Socket.OnDisconnect += new ObjectHandler(m_s5Sock_OnDisconnect);

                StreamHost[] streamhosts = bs.GetStreamHosts();
                //Scroll through the possible sock5 servers and try to connect
                //foreach (StreamHost sh in streamhosts)
                //this is done back to front in order to make sure that the proxy JID is chosen first
                //this is necessary as at this stage the application only knows how to connect to a 
                //socks 5 proxy.
                
                foreach (StreamHost sHost in streamhosts)
                {
                    if (sHost.Host != null)
                    {                        
                        _proxySocks5Socket.Address = sHost.Host;
                        _proxySocks5Socket.Port = sHost.Port;
                        _proxySocks5Socket.Target = m_XmppCon.MyJID;
                        _proxySocks5Socket.Initiator = m_From;
                        _proxySocks5Socket.SID = m_Sid;
                        _proxySocks5Socket.ConnectTimeout = 5000;
                        _proxySocks5Socket.SyncConnect();
                        if (_proxySocks5Socket.Connected)
                        {
                            SendStreamHostUsedResponse(sHost, iq);
                            break;
                        }
                    }
                }
                               
            }
        }
                
        private void m_s5Sock_OnDisconnect(object sender)
        {
            m_FileStream.Close();
            m_FileStream.Dispose();

            if (m_bytesTransmitted == m_lFileLength)
            {
                // completed
                tslTransmitted.Text = "completed";
                // Update Progress when complete
                BeginInvoke(new ObjectHandler(UpdateProgress), new object[] { sender });
            }
            else
            {
                // not complete, some error occured or somebody canceled the transfer

            }
        }
                
        void m_s5Sock_OnReceive(object sender, byte[] data, int count)
        {            
            m_FileStream.Write(data, 0, count);

            m_bytesTransmitted += count;            

            
            // Windows Forms are not Thread Safe, we need to invoke this :(
            // We're not in the UI thread, so we need to call BeginInvoke
			// to udate the progress bar	
            TimeSpan ts = DateTime.Now - m_lastProgressUpdate;
            if (ts.Seconds >= 1)
            {
                BeginInvoke(new ObjectHandler(UpdateProgress), new object[] { sender });                  
            }
        }
        
        private void UpdateProgress(object sender)
        {
            m_lastProgressUpdate = DateTime.Now;
            double percent = (double)m_bytesTransmitted / (double)m_lFileLength * 100;
            Console.WriteLine("Percent: " + percent.ToString());
            progress.Value = (int)percent;

            tslRate.Text = GetHRByteRateString();
            tslTransmitted.Text = HRSize(m_bytesTransmitted);
            tslRemaining.Text = GetHRRemainingTime();        
        }

        void m_s5Sock_OnConnect(object sender)
        {
            m_startDateTime = DateTime.Now;

            string path = Util.AppPath + @"\Received Files";
            System.IO.Directory.CreateDirectory(path);
                
            m_FileStream = new FileStream(Path.Combine(path, file.Name), FileMode.Create);

            //throw new Exception("The method or operation is not implemented.");
        }
        
        private void SendStreamHostUsedResponse(StreamHost sh, IQ iq)
        {
            ByteStreamIq bsIQ = new ByteStreamIq(IqType.result, m_From);
            bsIQ.Id = iq.Id;

            bsIQ.Query.StreamHostUsed = new StreamHostUsed(sh.Jid);
            m_XmppCon.Send(bsIQ);
           
        }
        
        private void cmdAccept_Click(object sender, EventArgs e)
        {
            cmdAccept.Enabled = false;
            cmdRefuse.Enabled = false;

            agsXMPP.protocol.extensions.featureneg.FeatureNeg fNeg = si.FeatureNeg;
            if (fNeg != null)
            {
                agsXMPP.protocol.x.data.Data data = fNeg.Data;
                if (data != null)
                {
                    agsXMPP.protocol.x.data.Field[] field = data.GetFields();
                    if (field.Length == 1)
                    {
                        Dictionary<string, string> methods = new Dictionary<string, string>();
                        foreach (agsXMPP.protocol.x.data.Option o in field[0].GetOptions())
                        {
                            string val = o.GetValue();
                            methods.Add(val, val);
                        }
                        if (methods.ContainsKey(agsXMPP.Uri.BYTESTREAMS))
                        {
                            // supports bytestream, so choose this option
                            agsXMPP.protocol.extensions.si.SIIq sIq = new agsXMPP.protocol.extensions.si.SIIq();
                            sIq.Id      = siIq.Id;
                            sIq.To      = siIq.From;
                            sIq.Type    = IqType.result;

                            sIq.SI.Id = si.Id;
                            sIq.SI.FeatureNeg = new agsXMPP.protocol.extensions.featureneg.FeatureNeg();

                            Data xdata = new Data();
                            xdata.Type = XDataFormType.submit;
                            Field f = new Field();
                            //f.Type = FieldType.List_Single;
                            f.Var = "stream-method";
                            f.AddValue(agsXMPP.Uri.BYTESTREAMS);
                            xdata.AddField(f);
                            sIq.SI.FeatureNeg.Data = xdata;                            

                            m_XmppCon.Send(sIq);
                        }
                    }
                }
            }
        }

       
        private bool IsForm(FeatureNeg fn)
        {
            bool bRetVal = false;
            if ((fn != null) && (fn.Data != null))
            {
                if (fn.Data.Type == XDataFormType.form)
                    bRetVal = true;
            }
            return bRetVal;
        }

        private bool SelectedByteStream(FeatureNeg fn)
        {
            if (fn != null)
            {
                Data data = fn.Data;
                if (data != null)
                {
                    foreach (Field field in data.GetFields())
                    {
                        if ( field != null && field.Var == "stream-method" )
                        {
                            if (field.GetValue() == agsXMPP.Uri.BYTESTREAMS)
                                return true;
                        }
                    }
                }
            }
            return false;            
        }
      

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            m_XmppCon.OnIq -= new IqHandler(XmppCon_OnIq);            
        }

        /// <summary>
        /// Reject the file transfer request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRefuse_Click(object sender, EventArgs e)
        {
            /*         
            / from PSI ??? i don't think this is correct
            <iq from="gnauck@jabber.org/Psi" type="error" id="aabaa" to="gnauck@myjabber.net/Psi" >
                <error code="403" >Declined</error>
            </iq>         
            
            <iq type='error' to='sender@jabber.org/resource' id='offer1'>
              <error code='403' type='cancel>
                <forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
                <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Offer Declined</text>
              </error>
            </iq>
            
            Exodus
            <iq id="agsXMPP_5" to="gnauck@ag-software.de/SharpIM" type="error">
                <error code="404" type="cancel">
                    <condition xmlns="urn:ietf:params:xml:ns:xmpp-stanzas">
                        <item-not-found/>
                    </condition>
                </error>
            </iq>
            
            Spark              
            <iq xmlns="jabber:client" from="gnauck@jabber.org/Spark" to="gnauck@ag-software.de/SharpIM" type="error" id="agsXMPP_5">
                <error code="403" />
            </iq> 
            
            
            Example 8. Rejecting Stream Initiation

            <iq type='error' to='sender@jabber.org/resource' id='offer1'>
              <error code='403' type='cancel>
                <forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>
                <text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'>Offer Declined</text>
              </error>
            </iq>
    
            */
            IQ iq = new IQ();

            iq.To   = siIq.From;
            iq.Id   = siIq.Id;
            iq.Type = IqType.error;

            iq.Error = new agsXMPP.protocol.client.Error(ErrorCondition.Forbidden);
            iq.Error.Code = ErrorCode.Forbidden;
            iq.Error.Type = ErrorType.cancel;
            

            m_XmppCon.Send(iq);
            
            this.Close();
        }

        #region << Helper Function >>

        /// <summary>
        /// Converts the Numer of bytes to a human readable string
        /// </summary>
        /// <param name="lBytes"></param>
        /// <returns></returns>
        public string HRSize(long lBytes)
        {
            StringBuilder sb = new StringBuilder();
            string strUnits = "Bytes";
            float fAdjusted = 0.0F;

            if (lBytes > 1024)
            {
                if (lBytes < 1024 * 1024)
                {
                    strUnits = "KB";
                    fAdjusted = Convert.ToSingle(lBytes) / 1024;
                }
                else
                {
                    strUnits = "MB";
                    fAdjusted = Convert.ToSingle(lBytes) / 1048576;
                }
                sb.AppendFormat("{0:0.0} {1}", fAdjusted, strUnits);
            }
            else
            {
                fAdjusted = Convert.ToSingle(lBytes);
                sb.AppendFormat("{0:0} {1}", fAdjusted, strUnits);
            }

            return sb.ToString();
        }

        private long GetBytePerSecond()
        {
            TimeSpan ts = DateTime.Now - m_startDateTime;
            double dBytesPerSecond = m_bytesTransmitted / ts.TotalSeconds;

            return (long) dBytesPerSecond;            
        }

        private string GetHRByteRateString()
        {
            TimeSpan ts = DateTime.Now - m_startDateTime;

            if (ts.TotalSeconds != 0)
            {
                double dBytesPerSecond = m_bytesTransmitted / ts.TotalSeconds;
                long lBytesPerSecond = Convert.ToInt64(dBytesPerSecond);
                return HRSize(lBytesPerSecond) + "/s";
            }
            else
            {
                // to fast to calculate a bitrate (0 seconds)
                return HRSize(0) + "/s";
            }
        }

        private string GetHRRemainingTime()
        {
            float fRemaingTime = 0;
            float fTotalNumberOfBytes = m_lFileLength;
            float fPartialNumberOfBytes = m_bytesTransmitted;
            float fBytesPerSecond = GetBytePerSecond();
            
            StringBuilder sb = new StringBuilder();
            
            if (fBytesPerSecond != 0)
                fRemaingTime = (fTotalNumberOfBytes - fPartialNumberOfBytes) / fBytesPerSecond;
            
            TimeSpan ts = TimeSpan.FromSeconds(fRemaingTime);
            
            return String.Format("{0:00}h {1:00}m {2:00}s",
                                ts.Hours, ts.Minutes, ts.Seconds);
        }        
        #endregion

        private void cmdSend_Click(object sender, EventArgs e)
        {
            SendSiIq();
            // Disable the Send button, because we can send this file only once
            cmdSend.Enabled = false;
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            m_DescriptionChanged = true;
        }

       
    }
}