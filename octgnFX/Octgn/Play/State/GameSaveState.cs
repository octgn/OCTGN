namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;

    public abstract class SaveState<T,T1>
    {
        public abstract T1 Create(T tp, Play.Player fromPlayer);
    }

    public class GameSaveState : SaveState<GameEngine,GameSaveState>
    {
        public PlayerSaveState[] Players { get; set; }

        public Dictionary<string, string> GlobalVariables { get; set; }
        public int TurnNumber { get; set; }
        public byte TurnPlayer { get; set; }
        public bool StopTurn { get; set; }
		public ushort CurrentUniqueId { get; set; }
		public Guid SessionId { get; set; }
		public GroupSaveState Table { get; set; }

        public override GameSaveState Create(GameEngine engine, Play.Player fromPlayer)
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
            if (engine.TurnPlayer != null) this.TurnPlayer = engine.TurnPlayer.Id;
            this.TurnNumber = engine.TurnNumber;
            if (Play.Player.LocalPlayer == fromPlayer)
            {
                CurrentUniqueId = engine.CurrentUniqueId;
            }
            Players = Play.Player.All.Where(x => x.Id != fromPlayer.Id).Select(x => new PlayerSaveState().Create(x, fromPlayer)).ToArray();
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
            Program.GameEngine.TurnNumber = state.TurnNumber;
            Program.GameEngine.TurnPlayer = Play.Player.Find(state.TurnPlayer);

            foreach (var p in state.Players)
            {
                var player = Play.Player.Find(p.Id);
                foreach (var gv in p.GlobalVariables)
                {
                    player.GlobalVariables[gv.Key] = gv.Value;
                }

                foreach (var counter in p.Counters)
                {
                    var cnt = Play.Counter.Find(counter.Id);
                    cnt.SetValue(counter.Value, player, false);
                }

                foreach (var g in p.Groups)
                {
                    LoadGroup(g);
                }
            }

			LoadGroup(Table,true);

            return this;
        }

        internal void LoadGroup(GroupSaveState g, bool isTable = false)
        {
            var group = Play.Group.Find(g.Id);
            if(!isTable)
                group.Controller = Play.Player.Find(g.Controller);
            group.Viewers = g.Viewers.Select(Play.Player.Find).ToList();
            group.Visibility = g.Visiblity;
            foreach (var c in g.Cards)
            {
                DataNew.Entities.Card model = null;
                if (c.Type != Guid.Empty)
                    model =
                        Core.DataManagers.GameManager.Get()
                            .GetById(Program.GameEngine.Definition.Id)
                            .GetCardById(c.Type);
                var owner = Play.Player.Find(c.Owner);
                var card = Play.Card.Find(c.Id);
				if(card == null)
					card = new Play.Card(owner, c.Id, (ulong)c.EncType, model, owner == Play.Player.LocalPlayer);
                group.Remove(card);
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
        public decimal EncType { get; set; }
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

        public CardSaveState()
        {
            
        }

        public override CardSaveState Create(Play.Card card, Play.Player fromPlayer)
        {
            this.Id = card.Id;
            this.EncType = card.GetEncryptedKey();
            if (card.Type.Revealing || fromPlayer == Play.Player.LocalPlayer)
            {
                Type = card.Type.Model.Id;
            }
            else Type = Guid.Empty;
            this.Index = card.GetIndex();
            this.FaceUp = ((card.FaceUp && card.Group.Viewers.Contains(fromPlayer)) || (card.Group.Viewers.Contains(fromPlayer))
                           || (card.PlayersLooking.Contains(fromPlayer) || card.PeekingPlayers.Contains(fromPlayer)
                               || card.IsVisibleToAll()));
            X = card.X;
            Y = card.Y;

            this.Markers =
                card.Markers.Select(x => new MarkerSaveState().Create(x,fromPlayer)).ToArray();
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
            
			if(group.Controller != null)
                this.Controller = group.Controller.Id;
            this.Cards = group.Cards.Select(x => new CardSaveState().Create(x, fromPlayer)).ToArray();
            this.Viewers = group.Viewers.Select(x => x.Id).ToArray();
            this.Visiblity = group.Visibility;
            return this;
        }

    }

    public class MarkerSaveState : SaveState<Play.Marker, MarkerSaveState>
    {
        public Guid Id { get; set; }
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
        public GroupSaveState[] Groups { get; set; }
        public Dictionary<string, string> GlobalVariables { get; set; }
        public CounterSaveState[] Counters { get; set; }

        public PlayerSaveState()
        {
            
        }

        public override PlayerSaveState Create(Play.Player play, Play.Player fromPlayer)
        {
            this.Id = play.Id;
            this.GlobalVariables = play.GlobalVariables;
            this.Counters = play.Counters.Select(x=>new CounterSaveState().Create(x,fromPlayer)).ToArray();
            this.Groups = play.Groups.Select(x => new GroupSaveState().Create(x, fromPlayer)).ToArray();
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