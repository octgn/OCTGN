namespace Octgn.Online.SASManagerService.Coms
{
    using System;
    using System.Threading.Tasks;

    using Octgn.Online.Library.SignalR.Coms;

    public class GameServiceToSASManagerService : IGameServiceToSASManagerService
    {
        public Task StartGame(Guid id, string name, string oGameName, Guid oGameId)
        {
            return new Task(() => { });
        }

        public Task Hello(string mess1, string mess2)
        {
            var ret = new Task(()=> Console.WriteLine("{0} {1}",mess1,mess2));
            ret.Start();
            return ret;
        }
    }
}