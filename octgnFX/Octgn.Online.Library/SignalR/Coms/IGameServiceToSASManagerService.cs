namespace Octgn.Online.Library.SignalR.Coms
{
    using System;
    using System.Threading.Tasks;

    public interface IGameServiceToSASManagerService
    {
        Task StartGame(Guid id, string name, string oGameName, Guid oGameId);

        Task Hello(string mess1, string mess2);
    }
}