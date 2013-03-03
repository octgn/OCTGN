﻿namespace Octgn.Online.Library.Coms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Octgn.Online.Library.Models;

    public interface IGameServiceToSASManagerService
    {
        Task StartGame(HostedGameSASRequest request);

        Task<IEnumerable<HostedGame>> GameList();

        Task Hello(string mess1, string mess2);
    }
}