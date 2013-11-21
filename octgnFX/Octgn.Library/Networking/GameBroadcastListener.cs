using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;

namespace Octgn.Library.Networking
{
    public class GameBroadcastListener : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsListening { get; internal set; }

        internal UdpClient Client { get; set; }
        internal MemoryCache GameCache { get; set; }
        internal int Port { get; set; }

        public IHostedGameData[] Games
        {
            get
            {
                lock (GameCache)
                {
                    var ret = GameCache.Select(x => x.Value).OfType<IHostedGameData>().ToArray();
                    return ret;
                }
            }
        }

        public GameBroadcastListener(int port = 21234)
        {
            Port = port;
            IsListening = false;
            GameCache = new MemoryCache("gamebroadcastlistenercache");
        }

        public void StartListening()
        {
            lock (this)
            {
                if (IsListening)
                {
                    return;
                }
                try
                {
                    if (Client == null)
                    {
                        Client = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
                        Client.Client.ReceiveTimeout = 1000;
                    }

                    Receive();

                    IsListening = true;
                }
                catch (Exception e)
                {
                    Log.Error("Error listening", e);
                }
            }
        }

        public void StopListening()
        {
            lock (this)
            {
                if (!IsListening)
                    return;

                try
                {
                    Client.Close();
                }
                catch
                {
                }

                Client = null;
                IsListening = false;
            }
        }

        private void Receive()
        {
            var bundle = new SocketReceiveBundle(this.Client);

            this.Client.BeginReceive(EndReceive, bundle);
        }

        private void EndReceive(IAsyncResult res)
        {
            try
            {
                var state = res.AsyncState as SocketReceiveBundle;
                var ep = new IPEndPoint(IPAddress.Any, Port);
                var data = state.UdpClient.EndReceive(res, ref ep);

                if (data.Length < 4) return;
                var length = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
                if (data.Length < length) return;

                data = data.Skip(4).ToArray();

                using (var ms = new MemoryStream(data))
                {
                    ms.Position = 0;
                    var bf = new BinaryFormatter();
                    var hg = (IHostedGameData)bf.Deserialize(ms);

                    hg.TimeStarted = hg.TimeStarted.ToLocalTime();
                    hg.IpAddress = ep.Address;
                    lock (GameCache)
                    {
                        if (GameCache.Contains(hg.Id.ToString()))
                            GameCache.Remove(hg.Id.ToString());
                        GameCache.Add(hg.Id.ToString(), hg, DateTime.Now.AddSeconds(10));
                    }
                }

            }
            catch (Exception e)
            {
                Log.Error("EndReceive", e);
            }
            finally
            {
                if (IsListening)
                {
                    Receive();
                }
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopListening();
            }
            catch { }
        }

        #endregion
    }
}