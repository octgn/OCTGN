namespace Octgn.Online.SASManagerService.Coms
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Octgn.Online.Library.SignalR.Coms;
    using Octgn.Online.Library.SignalR.Coms.Models;

    public class GameServiceToSASManagerService : IGameServiceToSASManagerService
    {
        public Task StartGame(Guid id, string name, string oGameName, Guid oGameId)
        {
            return new Task(() => { });
        }

        public Task<IEnumerable<HostedGame>> GameList()
        {
            var ret = new Task<IEnumerable<HostedGame>>(() => { return new HostedGame[0];});
            ret.Start();
            return ret;
        }

        public Task Hello(string mess1, string mess2)
        {
            var ret = new Task(()=> Console.WriteLine("{0} {1}",mess1,mess2));
            ret.Start();
            return ret;
        }
    }
}