/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.Play.Save;
using Octgn.DataNew.Entities;

namespace Octgn.Play.State
{
    public class JodsEngineGameSaveState : SaveState<GameEngine, JodsEngineGameSaveState>, IGameSaveState
    {
        [JsonIgnore]
        IPlayerSaveState[] IGameSaveState.Players => Players;

        public JodsEnginePlayerSaveState[] Players { get; set; }

        public Dictionary<string, string> GlobalVariables { get; set; }
        public int TurnNumber { get; set; }
        public byte ActivePlayer { get; set; }
        public bool StopTurn { get; set; }
        public string GameBoard { get; set; }
        public ushort CurrentUniqueId { get; set; }
        public Guid SessionId { get; set; }
        public GroupSaveState Table { get; set; }

        public override JodsEngineGameSaveState Create(GameEngine tp, Play.Player fromPlayer) {
            return Create(tp, fromPlayer, false);
        }

        public JodsEngineGameSaveState Create(GameEngine engine, Play.Player fromPlayer, bool includeFromPlayer)
        {
            // Still need to a way to send users their state back
            //  I'm thinking that the CardIdentity.Key could be
            //  encrypted with Crypto.Encrypt(something,pkey)
            //  and the server could store the player private key
            //  so that you only have to ask the server to decrypt
            //  the key and figure out the type
            // still need the card that's targeting this card or vise versa
            this.GlobalVariables = engine.GlobalVariables;
            this.StopTurn = engine.StopTurn;
            //this.TurnPlayer = engine.TurnPlayer.Id;
            if (engine.ActivePlayer != null) this.ActivePlayer = engine.ActivePlayer.Id;
            this.TurnNumber = engine.TurnNumber;
            this.GameBoard = engine.GameBoard.Name;
            if (Play.Player.LocalPlayer == fromPlayer)
            {
                CurrentUniqueId = engine.CurrentUniqueId;
            }
            Players = Play.Player.All
                .Where(x => !includeFromPlayer ? x.Id != fromPlayer.Id : true)
                .Select(x => new JodsEnginePlayerSaveState().Create(x, fromPlayer))
                .ToArray()
            ;
            Table = new GroupSaveState().Create(Program.GameEngine.Table, fromPlayer);
            Table.Visiblity = GroupVisibility.Undefined;
            SessionId = engine.SessionId;
            return this;
        }

        public JodsEngineGameSaveState Load(GameEngine engine, Play.Player fromPlayer)
        {
            var state = this;
            foreach (var gv in state.GlobalVariables)
            {
                Program.GameEngine.GlobalVariables[gv.Key] = gv.Value;
            }

            Program.GameEngine.StopTurn = state.StopTurn;
            Program.GameEngine.ChangeGameBoard(state.GameBoard);
            Program.GameEngine.TurnNumber = state.TurnNumber;
            Program.GameEngine.ActivePlayer = Play.Player.Find(state.ActivePlayer);

            foreach (var player in state.Players)
            {
                var playPlayer = Play.Player.Find(player.Id);
                playPlayer.Color = player.Color;
                playPlayer.ActualColor = player.Color;
                playPlayer.Brush = new SolidColorBrush(playPlayer.Color);
                playPlayer.TransparentBrush = new SolidColorBrush(playPlayer.Color) { Opacity = 0.4};

                if (player is JodsEnginePlayerSaveState playerSaveState) {
                    foreach (var gv in playerSaveState.GlobalVariables) {
                        playPlayer.GlobalVariables[gv.Key] = gv.Value;
                    }

                    foreach (var counter in playerSaveState.Counters) {
                        var cnt = Play.Counter.Find(counter.Id);
                        cnt.SetValue(counter.Value, playPlayer, false, false);
                    }

                    foreach (var g in playerSaveState.Groups) {
                        LoadGroup(g, fromPlayer);
                    }
                }
            }

            LoadGroup(Table, fromPlayer, true);

            return this;
        }

        internal void LoadGroup(GroupSaveState g, Play.Player fromPlayer, bool isTable = false)
        {
            var group = Play.Group.Find(g.Id);
            if (!isTable)
                group.Controller = Play.Player.Find(g.Controller);
            group.Viewers = g.Viewers.Select(Play.Player.Find).ToList();
            group.Visibility = g.Visiblity;
            foreach (var c in g.Cards)
            {
                var owner = Play.Player.Find(c.Owner);
                DataNew.Entities.Card model = null;
                if (c.Type != Guid.Empty)
                    model =
                        Core.DataManagers.GameManager.Get()
                            .GetById(Program.GameEngine.Definition.Id)
                            .GetCardById(c.Type);
                var card = Play.Card.Find(c.Id);
                if (fromPlayer == owner && card != null)
                {
                    //card.Type.Key = ulong.Parse(c.EncType);
                    card.SetModel(model.Clone());
                    //card.Type = new CardIdentity(card.Id){Key=(ulong)c.EncType,Model = model.Clone(),MySecret = owner == Play.Player.LocalPlayer};
                    //Play.Card.Remove(card);
                    //card = null;
                }
                if (card == null)
                    card = new Play.Card(owner, c.Id, model, owner == Play.Player.LocalPlayer,c.Size);                group.Remove(card);
                group.Add(card);
                card.Group = group;
                card.SwitchTo(owner, c.Alternate, false);
                card.Controller = Play.Player.Find(c.Controller);
                card.DeleteWhenLeavesGroup = c.DeleteWhenLeavesGroup;
                card.SetFaceUp(c.FaceUp);
                card.SetHighlight(c.HighlightColor);
                card.SetIndex(c.Index);
                card.Orientation = c.Orientation;
                card.SetOverrideGroupVisibility(c.OverrideGroupVisibility);
                card.SetTargetedBy(Play.Player.Find(c.TargetedBy));
                card.TargetsOtherCards = c.TargetsOtherCards;
                card.X = c.X;
                card.Y = c.Y;
                card.PropertyOverrides = c.PropertyOverrides;

                foreach (var m in c.Markers)
                {
                    card.SetMarker(card.Owner, m.Id, m.Name, m.Count, false);
                }
                foreach (var pp in c.PeekingPlayers.Select(Play.Player.Find))
                {
                    card.PeekingPlayers.Add(pp);
                }
            }
            group.OnCardsChanged();
        }
    }
}