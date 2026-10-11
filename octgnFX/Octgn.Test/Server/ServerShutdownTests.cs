using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using NUnit.Framework;
using Octgn.Online.Hosting;
using Octgn.Server;

namespace Octgn.Test.Server
{
    [TestFixture, NonParallelizable]
    public class ServerShutdownTests
    {
        private readonly List<Octgn.Server.Server> _servers = new List<Octgn.Server.Server>();

        private Octgn.Server.Server CreateUnstartedServer()
        {
            // The nonlocal constructor does not bind TCP. Avoid Server.Start,
            // which would begin UDP broadcasting before awaiting its first client.
            var game = new HostedGame {
                Id = Guid.NewGuid(), HostAddress = "127.0.0.1:1",
                Status = HostedGameStatus.StartedHosting
            };
            var server = new Octgn.Server.Server(new Config { IsLocal = true }, game, 21234);
            _servers.Add(server);
            return server;
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var server in _servers)
            {
                try
                {
                    server.Dispose();
                }
                finally
                {
                    try
                    {
                        server.Context.State.Players.Dispose();
                    }
                    finally
                    {
                        try
                        {
                            ((IDisposable)typeof(Octgn.Server.Server).GetField("_broadcaster",
                                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(server)).Dispose();
                        }
                        finally
                        {
                            ((TcpListener)typeof(Octgn.Server.Server).GetField("_tcp",
                                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(server)).Server.Dispose();
                        }
                    }
                }
            }
            _servers.Clear();
        }

        [Test]
        public void DisposeBeforeStart_NotifiesStopExactlyOnce()
        {
            var server = CreateUnstartedServer();
            var stops = 0;
            object stoppedServer = null;
            server.OnStop += (sender, args) => { stops++; stoppedServer = sender; };

            Assert.DoesNotThrow(() => server.Dispose());
            Assert.DoesNotThrow(() => server.Dispose());

            Assert.AreEqual(1, stops, "Disposal must deliver the stop notification even when no listener task was started.");
            Assert.AreSame(server, stoppedServer);
        }

        [Test]
        public void DisposeBeforeStart_StopCallbackCanDisposeAgain()
        {
            var server = CreateUnstartedServer();
            var stops = 0;
            server.OnStop += (sender, args) => { stops++; server.Dispose(); };

            Assert.DoesNotThrow(() => server.Dispose());
            Assert.DoesNotThrow(() => server.Dispose());

            Assert.AreEqual(1, stops, "A reentrant owner callback must not invoke the stop notification again.");
        }

        [Test]
        public void DisposeBeforeStart_FaultingCallbackDoesNotRepeatNotification()
        {
            var server = CreateUnstartedServer();
            var stops = 0;
            server.OnStop += (sender, args) => {
                stops++;
                throw new InvalidOperationException("Owned test stop callback failure.");
            };

            Assert.DoesNotThrow(() => server.Dispose());
            Assert.DoesNotThrow(() => server.Dispose());

            Assert.AreEqual(1, stops, "Callback failures must retain the existing shutdown-once behavior.");
        }
    }
}
