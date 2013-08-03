namespace Octgn.Test
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using NUnit.Framework;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Play.State;

    public class PlayGround
    {
        [Test]
        public void Spaces()
        {

            var g = Octgn.DataNew.DbContext
                .Get()
                .Games
                .First(x => x.Id == new Guid("844d5fe3-bdb5-4ad2-ba83-88c2c2db6d88"));
            Program.GameEngine = new GameEngine(g,"asdf");
            Program.GameEngine.StopTurn = true;

            var testSet = Octgn.DataNew.DbContext.Get().Sets.First(x => x.GameId == g.Id);

            var testCard = Octgn.DataNew.DbContext.Get().Cards.First(x => x.SetId == testSet.Id);

            Program.GameEngine.RecentCards.Add(testCard.ToMultiCard());

            Stopwatch sw = new Stopwatch();

            var oldSave = GameSave.Create();
            var oldSaveBytes = GameSave.Serialize(oldSave);

            var newSave = GameSave.Deserialize(oldSaveBytes);

            var newGe = newSave.Get<GameEngine>();

            Assert.AreEqual(Program.GameEngine.RecentCards.First().Id,newGe.RecentCards.First().Id);

            Assert.IsTrue(newGe.StopTurn);

            Assert.AreEqual(Program.GameEngine.Markers.First().Id,newGe.Markers.First().Id);
        }
    }
}