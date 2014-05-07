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
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string GameMode { get; set; }
        public int MaxPlayers { get; set; }

        public StartMatchmakingMessage()
        {
            this.Attributes.Add("MessageName", this.GetType().Name);
            Type = MessageType.normal;
            this.To = new Jid("matchmaking@of.octgn.net");
            this.Subject = this.GetType().Name;
            this.Body = "";
        }

        protected override void Read(Message m)
        {
            base.Read(m);
            this.GameId = Guid.Empty;
            Guid gameId;
            if (Guid.TryParse(m.GetTag("GameId"), out gameId))
            {
                GameId = gameId;
            }
            this.GameName = m.GetTag("GameName");
            this.GameMode = m.GetTag("GameMode");
            this.MaxPlayers = m.GetTagInt("MaxPlayers");
        }

        public override void Write()
        {
            base.Write();
            this.SetTag("GameId", GameId.ToString());
            this.SetTag("GameName",GameName);
            this.SetTag("GameMode",GameMode);
            this.SetTag("MaxPlayers",MaxPlayers);
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", GameId, GameName, GameMode, MaxPlayers);
        }
    }
}