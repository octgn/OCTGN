using Octgn.Server.Data;
using Octgn.Online.Library.Models;
using System.Collections.Concurrent;

namespace Octgn.Hosting
{
    internal class ServerPlayerRepository : IPlayerRepository
    {
        private ConcurrentDictionary<string, IHostedGamePlayer> _players;

        private uint _currentId = 1;

        public ServerPlayerRepository() {
            _players = new ConcurrentDictionary<string, IHostedGamePlayer>();
        }

        public IHostedGamePlayer Get(string id) {
            IHostedGamePlayer p = null;
            if (!_players.TryGetValue(id, out p)) return null;
            return p;
        }

        public IHostedGamePlayer GetOrAdd(IHostedGameState game, string connectionId) {

            var ret = _players.GetOrAdd(connectionId, new HostedGamePlayer() {
                Id = _currentId++,
            });

            return ret;
        }
    }
}