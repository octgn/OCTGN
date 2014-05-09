/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Skylabs.Lobby.Messages.Matchmaking
{
    public class StartMatchmakingRequest : GenericMessage
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
        public Version GameVersion
        {
            get
            {
                var ret = new Version();
                Version.TryParse(this.GetTag("GameVersion"), out ret);
                return ret;
            }
            set { this.SetTag("GameVersion", value.ToString()); }
        }
        public Version OctgnVersion
        {
            get
            {
                var ret = new Version();
                Version.TryParse(this.GetTag("OctgnVersion"), out ret);
                return ret;
            }
            set { this.SetTag("OctgnVersion", value.ToString()); }
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

        public StartMatchmakingRequest()
        {
            Type = MessageType.normal;
            this.To = new Jid("matchmaking@of.octgn.net");
            this.Subject = this.GetType().Name;
            this.Body = "";
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4} {5}", GameId, GameVersion, OctgnVersion, GameName, GameMode, MaxPlayers);
        }
    }

    public class StartMatchmakingResponse : MatchmakingMessage
    {
        public StartMatchmakingResponse()
        {
            
        }

        public StartMatchmakingResponse(Jid to, Guid queueId):base(to,queueId)
        {
        }
    }

    public class MatchmakingInLineUpdateMessage : MatchmakingMessage
    {
        public TimeSpan AverageWaitTime
        {
            get
            {
                var ret = TimeSpan.Zero;
                TimeSpan.TryParse(this.GetTag("AverageWaitTime"), out ret);
                return ret;
            }
            set { this.SetTag("AverageWaitTime", value.ToString()); }
        }

        public MatchmakingInLineUpdateMessage()
        {
            
        }
        public MatchmakingInLineUpdateMessage(TimeSpan avgWait, Jid to, Guid queueId):base(to,queueId)
        {
            AverageWaitTime = avgWait;
        }
    }

    public abstract class MatchmakingMessage : GenericMessage
    {
        public Guid QueueId
        {
            get
            {
                Guid ret = Guid.Empty;
                Guid.TryParse(this.GetTag("QueueId"), out ret);
                return ret;
            }
            set { this.SetTag("QueueId", value.ToString()); }
        }

        protected MatchmakingMessage()
        {
            
        }

        protected MatchmakingMessage(Jid to, Guid queueId)
        {
            Type = MessageType.normal;
            this.To = to;
            this.Subject = this.GetType().Name;
            this.Body = "";
            QueueId = queueId;
        }
    }
}