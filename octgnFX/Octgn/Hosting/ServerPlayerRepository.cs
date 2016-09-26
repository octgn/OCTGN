using Octgn.Server.Data;
using Octgn.Online.Library.Models;
using System.Collections.Concurrent;

namespace Octgn.Hosting
{
    internal class ServerPlayerRepository : IPlayerRepository
    {
        private ConcurrentDictionary<uint, IHostedGamePlayer> _players;

        private uint _currentId;

        public ServerPlayerRepository() {
            _players = new ConcurrentDictionary<uint, IHostedGamePlayer>();
        }

        public IHostedGamePlayer Get(uint id) {
            IHostedGamePlayer p = null;
            if (!_players.TryGetValue(id, out p)) return null;
            return p;
        }

        public IHostedGamePlayer GetOrAdd(IHostedGameState game, string connectionId, string username) {
            var id = uint.Parse(username);

            var ret = _players.GetOrAdd(id, new HostedGamePlayer() {
                Id = _currentId++,
            });

            return ret;
        }
    }
}