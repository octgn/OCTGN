using System.Windows;
using Octgn.DataNew.Entities;
using Octgn.DeckBuilder;
using System.Threading.Tasks;
using Octgn.Windows;
using System.IO;
using System;

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

        protected override async Task<Window> Load(ILoadingView loadingView) {
            try {
                var fn = Path.GetFileName(DeckPath);
                loadingView.UpdateStatus($"Loading Deck '{fn}'...");

                await Task.Run(() => {
                    Deck = (DeckPath == null) ? null : new MetaDeck(DeckPath);
                });

                var win = new DeckBuilderWindow(Deck, true);

                win.Show();

                return win;
            } catch (Exception e) {

                string msg;
                if (string.IsNullOrWhiteSpace(DeckPath)) {
                    msg = $"Deck editor failed to launch: {e.Message}";
                } else {
                    msg = $"Deck editor failed to launch '{DeckPath}': {e.Message}";
                }

                Log.Warn(msg, e);

                MessageBox.Show(
                    msg,
                    "Deck Editor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                this.Shutdown = true;
            }

            return null;
        }
    }
}