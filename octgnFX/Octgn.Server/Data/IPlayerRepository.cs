using Octgn.Online.Library.Models;
using System;

namespace Octgn.Server.Data
{
    public interface IPlayerRepository
    {
        IHostedGamePlayer Get(ulong id);
        IHostedGamePlayer GetOrAdd(IHostedGameState game, string connectionId, string username);
    }
}
