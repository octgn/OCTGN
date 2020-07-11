using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public class TableLauncher : LauncherBase
    {
        private readonly int? hostPort;
        private readonly Guid? gameId;

        public TableLauncher(int? hostport, Guid? gameid) {
            this.hostPort = hostport;
            this.gameId = gameid;
            if (this.gameId == null) {
                MessageBox.Show("You must supply a GameId with -g=GUID on the command line.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown = true;
            }
        }

        protected override Task Load() {
            return Task.CompletedTask;
        }

        protected override async Task Loaded() {
            try {
                await new GameTableLauncher().Launch(this.hostPort, this.gameId);
            } catch (Exception e) {
                this.Log.Warn("Couldn't host/join table mode", e);
                this.Shutdown = true;
                Program.Exit();
            }
        }
    }
}