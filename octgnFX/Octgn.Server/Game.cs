/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using Octgn.Online.Library.Enums;
using Octgn.Online.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octgn.Server
{
    public class Game : IHostedGameState
    {
        #region IHostedGameState
        public int Id { get; set; }

        public string Name { get; set; }

        public string HostUserName { get; set; }

        public string GameName { get; set; }

        public Guid GameId { get; set; }

        public Version GameVersion { get; set; }

        public bool HasPassword { get; set; }

        public bool TwoSidedTable { get; set; }

        public Uri HostUri { get; set; }

        public string Password { get; set; }

        public EnumHostedGameStatus Status { get; set; }

        public IList<IHostedGamePlayer> Players { get; set; }

        public int CurrentTurnPlayer { get; set; }
        public int CurrentTurnNumber { get; set; }

        public bool Spectators { get; set; }

        public string GameIconUrl { get; set; }

        public string HostUserIconUrl { get; set; }
        public bool AcceptingPlayers { get; set; }
        public HashSet<uint> TurnStopPlayers { get; set; }
        public HashSet<Tuple<uint, byte>> PhaseStopPlayers { get; set; }

        public IList<IHostedGamePlayer> KickedPlayers { get; set; }

        public IList<IHostedGamePlayer> DisconnectedPlayers { get; set; }

        public bool MuteSpectators { get; set; }

        public bool HideBoard { get; set; }
        #endregion IHostedGameState

        public Game(IHostedGameState game) {
            AcceptingPlayers = game.AcceptingPlayers;
            CurrentTurnNumber = game.CurrentTurnNumber;
            CurrentTurnPlayer = game.CurrentTurnPlayer;
            DisconnectedPlayers = game.DisconnectedPlayers;
            GameIconUrl = game.GameIconUrl;
            GameId = game.GameId;
            GameName = game.GameName;
            GameVersion = game.GameVersion;
            HasPassword = game.HasPassword;
            HideBoard = game.HideBoard;
            HostUri = game.HostUri;
            HostUserIconUrl = game.HostUserIconUrl;
            HostUserName = game.HostUserName;
            Id = game.Id;
            KickedPlayers = game.KickedPlayers;
            MuteSpectators = game.MuteSpectators;
            Name = game.Name;
            Password = game.Password;
            PhaseStopPlayers = game.PhaseStopPlayers;
            Players = game.Players;
            Spectators = game.Spectators;
            Status = game.Status;
            TurnStopPlayers = game.TurnStopPlayers;
            TwoSidedTable = game.TwoSidedTable;
        }

        public Player GetPlayer(uint fp) {
            return (Player)Players.FirstOrDefault(x => x.Id == fp);
        }

        public void KickPlayer(uint p, string reason) {
            var player = GetPlayer(p);
            if (player == null) return;
            Players.Remove(player);
            KickedPlayers.Add(player);
            ((Player)player).Kick(false, reason);
        }

        public void PlayerDisconnected(uint p) {
            var player = GetPlayer(p);
            if (player == null) return;
            Players.Remove(player);
            DisconnectedPlayers.Add(player);
        }

        public void PlayerReconnected(uint p) {
            var player = DisconnectedPlayers.FirstOrDefault(x=>x.Id == p);
            if (player == null) return;
            DisconnectedPlayers.Remove(player);
            Players.Add(player);
        }
    }
}
