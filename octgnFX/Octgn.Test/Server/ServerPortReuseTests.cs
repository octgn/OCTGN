using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using Octgn.Online.Hosting;
using Octgn.Server;

namespace Octgn.Test.Server
{
    [TestFixture, NonParallelizable, Timeout(5000)]
    public class ServerPortReuseTests
    {
        private readonly List<Octgn.Server.Server> _servers = new List<Octgn.Server.Server>();
        private readonly List<TcpListener> _owners = new List<TcpListener>();

        [TearDown]
        public void Cleanup()
        {
            foreach (var server in _servers)
            {
                server.Dispose();
                server.Context.State.Players.Dispose();
            }
            foreach (var listener in _owners) listener.Stop();
            _servers.Clear();
            _owners.Clear();
        }

        private static TcpListener Listener(Octgn.Server.Server server)
        {
            return (TcpListener)typeof(Octgn.Server.Server)
                .GetField("_tcp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(server);
        }

        private Octgn.Server.Server CreateServer(int port, bool local = true)
        {
            var game = new HostedGame {
                Id = Guid.NewGuid(), HostAddress = $"127.0.0.1:{port}",
                Status = HostedGameStatus.StartedHosting
            };
            var server = new Octgn.Server.Server(new Config { IsLocal = local }, game, 21234, local);
            _servers.Add(server);
            return server;
        }

        // Reserve an OS-selected contiguous range. All owner sockets and all client
        // connections in this fixture use loopback; Server.Start is never called,
        // because it starts UDP multicast broadcasting before awaiting a client.
        private int ReservePorts(int count, bool reuse = false)
        {
            for (var attempt = 0; attempt < 30; attempt++)
            {
                var range = new List<TcpListener>();
                try
                {
                    var first = new TcpListener(IPAddress.Loopback, 0);
                    range.Add(first);
                    if (reuse) first.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    else first.ExclusiveAddressUse = true;
                    first.Start();
                    var port = ((IPEndPoint)first.LocalEndpoint).Port;
                    if (port + count > IPEndPoint.MaxPort) continue;
                    for (var offset = 1; offset < count; offset++)
                    {
                        var listener = new TcpListener(IPAddress.Loopback, port + offset);
                        range.Add(listener);
                        listener.ExclusiveAddressUse = true;
                        listener.Start();
                    }
                    _owners.AddRange(range);
                    return port;
                }
                catch (SocketException) { }
                finally
                {
                    foreach (var listener in range)
                        if (!_owners.Contains(listener)) listener.Stop();
                }
            }
            Assert.Fail("Could not reserve an owned contiguous loopback port range.");
            return 0;
        }

        private int FreePort()
        {
            var port = ReservePorts(1);
            _owners[_owners.Count - 1].Stop();
            return port;
        }

        [Test]
        public void Server_OwnsPreferredPortFromConstructionUntilDispose()
        {
            var port = FreePort();
            var server = CreateServer(port);
            var listener = Listener(server);
            Assert.AreEqual(port, ((IPEndPoint)listener.LocalEndpoint).Port);
            Assert.IsTrue(listener.ExclusiveAddressUse);
            var competitor = new TcpListener(IPAddress.Loopback, port);
            try
            {
                competitor.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Assert.Throws<SocketException>(() => competitor.Start(), "A second listener must not share the advertised port.");
            }
            finally { competitor.Stop(); }
        }

        [Test]
        public void Server_CanReusePortImmediatelyAfterShutdown()
        {
            var port = FreePort();
            for (var iteration = 0; iteration < 3; iteration++)
            {
                var server = CreateServer(port);
                Assert.AreEqual(port, ((IPEndPoint)Listener(server).LocalEndpoint).Port);
                server.Dispose(); // Also covers shutdown before Server.Start.
            }
        }

        [Test]
        public void Server_FindsAlternativePortWhenPreferredIsUnavailable()
        {
            var port = ReservePorts(2);
            _owners[_owners.Count - 1].Stop(); // Release only the next port.
            var server = CreateServer(port);
            Assert.AreEqual(port + 1, server.Context.Game.Port);
            Assert.AreEqual("127.0.0.1", server.Context.Game.Host);
            Assert.AreEqual(port + 1, ((IPEndPoint)Listener(server).LocalEndpoint).Port);
        }

        [Test]
        public void Server_FallsBackWhenOccupiedOwnerEnabledReuseAddress()
        {
            var port = ReservePorts(2, true);
            _owners[_owners.Count - 1].Stop();
            var server = CreateServer(port);
            Assert.AreEqual(port + 1, server.Context.Game.Port);
            Assert.IsTrue(Listener(server).ExclusiveAddressUse);
        }

        [Test]
        public void Server_StartingBoundListenerAgainPreservesOwnership()
        {
            var port = FreePort();
            var server = CreateServer(port);
            var listener = Listener(server);
            var socket = listener.Server;
            listener.Start(); // Same call that Server.Start makes before accepting.
            Assert.AreSame(socket, listener.Server);
            Assert.AreEqual(port, ((IPEndPoint)listener.LocalEndpoint).Port);
            Assert.IsTrue(listener.ExclusiveAddressUse);
        }

        [Test]
        public void Server_CanReusePortAfterAcceptedConnectionCloses()
        {
            var port = FreePort();
            var server = CreateServer(port);
            var listener = Listener(server);
            var accept = listener.AcceptTcpClientAsync();
            using (var client = new TcpClient())
            {
                client.Connect(IPAddress.Loopback, port);
                Assert.IsTrue(accept.Wait(1000), "The retained listener must accept an owned loopback connection.");
                using (var accepted = accept.Result) accepted.Close();
                client.ReceiveTimeout = 1000;
                Assert.AreEqual(-1, client.GetStream().ReadByte());
            }
            server.Dispose();
            var restarted = CreateServer(port);
            Assert.AreEqual(port, ((IPEndPoint)Listener(restarted).LocalEndpoint).Port);
        }

        [Test]
        public void Server_ThrowsExceptionWhenNoPortsAvailable()
        {
            var port = ReservePorts(10);
            var game = new HostedGame { HostAddress = $"127.0.0.1:{port}" };
            // Invoke only the binding helper so a deliberately failing constructor
            // does not create unrelated PlayerCollection timers that cannot be disposed.
            var server = (Octgn.Server.Server)FormatterServices.GetUninitializedObject(typeof(Octgn.Server.Server));
            var helper = typeof(Octgn.Server.Server).GetMethod("CreateTcpListenerWithPortReuse", BindingFlags.Instance | BindingFlags.NonPublic);
            var exception = Assert.Throws<TargetInvocationException>(() => helper.Invoke(server, new object[] { game }));
            Assert.IsInstanceOf<InvalidOperationException>(exception.InnerException);
            StringAssert.Contains("10 ports", exception.InnerException.Message);
            Assert.AreEqual(port, game.Port);
        }

        [Test]
        public void Server_DefaultBehavior_UsesOriginalTcpListener()
        {
            var server = CreateServer(FreePort(), false);
            Assert.IsFalse(Listener(server).ExclusiveAddressUse);
            Assert.AreEqual(0, (int)Listener(server).Server.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress));
        }
    }
}
