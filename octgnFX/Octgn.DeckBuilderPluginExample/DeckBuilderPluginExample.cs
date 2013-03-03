namespace Octgn.DeckBuilderPluginExample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using Octgn.Data;
    using Octgn.Library.Plugin;

    public class DeckBuilderPluginExample : IDeckBuilderPlugin 
    {
        public IEnumerable<IPluginMenuItem> MenuItems
        {
            get
            {
                // Add your menu items here.
                return new List<IPluginMenuItem>{new PluginMenuItem()};
            }
        }

        public void OnLoad(GamesRepository games)
        {
            // I'm showing a message box, but don't do this, unless it's for updates or something...but don't do it every time as it pisses people off.
            MessageBox.Show("Hello!");
        }

        public Guid Id
        {
            get
            {
                // All plugins are required to have a unique GUID
                // http://www.guidgenerator.com/online-guid-generator.aspx
                return Guid.Parse("d94bc436-d46e-4968-914b-92ddbde11c3c");
            }
        }

        public string Name
        {
            get
            {
                // Display name of the plugin.
                return "Deck Builder Plugin Example";
            }
        }

        public Version Version
        {
            get
            {
                // Version of the plugin.
                // This code will pull the version from the assembly.
                return Assembly.GetCallingAssembly().GetName().Version;
            }
        }

        public Version RequiredByOctgnVersion
        {
            get
            {
                // Don't allow this plugin to be used in any version less than 3.0.7.51
                return Version.Parse("3.0.7.51");
            }
        }
    }

    public class PluginMenuItem : IPluginMenuItem
    {
        public string Name
        {
            get
            {
                return "Random 1 Card Deck";
            }
        }

        /// <summary>
        /// This happens when the menu item is clicked.
        /// </summary>
        /// <param name="con"></param>
        public void OnClick(IDeckBuilderPluginController con)
        {
            var curDeck = con.GetLoadedDeck();

            if(curDeck != null)
                MessageBox.Show(String.Format("{0}",curDeck.CardCount));

            // Find the first game with cards in it.
            var game = con.Games.Games.FirstOrDefault(x => x.SelectCards(null).Rows.Count > 0);
            if (game == null)
            {
                MessageBox.Show("No Games Installed?!?!?");
                return;
            }
            // Before we make a deck, we need to make sure we load the correct game for the deck.
            con.SetLoadedGame(game);

            // Select a random card from the games card list.
            var cm = game.SelectRandomCardModels(1).FirstOrDefault();
            var d = new Deck(game);

            // It's weird, but this is how you add a card.
            d.Sections[0].Cards.Add(new Deck.Element()
                                        {
                                            Card = cm,
                                            Quantity = (byte)1
                                        });

            // Load that mother.
            con.LoadDeck(d);
        }
    }
}
