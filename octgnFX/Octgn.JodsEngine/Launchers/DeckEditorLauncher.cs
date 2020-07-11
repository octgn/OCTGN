using System.Windows;
using log4net;
using Octgn.Controls;
using Octgn.DataNew.Entities;
using Octgn.DeckBuilder;
using Octgn.Library.Exceptions;
using System.Threading.Tasks;
using System.Reflection;

namespace Octgn.Launchers
{

    public class DeckEditorLauncher : ILauncher
    {
        internal string DeckPath;
        internal IDeck Deck;

        public DeckEditorLauncher(string deckPath = null) {
            // This way Deck == null instead of an empty string
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            this.DeckPath = string.IsNullOrWhiteSpace(deckPath) ? null : deckPath;
        }

        public ILog Log { get; private set; }
        public bool Shutdown { get; private set; }

        public Task Launch() {
            try {
                Deck = (DeckPath == null) ? null : new MetaDeck(DeckPath);
                var win = new DeckBuilderWindow(Deck, true);
                Application.Current.MainWindow = win;
                win.Show();
            } catch (UserMessageException e) {
                TopMostMessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                this.Shutdown = true;
            }
            return Task.CompletedTask;
        }
    }
}