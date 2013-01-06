using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.DeckBuilderPluginExample
{
    using NUnit.Framework;

    using Octgn.Data;
    using Octgn.Library.Plugin;

    [TestFixture]
    public class DeckBuilderPluginExampleTests
    {
        [Test]
        public void ConstructDeckBuilderPluginExample()
        {
            var plug = new DeckBuilderPluginExample();
            Assert.AreEqual(1,plug.MenuItems.Count());
            var first = plug.MenuItems.First();
            Assert.AreEqual("Random 1 Card Deck",first.Name);
            var controller = new DeckBuilderPluginController();
            Assert.Null(controller.LoadedDeck);
            first.OnClick(controller);
            Assert.NotNull(controller.LoadedDeck);
            Assert.AreEqual(1,controller.LoadedDeck.CardCount);
        }
    }

    public class DeckBuilderPluginController:IDeckBuilderPluginController
    {
        public GamesRepository Games
        {
            get
            {
                return games;
            }
        }

        private readonly GamesRepository games = new GamesRepository();

        public Game LoadedGame { get; private set; }

        public Deck LoadedDeck { get; private set; }

        public void SetLoadedGame(Game game)
        {
            this.LoadedGame = game;
        }

        public Game GetLoadedGame()
        {
            return this.LoadedGame;
        }

        public void LoadDeck(Deck deck)
        {
            LoadedDeck = deck;
        }
    }
}
