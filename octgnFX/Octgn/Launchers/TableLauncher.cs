namespace Octgn.Launchers
{
    using System;

    using Octgn.Launcher;

    public class TableLauncher : UpdatingLauncher
    {
        private readonly int? hostPort;
        private readonly Guid? gameId;

        public TableLauncher(int? hostport, Guid? gameid)
        {
            this.hostPort = hostport;
            this.gameId = gameid;
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