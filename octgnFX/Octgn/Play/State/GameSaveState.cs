﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */


namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;

    public abstract class SaveState<T, T1>
    {
        public abstract T1 Create(T tp, Play.Player fromPlayer);
    }

    public class GameSaveState : SaveState<GameEngine, GameSaveState>
    {
        public PlayerSaveState[] Players { get; set; }

        public Dictionary<string, string> GlobalVariables { get; set; }
        public int TurnNumber { get; set; }
        public byte ActivePlayer { get; set; }
        public bool StopTurn { get; set; }
        public string GameBoard { get; set; }
        public ushort CurrentUniqueId { get; set; }
        public Guid SessionId { get; set; }
        public GroupSaveState Table { get; set; }

        public override GameSaveState Create(GameEngine tp, Play.Player fromPlayer) {
            return Create(tp, fromPlayer, false);
        }

        public GameSaveState Create(GameEngine engine, Play.Player fromPlayer, bool includeFromPlayer)
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
                .Select(x => new PlayerSaveState().Create(x, fromPlayer))
                .ToArray()
            ;
            Table = new GroupSaveState().Create(Program.GameEngine.Table, fromPlayer);
            Table.Visiblity = GroupVisibility.Undefined;
            SessionId = engine.SessionId;
            return this;
        }

        public GameSaveState Load(GameEngine engine, Play.Player fromPlayer)
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

            foreach (var p in state.Players)
            {
                var player = Play.Player.Find(p.Id);
                player.Color = p.Color;
                player.ActualColor = p.Color;
                player.Brush = new SolidColorBrush(player.Color);
                player.TransparentBrush = new SolidColorBrush(player.Color) { Opacity = 0.4};
                foreach (var gv in p.GlobalVariables)
                {
                    player.GlobalVariables[gv.Key] = gv.Value;
                }

                foreach (var counter in p.Counters)
                {
                    var cnt = Play.Counter.Find(counter.Id);
                    cnt.SetValue(counter.Value, player, false, false);
                }

                foreach (var g in p.Groups)
                {
                    LoadGroup(g, fromPlayer);
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

    public class CardSaveState : SaveState<Play.Card, CardSaveState>
    {
        public int Id { get; set; }
        public Guid Type { get; set; }
        public int Index { get; set; }
        public bool FaceUp { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public MarkerSaveState[] Markers { get; set; }
        public bool DeleteWhenLeavesGroup { get; set; }
        public bool OverrideGroupVisibility { get; set; }
        public CardOrientation Orientation { get; set; }
        public bool IsTarget { get; set; }
        public byte TargetedBy { get; set; }
        public bool TargetsOtherCards { get; set; }
        public Color? HighlightColor { get; set; }
        public byte[] PeekingPlayers { get; set; }
        public string Alternate { get; set; }
        public byte Controller { get; set; }
        public byte Owner { get; set; }
        public string Size { get; set; }
        public Dictionary<string, Dictionary<string, object>> PropertyOverrides { get; set; }

        public CardSaveState()
        {

        }

        public override CardSaveState Create(Play.Card card, Play.Player fromPlayer)
        {
            this.Id = card.Id;
            this.Type = card.Type.Model.Id;
            this.Index = card.GetIndex();
            this.FaceUp = ((card.FaceUp && card.Group.Viewers.Contains(fromPlayer)) || (card.Group.Viewers.Contains(fromPlayer)) || card.IsVisibleToAll());
            X = card.X;
            Y = card.Y;

            this.Markers =
                card.Markers.Select(x => new MarkerSaveState().Create(x, fromPlayer)).ToArray();
            this.DeleteWhenLeavesGroup = card.DeleteWhenLeavesGroup;
            this.OverrideGroupVisibility = card.OverrideGroupVisibility;
            this.Orientation = card.Orientation;
            //this.IsTarget = card.tar
            //this.TargetedBy = card.TargetedBy.Id;
            if (card.TargetedBy != null)
                this.TargetedBy = card.TargetedBy.Id;
            this.TargetsOtherCards = card.TargetsOtherCards;
            this.HighlightColor = card.HighlightColor;
            this.PeekingPlayers = card.PeekingPlayers.Select(x => x.Id).ToArray();
            this.Alternate = card.Alternate();
            this.Controller = card.Controller.Id;
            this.Owner = card.Owner.Id;
            this.Size = card.Size.Name;
            this.PropertyOverrides = card.PropertyOverrides;

            return this;
        }
    }

    public class GroupSaveState : SaveState<Play.Group, GroupSaveState>
    {
        public int Id { get; set; }
        public byte Controller { get; set; }
        public CardSaveState[] Cards { get; set; }
        public byte[] Viewers { get; set; }
        public GroupVisibility Visiblity { get; set; }

        public GroupSaveState()
        {

        }

        public override GroupSaveState Create(Play.Group group, Play.Player fromPlayer)
        {
            this.Id = group.Id;

            if (group.Controller != null)
                this.Controller = group.Controller.Id;
            this.Cards = group.Cards.Select(x => new CardSaveState().Create(x, fromPlayer)).ToArray();
            this.Viewers = group.Viewers.Select(x => x.Id).ToArray();
            this.Visiblity = group.Visibility;
            return this;
        }

    }

    public class MarkerSaveState : SaveState<Play.Marker, MarkerSaveState>
    {
        public string Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }

        public MarkerSaveState()
        {

        }

        public override MarkerSaveState Create(Play.Marker marker, Play.Player fromPlayer)
        {
            this.Id = marker.Model.Id;
            this.Count = marker.Count;
            this.Name = marker.Model.Name;
            return this;
        }
    }

    public class PlayerSaveState : SaveState<Play.Player, PlayerSaveState>
    {
        public byte Id { get; set; }
        public string Nickname { get; set; }
        public GroupSaveState[] Groups { get; set; }
        public Dictionary<string, string> GlobalVariables { get; set; }
        public CounterSaveState[] Counters { get; set; }
        public Color Color { get; set; }

        public PlayerSaveState()
        {

        }

        public override PlayerSaveState Create(Play.Player play, Play.Player fromPlayer)
        {
            this.Id = play.Id;
            this.Nickname = play.Name;
            this.GlobalVariables = play.GlobalVariables;
            this.Counters = play.Counters.Select(x => new CounterSaveState().Create(x, fromPlayer)).ToArray();
            this.Groups = play.Groups.Select(x => new GroupSaveState().Create(x, fromPlayer)).ToArray();
            this.Color = play.ActualColor;
            return this;
        }
    }

    public class CounterSaveState : SaveState<Play.Counter, CounterSaveState>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public byte TypeId { get; set; }
        public int Id { get; set; }

        public CounterSaveState()
        {

        }

        public override CounterSaveState Create(Play.Counter counter, Play.Player fromPlayer)
        {
            this.Name = counter.Name;
            this.Value = counter.Value;
            this.TypeId = counter.Definition.Id;
            this.Id = counter.Id;
            return this;
        }
    }
}