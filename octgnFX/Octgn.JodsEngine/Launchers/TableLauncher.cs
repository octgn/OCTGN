using System.Windows;
using System;
using System.Threading.Tasks;
using Octgn.Windows;
using Octgn.Library.Exceptions;

namespace Octgn.Launchers
{
    public class TableLauncher : LauncherBase
    {
        public override string Name => "Game Table";

        private readonly int? hostPort;
        private readonly Guid? gameId;

        public TableLauncher(int? hostport, Guid? gameid) {
            hostPort = hostport;
            gameId = gameid;
        }

        protected override async Task<Window> Load(ILoadingView loadingView) {
            try {
                await new GameTableLauncher().Launch(this.hostPort, this.gameId);

                return WindowManager.PlayWindow;
            } catch (Exception e) {
                var msg = "Couldn't host local game";

                Log.Warn(msg, e);

                throw new UserMessageException(UserMessageExceptionMode.Blocking, msg, e);
            }
        }
    }
}