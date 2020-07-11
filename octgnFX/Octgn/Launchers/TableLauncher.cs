using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public class TableLauncher : UpdatingLauncher
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

        public override Task BeforeUpdate() {
            return Task.CompletedTask;
        }

        public override Task AfterUpdate() {
            try {
                Program.JodsEngine.HostGame(hostPort, gameId);
            } catch (Exception e) {
                this.Log.Warn("Couldn't host/join table mode", e);
                this.Shutdown = true;
                Program.Exit();
            }

            return Task.CompletedTask;
        }
    }
}