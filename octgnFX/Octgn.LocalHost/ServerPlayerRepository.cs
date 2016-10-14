/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Octgn.Server.Data;
using Octgn.Online.Library.Models;
using System.Collections.Concurrent;
using System;

namespace Octgn.LocalHost
{
    internal class ServerPlayerRepository : IPlayerRepository
    {
        private ConcurrentDictionary<string, IHostedGamePlayer> _players;

        private uint _currentId = 1;

        public ServerPlayerRepository() {
            _players = new ConcurrentDictionary<string, IHostedGamePlayer>();
        }

        public IHostedGamePlayer Get( string id ) {
            IHostedGamePlayer p = null;
            if( !_players.TryGetValue( id, out p ) ) return null;
            return p;
        }

        public IHostedGamePlayer GetOrAdd( IHostedGameState game, string connectionId ) {

            var ret = _players.GetOrAdd( connectionId, new HostedGamePlayer() {
                Id = Guid.NewGuid()
            } );

            return ret;
        }
    }
}