/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Octgn.Core;

namespace Octgn.Controls
{
    using Skylabs.Lobby;

    /// <summary>
    ///     Control specifically for the lobby chat room.
    /// </summary>
    public class LobbyChat : ChatControl
    {
        private readonly Timer _adminTimer;
        /// <summary>
        /// Initializes a new instance of the <see cref="LobbyChat"/> class.
        /// </summary>
        public LobbyChat()
        {
            Program.LobbyClient.Chatting.OnCreateRoom += this.Chatting_OnCreateRoom;
            _adminTimer = new Timer(UpdateIsAdmin, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// When a Lobby creates a chat room.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="createdroom">
        /// The created room.
        /// </param>
        private void Chatting_OnCreateRoom(object sender, ChatRoom createdroom)
        {
            if (createdroom.GroupUser == null || createdroom.GroupUser.UserName != "lobby" || this.Room != null)
            {
                return;
            }

            this.SetRoom(createdroom);
        }

        private void UpdateIsAdmin(object state)
        {
            if (this.Room != null && Program.LobbyClient != null
                    && Program.LobbyClient.Me != null
                    && Program.LobbyClient.Me.ApiUser != null
                    && Program.LobbyClient.IsConnected)
            {
                var allowed = new List<User>();
                allowed.AddRange(this.Room.AdminList);
                allowed.AddRange(this.Room.ModeratorList);
                allowed.AddRange(this.Room.OwnerList);
                if (allowed.Any(x => x.ApiUser != null
                    && x.ApiUser.Id == Program.LobbyClient.Me.ApiUser.Id))
                {
                    Prefs.IsAdmin = true;
                }
                else
                {
                    Prefs.IsAdmin = false;
                }
            }
        }

        public new void Dispose()
        {
            _adminTimer.Dispose();
            base.Dispose();
        }
    }
}
