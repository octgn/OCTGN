/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Skylabs.Lobby.Messages.Matchmaking
{
    public class StartMatchmakingMessage : GenericMessage
    {
        public Guid GameId
        {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(this.GetTag("GameId"), out ret);
                return ret;
            }
            set { this.SetTag("GameId", value.ToString()); }
        }
        public string GameName
        {
            get
            {
                return this.GetTag("GameName");
            }
            set
            {
                this.SetTag("GameName", value);
            }
        }
        public string GameMode
        {
            get
            {
                return this.GetTag("GameMode");
            }
            set
            {
                this.SetTag("GameMode", value);
            }
        }
        public int MaxPlayers
        {
            get
            {
                return this.GetTagInt("MaxPlayers");
            }
            set
            {
                this.SetTag("MaxPlayers", value);
            }
        }

        public StartMatchmakingMessage()
        {
            this.Attributes.Add("MessageName", this.GetType().Name);
            Type = MessageType.normal;
            this.To = new Jid("matchmaking@of.octgn.net");
            this.Subject = this.GetType().Name;
            this.Body = "";
			
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", GameId, GameName, GameMode, MaxPlayers);
        }
    }
}