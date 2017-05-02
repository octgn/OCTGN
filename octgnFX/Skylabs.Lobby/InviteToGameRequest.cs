/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
namespace Skylabs.Lobby
{
    public class InviteToGameRequest
    {
        public InviteToGameRequest()
        {
        }

        public InviteToGameRequest(Guid sessionId, string password)
        {
            this.SessionId = sessionId;
            this.Password = password;
        }

        public Guid SessionId { get; set; }

        public string Password { get; set; }
    }
}