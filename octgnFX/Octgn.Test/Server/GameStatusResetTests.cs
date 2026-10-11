using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using Octgn.Server;
using Octgn.Online.Hosting;

namespace Octgn.Test.Server
{
    [TestFixture]
    public class GameStatusResetTests
    {
        [Test]
        public void AllPlayersDisconnected_GameInProgress_ResetsStatusToStartedHosting()
        {
            using (var game = new TestGame())
            {
                var first = game.RegisterPlayer();
                var second = game.RegisterPlayer();
                game.Context.Game.Status = HostedGameStatus.GameInProgress;
                Assert.AreEqual(HostedGameStatus.GameInProgress, game.Context.Game.Status);

                game.Disconnect(first);
                game.Disconnect(second);
                game.Tick();

                Assert.AreEqual(HostedGameStatus.StartedHosting, game.Context.Game.Status,
                    "Game status should reset when all registered players disconnect");
                Assert.AreEqual(0, game.AllPlayersDisconnectedEvents,
                    "Status resets before the delayed shutdown event");
            }
        }

        [Test]
        public void AllPlayersDisconnected_AlreadyStartedHosting_StatusUnchanged()
        {
            using (var game = new TestGame())
            {
                var player = game.RegisterPlayer();
                game.Disconnect(player);
                game.Tick();

                Assert.AreEqual(HostedGameStatus.StartedHosting, game.Context.Game.Status);
                Assert.AreEqual(0, game.AllPlayersDisconnectedEvents);
            }
        }

        [Test]
        public void GameStatusReset_SolvesPlayerRejectionIssue()
        {
            using (var game = new TestGame())
            {
                var player = game.RegisterPlayer();
                var handler = new Handler(game.Context);
                game.Context.Game.Status = HostedGameStatus.GameInProgress;
                Assert.IsFalse(handler.AcceptingNewPlayers,
                    "Game in progress should not accept new players");

                game.Disconnect(player);
                game.Tick();

                Assert.IsTrue(handler.AcceptingNewPlayers,
                    "After all players disconnect, game should accept new players again");
                Assert.AreEqual(HostedGameStatus.StartedHosting, game.Context.Game.Status);
                game.RegisterPlayer(); // Verify Hello accepts an actual replacement player.
                Assert.AreEqual(2, game.Players.Players.Length);
            }
        }

        [Test]
        public void NoPlayersEverAdded_GameInProgress_DoesNotResetStatus()
        {
            using (var game = new TestGame())
            {
                game.Context.Game.Status = HostedGameStatus.GameInProgress;
                Assert.AreEqual(0, game.Players.TotalPlayersAdded);

                game.Tick();

                Assert.AreEqual(HostedGameStatus.GameInProgress, game.Context.Game.Status);
                Assert.IsFalse(new Handler(game.Context).AcceptingNewPlayers);
                Assert.AreEqual(0, game.AllPlayersDisconnectedEvents);
            }
        }

        [Test]
        public void OnePlayerStillConnected_GameInProgress_DoesNotResetStatus()
        {
            using (var game = new TestGame())
            {
                var disconnected = game.RegisterPlayer();
                var connected = game.RegisterPlayer();
                game.Context.Game.Status = HostedGameStatus.GameInProgress;
                game.Disconnect(disconnected);

                game.Tick();

                Assert.IsTrue(connected.Connected);
                Assert.AreEqual(HostedGameStatus.GameInProgress, game.Context.Game.Status);
                Assert.IsFalse(new Handler(game.Context).AcceptingNewPlayers);
                Assert.AreEqual(0, game.AllPlayersDisconnectedEvents);
            }
        }

        private sealed class TestGame : IDisposable
        {
            private readonly Octgn.Server.Server _server;
            private readonly List<TcpClient> _clients = new List<TcpClient>();
            private readonly Dictionary<Player, ServerSocket> _sockets = new Dictionary<Player, ServerSocket>();

            public GameContext Context { get { return _server.Context; } }
            public PlayerCollection Players { get { return Context.State.Players; } }
            public int AllPlayersDisconnectedEvents { get; private set; }

