using Octgn.Online.Library.Models;
using System;

namespace Octgn.Server.Data
{
    public interface IGameRepository
    {
        IPlayerRepository Players { get; }
        IHostedGameState Checkout(int id);
        void Checkin(IHostedGameState game);
    }
}
