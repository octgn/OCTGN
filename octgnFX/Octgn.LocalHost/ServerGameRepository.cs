/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Octgn.Server.Data;
using Octgn.Online.Library.Models;
using Octgn.Library.Utils;
using System;

namespace Octgn.LocalHost
{
    internal class ServerGameRepository : LibraryRepositoryBase<Guid, IHostedGameState>, IGameRepository
    {
        public IPlayerRepository Players { get; }

        public ServerGameRepository() {
            Players = new ServerPlayerRepository();
        }
    }
}