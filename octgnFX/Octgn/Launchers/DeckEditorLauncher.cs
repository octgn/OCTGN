namespace Octgn.Launchers
{
    using System.Reflection;
    using System.Windows;

    using log4net;

    using Octgn.Controls;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;
    using Octgn.DeckBuilder;
    using Octgn.Library.Exceptions;

    public class DeckEditorLauncher : ILauncher
    {
        internal string DeckPath;
        internal IDeck Deck;

        public DeckEditorLauncher(string deckPath = null)
        {
			// This way Deck == null instead of an empty string
			Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            
            this.DeckPath = string.IsNullOrWhiteSpace(deckPath) ? null : deckPath;
        }

        //public override void BeforeUpdate()
        //{
        //    if (DeckPath == null) return;
        //    try
        //    {
        //        Deck = new MetaDeck(DeckPath);
        //    }
        //    catch (UserMessageException e)
        //    {
        //        TopMostMessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        //        this.Shutdown = true;
        //    }
        //}

        //public override void AfterUpdate()
        //{
        //    var win = new DeckBuilderWindow(Deck,true);
        //    Application.Current.MainWindow = win;
        //    win.Show();
        //}

        public ILog Log { get; private set; }
        public bool Shutdown { get; private set; }

        public void Launch()
        {
            if (DeckPath == null) return;
            try
            {
                Deck = new MetaDeck(DeckPath);
                var win = new DeckBuilderWindow(Deck,true);
                Application.Current.MainWindow = win;
                win.Show();
            }
            catch (UserMessageException e)
            {
                TopMostMessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                this.Shutdown = true;
            }
        }
    }
}