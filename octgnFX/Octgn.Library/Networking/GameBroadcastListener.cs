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

        public HostedGame[] Games {
            get {
                lock (GameCache) {
                    return GameCache.Select(x => x.Value).OfType<HostedGame>().ToArray();
                }
            }
        }

        public GameBroadcastListener(int port = 21234) {
            Port = port;
            IsListening = false;
            // Expected: System.InvalidOperationException
            // Additional information: The requested Performance Counter is not a custom counter, it has to be initialized as ReadOnly.
            GameCache = new MemoryCache("gamebroadcastlistenercache");
        }

        public void StartListening() {
            if (IsListening)
                throw new InvalidOperationException("Already listening");

            IsListening = true;

            Client = new UdpClient();

            // We want an exception if someone else is bound, otherwise we won't get any packets.
            Client.ExclusiveAddressUse = false;
            Client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Client.ExclusiveAddressUse = false;

            Client.Client.Bind(new IPEndPoint(IPAddress.Any, Port));

            Client.JoinMulticastGroup(GameBroadcaster.MulticastAddress);

            Client.Client.ReceiveTimeout = 1000;

            Receive();
        }

        public void StopListening() {
            if (!IsListening) return;
            IsListening = false;

            try {
                Client.Close();
            } catch (Exception ex) {
                Log.Error($"{nameof(StopListening)}: Error closing client.", ex);
            }

            Client = null;
        }

        private void Receive() {
            if (!IsListening) return;

            var bundle = new SocketReceiveBundle(this.Client);

            this.Client.BeginReceive(EndReceive, bundle);
        }

        private void EndReceive(IAsyncResult res) {
            var state = res.AsyncState as SocketReceiveBundle;
            var ep = new IPEndPoint(IPAddress.Any, Port);
            byte[] data = null;
            try {
                data = state.UdpClient.EndReceive(res, ref ep);
            } catch (ObjectDisposedException ex) {
                Log.Info($"{nameof(EndReceive)}: This is more or less expected", ex);
                return;
            }

            if (data.Length < 4) {
                Receive();
                return;
            }
            var length = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            if (data.Length < length) {
                Receive();
                return;
            }

            data = data.Skip(4).ToArray();

            try {
                using (var ms = new MemoryStream(data)) {
                    ms.Position = 0;
                    var bf = new BinaryFormatter();
                    var hg = (HostedGame)bf.Deserialize(ms);

                    if(hg.Host == "0.0.0.0") {
                        hg.HostAddress = $"{ep.Address}:{hg.Port}";
                    }

                    lock (GameCache) {
                        if (GameCache.Contains(hg.Id.ToString()))
                            GameCache.Remove(hg.Id.ToString());
                        GameCache.Add(hg.Id.ToString(), hg, DateTime.Now.AddSeconds(10));

                        if (_awaitingStart.TryGetValue(hg.Id, out var tcs)) {
                            tcs.SetResult(hg);
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error($"{nameof(EndReceive)}: Error reading data", ex);
            }

            Receive();
        }

        private readonly ConcurrentDictionary<Guid, TaskCompletionSource<HostedGame>>
            _awaitingStart = new ConcurrentDictionary<Guid, TaskCompletionSource<HostedGame>>();

        public async Task<HostedGame> WaitForGame(Guid id) {
            try {
                var taskSource = _awaitingStart.GetOrAdd(id, (gid) => new TaskCompletionSource<HostedGame>());

                var result = await Task.WhenAny(taskSource.Task, Task.Delay(6000));
                if (result == taskSource.Task) {
                    return taskSource.Task.Result;
                } else {
                    throw new TimeoutException($"{nameof(WaitForGame)}: Timed out");
                }
            } finally {
                _awaitingStart.TryRemove(id, out var asdf);
                asdf.TrySetResult(null);
            }
        }

        public void Dispose() {
            try {
                if (IsListening)
                    StopListening();
            } catch (InvalidOperationException ex) {
                Log.Warn("If it's a 'Not listening' exception then it doesn't matter.", ex);
            }
        }
    }
}