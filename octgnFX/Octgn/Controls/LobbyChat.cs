/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octgn.Controls
{
    using Skylabs.Lobby;

    /// <summary>
    ///     Control specifically for the lobby chat room.
    /// </summary>
    public class LobbyChat : ChatControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LobbyChat"/> class.
        /// </summary>
        public LobbyChat()
        {
            Program.LobbyClient.Chatting.OnCreateRoom += this.Chatting_OnCreateRoom;
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
    }
}
