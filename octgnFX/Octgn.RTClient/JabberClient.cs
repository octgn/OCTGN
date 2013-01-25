namespace Octgn.RTClient
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    using Windows.Foundation;
    using Windows.Networking;
    using Windows.Networking.Sockets;
    using Windows.Storage.Streams;

    public class JabberClient
    {
        public static JabberClient Current { get; set; }

        public StreamSocket Socket { get; set; }
        public bool IsConnected { get; private set; }

        public JabberClient()
        {
            Socket = new StreamSocket();

            IsConnected = false;
            Current = this;
        }

        public void Connect()
        {
            if (IsConnected)
                throw new Exception("Already connected.");
            Socket.ConnectAsync(new HostName("of.octgn.net"), "5222").Completed = this.ConnectCompleted;
        }

        private async void ConnectCompleted(IAsyncAction asyncInfo, AsyncStatus asyncStatus)
        {
            if (asyncInfo.Status == AsyncStatus.Error)
                throw new NotImplementedException("Connection failed");
            var buffer = new Windows.Storage.Streams.Buffer(1024);
            IsConnected = true;
            var w = new Windows.Storage.Streams.DataWriter(Socket.OutputStream);
            w.WriteString("<stream:stream to='of.octgn.net' xmlns='jabber:client' xmlns:stream='http://etherx.jabber.org/streams' version='1.0' xml:lang='en'>");
            await w.StoreAsync();
            new Task(this.StartReading).Start();
        }

        private async void StartReading()
        {
            var reader = new DataReader(Socket.InputStream);
            var bufferString = "";
            var textReader = new StringReader(bufferString);
            var xmlreader = XmlReader.Create(textReader);
            while (true)
            {
                //if (reader.UnconsumedBufferLength <= 0)
                //{
                //    await Task.Delay(10);
                //    continue;
                //}
                var len = await reader.LoadAsync(8);
                var str = reader.ReadString(len);
                bufferString += str;
                if(xmlreader.CanResolveEntity)
                    if(xmlreader.Read())
                        Debug.WriteLine(xmlreader.ReadContentAsString());
            }
        }
    }
}
