namespace Octgn.Launchers
{
    using System.Windows;

    using Octgn.Controls;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;
    using Octgn.DeckBuilder;
    using Octgn.Library.Exceptions;

    public class DeckEditorLauncher : UpdatingLauncher
    {
        internal string DeckPath;
        internal IDeck Deck;

        public DeckEditorLauncher(string deckPath = null)
        {
			// This way Deck == null instead of an empty string
            this.DeckPath = string.IsNullOrWhiteSpace(deckPath) ? null : deckPath;
        }

        public override void BeforeUpdate()
        {
            if (DeckPath == null) return;
            try
            {
                Deck = Deck.Load(DeckPath);
            }
            catch (UserMessageException e)
            {
                TopMostMessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                this.Shutdown = true;
            }
        }

        public override void AfterUpdate()
        {
            var win = new DeckBuilderWindow(Deck,true);
            Application.Current.MainWindow = win;
            win.Show();
        }
    }
}