namespace Octgn.Online.SASManagerService.Coms
{
    using System;

    using Octgn.Online.Library.SignalR.Coms;

    public class GameServiceToSASManagerService : IGameServiceToSASManagerService
    {
        public void StartGame(Guid id, string name, string oGameName, Guid oGameId)
        {
            
        }

        public void Hello(string mess1, string mess2)
        {
            Console.WriteLine("{0} {1}",mess1,mess2);
        }
    }
}