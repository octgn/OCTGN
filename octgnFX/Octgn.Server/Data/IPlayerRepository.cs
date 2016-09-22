/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
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
