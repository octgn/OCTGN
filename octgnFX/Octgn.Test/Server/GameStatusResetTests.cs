using System;
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
            // Arrange
            var config = new Config { IsLocal = true };
            var hostedGame = new HostedGame
            {
                Id = Guid.NewGuid(),
                Status = HostedGameStatus.GameInProgress
            };
            var playerCollection = new PlayerCollection(new GameContext(hostedGame, config));

            // Verify game is initially in progress
            Assert.AreEqual(HostedGameStatus.GameInProgress, hostedGame.Status);

            // Act - Simulate the disconnected player timer tick when no players are connected
            // Since the DisconnectedPlayerTimer_Tick method is private, we need to trigger it indirectly
            // by waiting for the timer to fire. The timer runs every 2 seconds.
            
            // Wait up to 5 seconds for the timer to detect all players disconnected and reset status
            var timeout = DateTime.Now.AddSeconds(5);
            while (hostedGame.Status == HostedGameStatus.GameInProgress && DateTime.Now < timeout)
            {
                Thread.Sleep(100);
            }

            // Assert
            Assert.AreEqual(HostedGameStatus.StartedHosting, hostedGame.Status, 
                "Game status should be reset to StartedHosting when all players disconnect");
        }

        [Test]
        public void AllPlayersDisconnected_AlreadyStartedHosting_StatusUnchanged()
        {
            // Arrange
            var config = new Config { IsLocal = true };
            var hostedGame = new HostedGame
            {
                Id = Guid.NewGuid(),
                Status = HostedGameStatus.StartedHosting
            };
            var playerCollection = new PlayerCollection(new GameContext(hostedGame, config));

            // Act - Wait for timer tick
            Thread.Sleep(3000);

            // Assert - Status should remain unchanged
            Assert.AreEqual(HostedGameStatus.StartedHosting, hostedGame.Status);
        }

        [Test]
        public void GameStatusReset_SolvesPlayerRejectionIssue()
        {
            // This test simulates the real-world scenario described in the issue
            
            // Arrange - Game starts and becomes "in progress"
            var config = new Config { IsLocal = true };
            var hostedGame = new HostedGame
            {
                Id = Guid.NewGuid(),
                Status = HostedGameStatus.GameInProgress
            };
            var context = new GameContext(hostedGame, config);
            var handler = new Handler(context);

            // Initially, new players should be rejected
            Assert.IsFalse(handler.AcceptingNewPlayers, "Game in progress should not accept new players");

            // Act - All players disconnect (simulated by creating PlayerCollection which triggers timer)
            var playerCollection = new PlayerCollection(context);
            
            // Wait for the timer to reset the game status
            var timeout = DateTime.Now.AddSeconds(5);
            while (!handler.AcceptingNewPlayers && DateTime.Now < timeout)
            {
                Thread.Sleep(100);
            }

            // Assert - New players should now be accepted
            Assert.IsTrue(handler.AcceptingNewPlayers, 
                "After all players disconnect, game should accept new players again");
            Assert.AreEqual(HostedGameStatus.StartedHosting, hostedGame.Status);
        }
    }
}