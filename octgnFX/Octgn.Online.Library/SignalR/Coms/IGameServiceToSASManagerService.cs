namespace Octgn.Online.Library.SignalR.Coms
{
    using System;

    public interface IGameServiceToSASManagerService
    {
        void StartGame(Guid id, string name, string oGameName, Guid oGameId);

        void Hello(string mess1, string mess2);
    }
}