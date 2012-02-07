using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octgn.Controls;
using Octgn.Data;
using Octgn.Definitions;
using Octgn.Play.Actions;
using Octgn.Play.Gui;

namespace Octgn.Play
{
    [Flags]
    public enum CardOrientation
    {
        Rot0 = 0,
        Rot90 = 1,
        Rot180 = 2,
        Rot270 = 3
    };

    public sealed class Card : ControllableObject
    {
        #region Static interface

        private static readonly Dictionary<int, Card> All = new Dictionary<int, Card>();

        public static string DefaultFront
        {
            get { return Program.Game.Definition.CardDefinition.Front; }
        }

        public static string DefaultBack
        {
            get { return Program.Game.Definition.CardDefinition.Back; }
        }

        internal new static Card Find(int id)
        {
            Card res;
            bool success = All.TryGetValue(id, out res);
            return success ? res : null;
        }

        internal static void Reset()
        {
            All.Clear();
        }

        #endregion Static interface

        #region Private fields

        private readonly int id;

        private readonly ObservableCollection<Player> playersPeeking = new ObservableCollection<Player>();
                                                      // List of players, who had peeked at this card. The list is reset when the card changes group.

        private Color? _highlight;

        private bool _selected;
        private Player _target;
        private double _x, _y;
        private CardDef definition;
        private bool faceUp;
        private Group group;
        private bool isAlternateImage;

        internal bool mayBeConsideredFaceUp;
                      /* For better responsiveness, turning a card face down is applied immediately,
															   without waiting on the server.
															   If a script tries to print the card's lName, when the message arrives the card is already
															   face down although it should still be up. */

        private bool overrideGroupVisibility;

        internal List<Player> playersLooking = new List<Player>(1);
                              // List of players looking at this card currently. A player may appear more than once since he can have more than one window opened

        private CardOrientation rot;
        private CardIdentity type;

        #endregion Private fields

        internal Card(Player owner, int id, ulong key, CardDef def, CardModel model, bool mySecret)
            : base(owner)
        {
            this.id = id;
            Type = new CardIdentity(id) {alias = false, key = key, model = model, mySecret = mySecret};
            definition = def;
            All.Add(id, this);
            isAlternateImage = false;
        }

        public bool IsAlternateImage
        {
            get { return isAlternateImage; }
            set
            {
                if (value != isAlternateImage)
                {
                    Program.Client.Rpc.IsAlternateImage(this, value);

                    isAlternateImage = value;
                    OnPropertyChanged("Picture");
                }
            }
        }

        internal override int Id
        {
            get { return id; }
        }

        public override string Name
        {
            get { return FaceUp && type.model != null ? type.model.Name : "Card"; }
        }

        public string RealName
        {
            get { return type.model != null ? type.model.Name : "Card"; }
        }

        internal CardIdentity Type
        {
            get { return type; }
            set
            {
                if (type != null)
                {
                    // Free the old identity
                    type.inUse = false;
                    // Make changes in the Card hashtable
                    // jods: tying the lId to the CardIdentity is buggy. Trying to change behavior.
                    // All.Remove(type.lId);
                }
                if (value != null)
                {
                    if (value.inUse)
                        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                                 "The same card identity is used for two different cards!");
                    // Acquire the new identity
                    value.inUse = true;
                    // Make changes in the Card hashtable
                    // jods: tying the lId to the CardIdentity is buggy. Trying to change behavior.
                    //All.Add(value.lId, this);
                }
                // Set the value
                type = value;
                OnPropertyChanged("Picture");
            }
        }

        internal bool DeleteWhenLeavesGroup { get; set; }

        public Group Group
        {
            get { return group; }
            internal set
            {
                if (value != group)
                {
                    if (group != null)
                    {
                        // Remove the card from peeking lists
                        foreach (var lookedCards in group.lookedAt.Values)
                            lookedCards.Remove(this);
                    }
                    group = value;
                    // Clear the target status
                    SetTargetedBy(null);
                    // Clear highlights
                    SetHighlight(null);
                    // Remove all markers (TODO: should this be configurable per game?)
                    markers.Clear();
                    // Remove from selection (if any)
                    Selection.Remove(this);
                    // Remove any player looking at the card (if any)
                    playersLooking.Clear();
                    // Clear peeking (if any)
                    PeekingPlayers.Clear();
                    //Switch back to original image.
                    IsAlternateImage = false;
                }
            }
        }

