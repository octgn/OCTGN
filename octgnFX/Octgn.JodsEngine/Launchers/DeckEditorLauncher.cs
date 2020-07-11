using System.Windows;
using Octgn.DataNew.Entities;
using Octgn.DeckBuilder;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public class DeckEditorLauncher : LauncherBase
    {
        public string DeckPath { get; }
        public IDeck Deck { get; private set; }

        public DeckEditorLauncher(string deckPath = null) {
            // This way Deck == null instead of an empty string
            DeckPath = string.IsNullOrWhiteSpace(deckPath) ? null : deckPath;
        }

        protected override Task Load() {
            Deck = (DeckPath == null) ? null : new MetaDeck(DeckPath);

            return Task.CompletedTask;
        }

        protected override Task Loaded() {
            var win = new DeckBuilderWindow(Deck, true);
            Application.Current.MainWindow = win;
            win.Show();

            return Task.CompletedTask;
        }
    }
}