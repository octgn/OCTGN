using Octgn.Server.Data;
using Octgn.Online.Library.Models;
using Octgn.Library.Utils;
using System;

namespace Octgn.Hosting
{
    internal class ServerGameRepository : LibraryRepositoryBase<Guid, IHostedGameState>, IGameRepository
    {
        public IPlayerRepository Players { get; }

        public ServerGameRepository() {
            Players = new ServerPlayerRepository();
        }
    }
}