        public bool FaceUp
        {
            get { return faceUp; }
            set
            {
                if (faceUp != value)
                {
                    Program.Client.Rpc.TurnReq(this, value);
                    if (faceUp) mayBeConsideredFaceUp = true; // See comment for mayBeConsideredFaceUp
                    new Turn(Player.LocalPlayer, this, value).Do();
                }
            }
        }

        public bool OverrideGroupVisibility
        {
            get { return overrideGroupVisibility; }
        }

        public CardOrientation Orientation
        {
            get { return rot; }
            set
            {
                if (value != rot)
                {
                    Program.Client.Rpc.RotateReq(this, value);
                    new Rotate(Player.LocalPlayer, this, value).Do();
                }
            }
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value) return;
                bool currentState = IsHighlighted;
                _selected = value;
                OnPropertyChanged("Selected");
                if (currentState != IsHighlighted) OnPropertyChanged("IsHighlighted");
            }
        }

        public double X
        {
            get { return _x; }
            set
            {
                if (Math.Abs(_x - value) > double.Epsilon)
                {
                    _x = value;
                    OnPropertyChanged("X");
                }
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                if (Math.Abs(_y - value) > double.Epsilon)
                {
                    _y = value;
                    OnPropertyChanged("Y");
                }
            }
        }

        public Player TargetedBy
        {
            get { return _target; }
        }

        internal bool TargetsOtherCards { get; set; }

        public Color? HighlightColor
        {
            get { return _highlight; }
            set
            {
                SetHighlight(value);
                Program.Client.Rpc.Highlight(this, value);
            }
        }

        public bool IsHighlighted
        {
            get { return _selected || _highlight != null; }
        }

        public ObservableCollection<Player> PeekingPlayers
        {
            get { return playersPeeking; }
        }

        public string Picture
        {
            get
            {
                if (IsAlternateImage)
                    return Type.model.AlternatePicture;
                if (!FaceUp) return DefaultBack;
                if (Type.model == null) return DefaultFront;
                return Type.model.Picture;
            }
        }

        public void ToggleTarget()
        {
            if (TargetedBy != null || TargetsOtherCards)
                Untarget();
            else
                Target();
        }

        public void Target()
        {
            if (TargetedBy == Player.LocalPlayer) return;
            Program.Client.Rpc.TargetReq(this);
            new Target(Player.LocalPlayer, this, null, true).Do();
        }

        public void Untarget()
        {
            if (TargetedBy == null && !TargetsOtherCards) return;
            Program.Client.Rpc.UntargetReq(this);
            new Target(Player.LocalPlayer, this, null, false).Do();
        }

        public void Target(Card otherCard)
        {
            if (otherCard == null)
            {
                Target();
                return;
            }
            Program.Client.Rpc.TargetArrowReq(this, otherCard);
            new Target(Player.LocalPlayer, this, otherCard, true).Do();
        }

        public override string ToString()
        {
            return Name;
        }

        public object GetProperty(string name)
        {
            if (type.model == null) return null;
            if (name == "Name") return type.model.Name;
            if (name == "Id") return type.model.Id;
            return type.model.Properties[name];
        }

        public void MoveTo(Group to, bool lFaceUp)
        {
            // Default: move cards to the end of hand, top of table; but top of piles (which is index 0)
            int toIdx = to is Pile ? 0 : to.Cards.Count;
            MoveTo(to, lFaceUp, toIdx);
        }

        public void MoveTo(Group to, bool lFaceUp, int idx)
        {
            if (to != Group || idx >= Group.Count || Group[idx] != this)
            {
                if (to.Visibility != GroupVisibility.Undefined) lFaceUp = FaceUp;
                Program.Client.Rpc.MoveCardReq(this, to, idx, lFaceUp);
                new MoveCard(Player.LocalPlayer, this, to, idx, lFaceUp).Do();
            }
        }

        public void MoveToTable(int x, int y, bool lFaceUp, int idx)
        {
            Program.Client.Rpc.MoveCardAtReq(this, x, y, idx, lFaceUp);
            new MoveCard(Player.LocalPlayer, this, x, y, idx, lFaceUp).Do();
        }

        public int GetIndex()
        {
            return Group.GetCardIndex(this);
        }

        public void SetIndex(int idx)
        {
            Group.SetCardIndex(this, idx);
        }

        public void Peek()
        {
            if (FaceUp) return;
            Program.Client.Rpc.PeekReq(this);
            Type.Revealed += PeekContinuation;
        }

        private void PeekContinuation(object sender, RevealEventArgs e)
        {
            var identity = (CardIdentity) sender;
            identity.Revealed -= PeekContinuation;
            if (e.NewIdentity.model == null)
            {
                e.NewIdentity.Revealed += PeekContinuation;
                return;
            }
            Program.TracePlayerEvent(Player.LocalPlayer, "You peeked at {0}.", e.NewIdentity.model);
        }

        internal string GetPicture(bool up)
        {
            if (IsAlternateImage)
                return Type.model.AlternatePicture;
            if (!up) return DefaultBack;
            if (Type == null || Type.model == null) return DefaultFront;
            return Type.model.Picture;
        }

        internal BitmapImage GetBitmapImage(bool up)
        {
            if (IsAlternateImage)
            {
                var bmp = new BitmapImage(new Uri(Type.model.AlternatePicture)) {CacheOption = BitmapCacheOption.OnLoad};
                bmp.Freeze();
                return bmp;
            }
            if (!up) return Program.Game.CardBackBitmap;
            if (Type == null || Type.model == null) return Program.Game.CardFrontBitmap;
            var bmpo = new BitmapImage(new Uri(Type.model.Picture)) {CacheOption = BitmapCacheOption.OnLoad};
            bmpo.Freeze();
            return bmpo;
        }

        internal void SetOrientation(CardOrientation value)
        {
            if (value != rot)
            {
                rot = value;
                OnPropertyChanged("Orientation");
            }
        }

        internal void SetFaceUp(bool lFaceUp)
        {
            if (this.faceUp != lFaceUp)
            {
                this.faceUp = lFaceUp;
                OnPropertyChanged("FaceUp");
                if (lFaceUp) PeekingPlayers.Clear();
            }
        }

        internal void SetOverrideGroupVisibility(bool overrides)
        {
            overrideGroupVisibility = overrides;
        }

        internal void SetTargetedBy(Player player)
        {
            if (_target != player)
            {
                _target = player;
                OnPropertyChanged("TargetedBy");
            }
        }

        internal void SetHighlight(Color? value)
        {
            if (value == _highlight) return;
            bool currentState = IsHighlighted;
            _highlight = value;
            OnPropertyChanged("HighlightColor");
            if (currentState != IsHighlighted) OnPropertyChanged("IsHighlighted");
        }

        internal void SetVisibility(GroupVisibility visibility, List<Player> viewers)
        {
            switch (visibility)
            {
                case GroupVisibility.Nobody:
                    SetFaceUp(false);
                    break;
                case GroupVisibility.Everybody:
                    SetFaceUp(true);
                    Reveal();
                    break;
                case GroupVisibility.Undefined:
                    if (FaceUp) Reveal();
                    break;
                case GroupVisibility.Custom:
                    SetFaceUp(viewers.Contains(Player.LocalPlayer));
                    RevealTo(viewers);
                    break;
                default: // could be GroupVisibilty.Owner
                    Debug.Fail("[Card.SetVisibility] Invalid visibility!");
                    return;
            }
        }

        internal void SetModel(CardModel model)
        {
            Type.model = model;
            OnPropertyChanged("Picture");
        }

        internal bool IsVisibleToAll()
        {
            Group g = Group;
            return g.Visibility == GroupVisibility.Everybody || (g.Visibility == GroupVisibility.Undefined && FaceUp);
        }

        protected override void OnControllerChanged()
        {
            if (Selected && Controller != Player.LocalPlayer)
                Selection.Remove(this);
        }

        internal override void NotControlledError()
        {
            Tooltip.PopupError("You don't control this card.");
        }

        internal override bool TryToManipulate()
        {
            // FIX (jods): Containing group has to be manipulable as well,
            // e.g. during a shuffle a pile is locked
            if (!Group.TryToManipulate()) return false;
            return base.TryToManipulate();
        }

        internal void Reveal()
        {
            // Check if the type is already being revealed.
            // This may happen e.g. when moving a card between zones faster than the network latency.
            // It then leads to bugs if not taken good care of.
            if (Type.revealing) return;

            Type.revealing = true;
            if (!Type.mySecret) return;
            Program.Client.Rpc.Reveal(this, type.key, type.alias ? Guid.Empty : type.model.Id);
        }

        internal void RevealTo(IEnumerable<Player> players)
        {
            // If it's not our secret, we can't reveal it!
            if (!Type.mySecret)
            {
                // If the type is public and it's being revealed to myself,
                // trigger the OnReveal event (e.g. during a Peek of a known face-down card)
                if (Type.model != null && players.Contains(Player.LocalPlayer))
                    Type.OnRevealed(Type);
                return;
            }

            // If it's an alias pass it to the one who created it
            if (Type.alias)
            {
                Player p = Player.Find((byte) (Type.key >> 16));
                Program.Client.Rpc.RevealToReq(p, players.ToArray(), this, Crypto.Encrypt(Type.key, p.PublicKey));
            }
                // Else pass to every viewer
            else
            {
                var pArray = new Player[1];
                foreach (Player p in players)
                {
                    if (p == Player.LocalPlayer)
                        Type.OnRevealed(Type);
                    else
                    {
                        pArray[0] = p;
                        Program.Client.Rpc.RevealToReq(p, pArray, this, Crypto.Encrypt(Type.model.Id, p.PublicKey));
                    }
                }
            }
        }

        #region Comparers

        #region Nested type: NameComparer

        public class NameComparer : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                return System.String.CompareOrdinal(((Card) x).Name, ((Card) y).Name);
            }

            #endregion
        }

        #endregion

        #region Nested type: RealNameComparer

        public class RealNameComparer : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                return System.String.CompareOrdinal(((Card) x).RealName, ((Card) y).RealName);
            }

            #endregion
        }

        #endregion

        #endregion Comparers

        #region Markers

        private readonly ObservableCollection<Marker> markers = new ObservableCollection<Marker>();

        public IList<Marker> Markers
        {
            get { return markers; }
        }

        internal void AddMarker(MarkerModel model, ushort count)
        {
            Marker marker = markers.FirstOrDefault(m => m.Model.Equals(model));
            if (marker != null)
                marker.SetCount((ushort) (marker.Count + count));
            else if (count > 0)
                markers.Add(new Marker(this, model, count));
        }

        internal void AddMarker(MarkerModel model)
        {
            AddMarker(model, 1);
        }

        internal int RemoveMarker(Marker marker, ushort count)
        {
            if (!markers.Contains(marker)) return 0;

            if (marker.Count <= count)
            {
                int realCount = marker.Count;
                markers.Remove(marker);
                return realCount;
            }

            marker.SetCount((ushort) (marker.Count - count));
            return count;
        }

        internal void RemoveMarker(Marker marker)
        {
            markers.Remove(marker);
        }

        internal Marker FindMarker(Guid lId, string name)
        {
            return markers.FirstOrDefault(m =>
                                          m.Model.id == lId &&
                                          (!(m.Model is DefaultMarkerModel) || m.Model.Name == name));
        }

        internal void SetMarker(Player player, Guid lId, string name, int count)
        {
            int oldCount = 0;
            Marker marker = FindMarker(lId, name);
            if (marker != null)
            {
                oldCount = marker.Count;
                marker.SetCount((ushort) count);
            }
            else if (count > 0)
            {
                MarkerModel model = Program.Game.GetMarkerModel(lId);
                var defaultMarkerModel = model as DefaultMarkerModel;
                if (defaultMarkerModel != null)
                    (defaultMarkerModel).SetName(name);
                AddMarker(model, (ushort) count);
            }
            if (count != oldCount)
                Program.TracePlayerEvent(player, "{0} sets {1} ({2}) markers {3} on {4}.",
                                         player, count, (count - oldCount).ToString("+#;-#"),
                                         marker != null ? marker.Model.Name : name, this);
        }

        #endregion Markers
    }
}