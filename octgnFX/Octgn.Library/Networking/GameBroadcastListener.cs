using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using log4net;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Octgn.Online.Hosting;

namespace Octgn.Library.Networking
{
    public class GameBroadcastListener : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsListening { get; internal set; }

        internal UdpClient Client { get; set; }
        internal MemoryCache GameCache { get; set; }
        internal int Port { get; set; }

        public HostedGame[] Games
        {
            get
            {
                lock (GameCache)
                {
                    var ret = GameCache.Select(x => x.Value).OfType<HostedGame>().ToArray();
                    return ret;
                }
            }
        }

        public GameBroadcastListener(int port = 21234)
        {
            Port = port;
            IsListening = false;
            // Expected: System.InvalidOperationException
            // Additional information: The requested Performance Counter is not a custom counter, it has to be initialized as ReadOnly.
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
                        Client = new UdpClient();
                        Client.ExclusiveAddressUse = false;
                        Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        Client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));
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
                    Client.Client.Disconnect(true);
                    Client.Client.Dispose();
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
                    var hg = (HostedGame)bf.Deserialize(ms);

                    lock (GameCache)
                    {
                        if (GameCache.Contains(hg.Id.ToString()))
                            GameCache.Remove(hg.Id.ToString());
                        GameCache.Add(hg.Id.ToString(), hg, DateTime.Now.AddSeconds(10));

                        if (_awaitingStart.TryGetValue(hg.Id, out var tcs)) {
                            tcs.SetResult(hg);
                        }
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

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<HostedGame>>
            _awaitingStart = new ConcurrentDictionary<Guid, TaskCompletionSource<HostedGame>>();

        public async Task<HostedGame> WaitForGame(Guid id) {
            try {
                var taskSource = _awaitingStart.GetOrAdd(id, (gid) => new TaskCompletionSource<HostedGame>());

                var result = await Task.WhenAny(taskSource.Task, Task.Delay(6000));
                if(result == taskSource.Task) {
                    return taskSource.Task.Result;
                } else {
                    throw new TimeoutException($"{nameof(WaitForGame)}: Timed out");
                }
            } finally {
                _awaitingStart.TryRemove(id, out var asdf);
                asdf.TrySetResult(null);
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