            public TestGame()
            {
                var config = new Config { IsLocal = true, PlayerTimeoutSeconds = 3600 };
                var hostedGame = new HostedGame
                {
                    Id = Guid.NewGuid(),
                    GameId = Guid.NewGuid(),
                    GameVersion = "1.0",
                    Name = "Reset fixture",
                    HostAddress = "127.0.0.1:1",
                    Status = HostedGameStatus.StartedHosting
                };
                // Construct only: Server.Start would also start UDP broadcasting.
                _server = new Octgn.Server.Server(config, hostedGame, 21234);
                try
                {
                    StopTimer("_pingTimer");
                    StopTimer("_disconnectedPlayerTimer");
                    Players.AllPlayersDisconnected += CountAllPlayersDisconnected;
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            private void CountAllPlayersDisconnected(object sender, EventArgs e)
            {
                AllPlayersDisconnectedEvents++;
            }

            private void StopTimer(string fieldName)
            {
                var timer = (Timer)typeof(PlayerCollection)
                    .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Players);
                using (var drained = new ManualResetEvent(false))
                {
                    timer.Dispose(drained);
                    Assert.IsTrue(drained.WaitOne(TimeSpan.FromSeconds(5)),
                        "Collection timer callbacks must finish before deterministic ticks");
                }
            }

            public Player RegisterPlayer()
            {
                var listener = new TcpListener(IPAddress.Loopback, 0);
                TcpClient accepted;
                try
                {
                    listener.Start();
                    var peer = new TcpClient(AddressFamily.InterNetwork);
                    _clients.Add(peer);
                    peer.Connect((IPEndPoint)listener.LocalEndpoint);
                    accepted = listener.AcceptTcpClient();
                    _clients.Add(accepted);
                }
                finally
                {
                    listener.Stop();
                    listener.Server.Dispose();
                }

                var socket = new ServerSocket(accepted, _server);
                Players.AddClient(socket);
                var player = Players.Clients.Last();
                _sockets.Add(player, socket);
                socket.Handler.Hello("Player " + Players.TotalPlayersAdded, Guid.NewGuid().ToString(),
                    (ulong)Players.TotalPlayersAdded, "Reset fixture", Context.Config.ServerVersion,
                    Context.Config.ServerVersion, Context.Game.GameId, new Version(Context.Game.GameVersion),
                    string.Empty, false);

                Assert.IsTrue(player.SaidHello, "Hello must register the player in the context's collection");
                Assert.IsTrue(player.Connected);
                CollectionAssert.Contains(Players.Players, player);
                return player;
            }

            public void Disconnect(Player player)
            {
                PlayerDisconnectedEventArgs disconnected = null;
                EventHandler<PlayerDisconnectedEventArgs> handler = (sender, e) => disconnected = e;
                Players.PlayerDisconnected += handler;
                try
                {
                    _sockets[player].Disconnect();
                    Assert.IsNotNull(disconnected, "A real socket disconnect must notify the owned collection");
                    Assert.AreSame(player, disconnected.Player);
                    Assert.AreEqual(PlayerDisconnectedEventArgs.DisconnectedReason, disconnected.Reason);
                    Assert.IsFalse(player.Connected);
                    CollectionAssert.Contains(Players.Players, player);
                }
                finally
                {
                    Players.PlayerDisconnected -= handler;
                }
            }

            public void Tick()
            {
                typeof(PlayerCollection).GetMethod("DisconnectedPlayerTimer_Tick",
                    BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { Players });
            }

            public void Dispose()
            {
                try
                {
                    foreach (var socket in _sockets.Values)
                        socket.Disconnect();
                    foreach (var client in _clients)
                        client.Close();
                }
                finally
                {
                    try
                    {
                        Players.Dispose();
                    }
                    finally
                    {
                        try
                        {
                            _server.Dispose();
                            // Server.Dispose does not dispose its unstarted broadcaster timer.
                            ((IDisposable)typeof(Octgn.Server.Server).GetField("_broadcaster",
                                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_server)).Dispose();
                            ((TcpListener)typeof(Octgn.Server.Server).GetField("_tcp",
                                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_server)).Server.Dispose();
                        }
                        finally
                        {
                            foreach (var client in _clients)
                                client.Close();
                        }
                    }
                }
            }
        }
    }
}
