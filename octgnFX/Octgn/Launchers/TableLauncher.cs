using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Octgn.Launchers
{
    using System;

    public class TableLauncher : UpdatingLauncher
    {
        private readonly int? hostPort;
        private readonly Guid? gameId;

        public TableLauncher(int? hostport, Guid? gameid)
        {
            this.hostPort = hostport;
            this.gameId = gameid;
            if (this.gameId == null)
            {
                MessageBox.Show("You must supply a GameId with -g=GUID on the command line.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown = true;
            }
        }

        public override void BeforeUpdate()
        {
            
        }

        public override void AfterUpdate()
        {
            try
            {
                new GameTableLauncher().Launch(this.hostPort, this.gameId);
            }
            catch (Exception e)
            {
                this.Log.Warn("Couldn't host/join table mode", e);
                this.Shutdown = true;
                Program.Exit();
            }
        }
    }
}