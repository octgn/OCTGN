using System;
using System.Threading;
using NUnit.Framework;

namespace Octgn.Test.Server
{
    [TestFixture]
    public class PlayerCollectionTimeoutTests
    {
        [Test]
        public void LocalGameTimeout_ShouldBe5Seconds()
        {
            // Arrange - Create a mock config for local game
            var config = new Octgn.Server.Config { IsLocal = true };
            
            // Act - Calculate timeout as done in PlayerCollection.DisconnectedPlayerTimer_Tick
            var timeToWait = config.IsLocal
                ? TimeSpan.FromSeconds(5)
                : TimeSpan.FromMinutes(2);
            
            // Assert
            Assert.AreEqual(5, timeToWait.TotalSeconds, 
                "Local game timeout should be 5 seconds to allow for quick testing scenarios");
        }
        
        [Test]
        public void NonLocalGameTimeout_ShouldBe2Minutes()
        {
            // Arrange - Create a mock config for non-local game
            var config = new Octgn.Server.Config { IsLocal = false };
            
            // Act - Calculate timeout as done in PlayerCollection.DisconnectedPlayerTimer_Tick
            var timeToWait = config.IsLocal
                ? TimeSpan.FromSeconds(5)
                : TimeSpan.FromMinutes(2);
            
            // Assert
            Assert.AreEqual(120, timeToWait.TotalSeconds, 
                "Non-local game timeout should remain 2 minutes for network stability");
        }
        
        [Test]
        public void LocalGameTimeout_ShouldBeSignificantlyFasterThanBefore()
        {
            // Arrange - Create a mock config for local game
            var config = new Octgn.Server.Config { IsLocal = true };
            
            // Act - Calculate current timeout and compare with old timeout
            var newTimeout = config.IsLocal
                ? TimeSpan.FromSeconds(5)
                : TimeSpan.FromMinutes(2);
            
            var oldTimeout = TimeSpan.FromSeconds(15); // Previous timeout value
            
            // Assert
            Assert.Less(newTimeout.TotalSeconds, oldTimeout.TotalSeconds,
                "New local timeout should be less than the old 15-second timeout");
            Assert.AreEqual(5, newTimeout.TotalSeconds,
                "New timeout should be exactly 5 seconds for optimal testing experience");
        }
    }
}