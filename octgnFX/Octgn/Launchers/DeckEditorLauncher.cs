using log4net;
using Octgn.DataNew.Entities;
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
            Program.JodsEngine.LaunchDeckEditor(DeckPath);

            return Task.CompletedTask;
        }
    }
}