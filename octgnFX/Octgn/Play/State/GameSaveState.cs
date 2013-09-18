namespace Octgn.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;

    using Octgn.DataNew.Entities;

    public class GameSaveState
    {
        public PlayerSaveState[] Players { get; set; }

        public Dictionary<string, string> GlobalVariables { get; set; }
        public int TurnNumber { get; set; }
        public byte TurnPlayer { get; set; }
        public bool StopTurn { get; set; }

        public GameSaveState Create(GameEngine engine, Play.Player fromPlayer)
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
            this.TurnPlayer = engine.TurnPlayer.Id;
            this.TurnNumber = engine.TurnNumber;
            Players = Play.Player.All.Where(x => x.Id != fromPlayer.Id).Select(x => new PlayerSaveState().Create(x, fromPlayer)).ToArray();
            return this;
        }

    }

    public class CardSaveState
    {
        public int Id { get; set; }
        public ulong EncType { get; set; }
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

        public CardSaveState()
        {
            
        }

        public CardSaveState Create(Play.Card card, Play.Player fromPlayer)
        {
            this.Id = card.Id;
            this.EncType = card.GetEncryptedKey();
            if (card.Type.Revealing)
            {
                Type = card.Type.Model.Id;
            }
            this.Index = card.GetIndex();
            this.FaceUp = ((card.FaceUp && card.Group.Viewers.Contains(fromPlayer)) || (card.Group.Viewers.Contains(fromPlayer))
                           || (card.PlayersLooking.Contains(fromPlayer) || card.PeekingPlayers.Contains(fromPlayer)
                               || card.IsVisibleToAll()));
            X = card.X;
            Y = card.Y;

            this.Markers =
                card.Markers.Select(x => new MarkerSaveState().Create(x.Model.Id, x.Count, x.Model.Name)).ToArray();
            this.DeleteWhenLeavesGroup = card.DeleteWhenLeavesGroup;
            this.OverrideGroupVisibility = card.OverrideGroupVisibility;
            this.Orientation = card.Orientation;
            //this.IsTarget = card.tar
            this.TargetedBy = card.TargetedBy.Id;
            this.TargetsOtherCards = card.TargetsOtherCards;
            this.HighlightColor = card.HighlightColor;
            this.PeekingPlayers = card.PeekingPlayers.Select(x => x.Id).ToArray();
            this.Alternate = card.Alternate();
            this.Controller = card.Controller.Id;
            return this;
        }
    }

    public class GroupSaveState
    {
        public int Id { get; set; }
        public byte Controller { get; set; }
        public CardSaveState[] Cards { get; set; }
        public byte[] Viewers { get; set; }
        public GroupVisibility Visiblity { get; set; }

        public GroupSaveState()
        {
            
        }

        public GroupSaveState Create(Play.Group group, Play.Player fromPlayer)
        {
            this.Id = group.Id;
            this.Controller = group.Controller.Id;
            this.Cards = group.Cards.Select(x => new CardSaveState().Create(x, fromPlayer)).ToArray();
            this.Viewers = group.Viewers.Select(x => x.Id).ToArray();
            this.Visiblity = group.Visibility;
            return this;
        }

    }

    public class MarkerSaveState
    {
        public Guid Id { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }

        public MarkerSaveState()
        {
            
        }

        public MarkerSaveState Create(Guid id, int count, string name)
        {
            this.Id = id;
            this.Count = count;
            this.Name = name;
            return this;
        }
    }

    public class PlayerSaveState
    {
        public GroupSaveState[] Groups { get; set; }
        public Dictionary<string, string> GlobalVariables { get; set; }
        public CounterSaveState[] Counters { get; set; }

        public PlayerSaveState()
        {
            
        }

        public PlayerSaveState Create(Play.Player play, Play.Player fromPlayer)
        {
            this.GlobalVariables = play.GlobalVariables;
            this.Counters = play.Counters.Select(x=>new CounterSaveState().Create(x)).ToArray();
            this.Groups = play.Groups.Select(x => new GroupSaveState().Create(x, fromPlayer)).ToArray();
            return this;
        }
    }

    public class CounterSaveState
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public byte Id { get; set; }

        public CounterSaveState()
        {
            
        }

        public CounterSaveState Create(Play.Counter counter)
        {
            this.Name = counter.Name;
            this.Value = counter.Value;
            this.Id = counter.Definition.Id;
            return this;
        }
    }
}