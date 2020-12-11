using NUnit.Framework;
using Octgn.Communication;
using Octgn.Library.Networking;
using Octgn.Online.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octgn.Test.Library.Networking
{
    [TestFixture]
    public class GameBroadcastingTests
    {
        [TestCase]
        public async Task Test() {
            var game = new HostedGame() {
                Id = Guid.NewGuid(),
                HostUser = new User("id", "name"),
                HostAddress = "0.0.0.0:5000",   // needed to avoid a parsing error
            };
            using(var broadcaster = new GameBroadcaster(game, 3456)) {
                using(var listener = new GameBroadcastListener(3456)) {
                    broadcaster.StartBroadcasting();
                    listener.StartListening();

                    var waitedGame = await listener.WaitForGame(game.Id);

                    Assert.AreEqual(game.HostUser.Id, waitedGame.HostUser.Id);
                    Assert.AreEqual(game.HostUser.DisplayName, waitedGame.HostUser.DisplayName);
                }
            }
        }
    }
}
