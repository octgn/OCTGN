namespace Octgn.Online.Library.SignalR.Coms
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Octgn.Online.Library.SignalR.Coms.Models;

    public interface IGameServiceToSASManagerService
    {
        Task StartGame(Guid id, string name, string oGameName, Guid oGameId);

        Task<IEnumerable<HostedGame>> GameList();

        Task Hello(string mess1, string mess2);
    }
}