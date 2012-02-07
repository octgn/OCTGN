using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Octgn.Play
{
    public enum CardOrientation { Rot0 = 0, Rot90 = 1, Rot180 = 2, Rot270 = 3 };

    public sealed class Card : ControllableObject
    {
        #region Static interface

        private static readonly Dictionary<int, Card> All = new Dictionary<int, Card>();

        internal static new Card Find(int id)
        {
            Card res;
            bool success = All.TryGetValue(id, out res);
            return success ? res : null;
        }

        internal static void Reset()
        { All.Clear(); }

        public static string DefaultFront
        { get { return Program.Game.Definition.CardDefinition.Front; } }

        public static string DefaultBack
        { get { return Program.Game.Definition.CardDefinition.Back; } }

        #endregion Static interface

        #region Private fields

        private int id;
        private Group group;
        private CardOrientation rot;
        private bool faceUp, overrideGroupVisibility;
        internal List<Player> playersLooking = new List<Player>(1);    // List of players looking at this card currently. A player may appear more than once since he can have more than one window opened
        private ObservableCollection<Player> playersPeeking = new ObservableCollection<Player>();    // List of players, who had peeked at this card. The list is reset when the card changes group.
        private CardIdentity type;
        private Definitions.CardDef definition;
        private bool _selected;
        private double _x, _y;
        private Player _target;
        private Color? _highlight;
        private bool isAlternate = false;
        private bool isAlternateImage =false;
        internal bool mayBeConsideredFaceUp;   /* For better responsiveness, turning a card face down is applied immediately,
															   without waiting on the server.
															   If a script tries to print the card's name, when the message arrives the card is already
															   face down although it should still be up. */

        #endregion Private fields
        public bool IsAlternate
        {
            get { return isAlternate; }
            set
            {
                if (value != isAlternate)
                {
                    Program.Client.Rpc.IsAlternate(this, value);

                    isAlternate = value;
                    OnPropertyChanged("Card");
                }
            }
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
        { get { return id; } }

        public override string Name
        {
            get
            {
                return FaceUp && type.model != null ? type.model.Name : "Card";
            }
        }

        public string RealName
        { get { return type.model != null ? type.model.Name : "Card"; } }

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
                    // jods: tying the id to the CardIdentity is buggy. Trying to change behavior.
                    // All.Remove(type.id);
                }
                if (value != null)
                {
                    if (value.inUse)
                        Program.Trace.TraceEvent(System.Diagnostics.TraceEventType.Warning, EventIds.Event, "The same card identity is used for two different cards!");
                    // Acquire the new identity
                    value.inUse = true;
                    // Make changes in the Card hashtable
                    // jods: tying the id to the CardIdentity is buggy. Trying to change behavior.
                    //All.Add(value.id, this);
                }
                // Set the value
                type = value;
                OnPropertyChanged("Picture");
            }
        }

        internal bool DeleteWhenLeavesGroup
        { get; set; }

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
                    Gui.Selection.Remove(this);
                    // Remove any player looking at the card (if any)
                    playersLooking.Clear();
                    // Clear peeking (if any)
                    PeekingPlayers.Clear();
                    //Switch back to original image.
                    IsAlternateImage = false; //This actually changes the image?
                    IsAlternate = false;
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
                    if (faceUp) mayBeConsideredFaceUp = true;   // See comment for mayBeConsideredFaceUp
                    new Actions.Turn(Player.LocalPlayer, this, value).Do();
                }
            }
        }

        public bool OverrideGroupVisibility
        { get { return overrideGroupVisibility; } }

        public CardOrientation Orientation
        {
            get { return rot; }
            set
            {
                if (value != rot)
                {
                    Program.Client.Rpc.RotateReq(this, value);
                    new Actions.Rotate(Player.LocalPlayer, this, value).Do();
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
                if (_x != value)
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
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged("Y");
                }
            }
        }

        public Player TargetedBy
        { get { return _target; } }

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

        public ObservableCollection<Player> PeekingPlayers { get { return playersPeeking; } }

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
            new Actions.Target(Player.LocalPlayer, this, null, true).Do();
        }

        public void Untarget()
        {
            if (TargetedBy == null && !TargetsOtherCards) return;
            Program.Client.Rpc.UntargetReq(this);
            new Actions.Target(Player.LocalPlayer, this, null, false).Do();
        }

        public void Target(Card otherCard)
        {
            if (otherCard == null)
            { Target(); return; }
            Program.Client.Rpc.TargetArrowReq(this, otherCard);
            new Actions.Target(Player.LocalPlayer, this, otherCard, true).Do();
        }

        public override string ToString()
        { return Name; }

        public object GetProperty(string name)
        {
            if (type.model == null) return null;
            if (name == "Name") return type.model.Name;
            if (name == "Id") return type.model.Id;
            return type.model.Properties[name];
        }

        public void MoveTo(Group to, bool faceUp)
        {
            // Default: move cards to the end of hand, top of table; but top of piles (which is index 0)
            int toIdx = to is Pile ? 0 : to.Cards.Count;
            MoveTo(to, faceUp, toIdx);
        }

        public void MoveTo(Group to, bool faceUp, int idx)
        {
            if (to != Group || idx >= Group.Count || Group[idx] != this)
            {
                if (to.Visibility != GroupVisibility.Undefined) faceUp = FaceUp;
                Program.Client.Rpc.MoveCardReq(this, to, idx, faceUp);
                new Actions.MoveCard(Player.LocalPlayer, this, to, idx, faceUp).Do();
            }
        }

        public void MoveToTable(int x, int y, bool faceUp, int idx)
        {
            Program.Client.Rpc.MoveCardAtReq(this, x, y, idx, faceUp);
            new Actions.MoveCard(Player.LocalPlayer, this, x, y, idx, faceUp).Do();
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
            var identity = (CardIdentity)sender;
            identity.Revealed -= PeekContinuation;
            if (e.NewIdentity.model == null)
            {
                e.NewIdentity.Revealed += PeekContinuation;
                return;
            }
            Program.TracePlayerEvent(Player.LocalPlayer, "You peeked at {0}.", e.NewIdentity.model);
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
                var bmp = new BitmapImage(new Uri(Type.model.AlternatePicture)) { CacheOption = BitmapCacheOption.OnLoad };
                bmp.Freeze();
                return bmp;
            }
            if (!up) return Program.Game.CardBackBitmap;
            if (Type == null || Type.model == null) return Program.Game.CardFrontBitmap;
            var bmpo = new BitmapImage(new Uri(Type.model.Picture)) { CacheOption = BitmapCacheOption.OnLoad };
            bmpo.Freeze();
            return bmpo;
        }

        internal Card(Player owner, int id, ulong key, Definitions.CardDef def, Data.CardModel model, bool mySecret)
            : base(owner)
        {
            this.id = id;
            this.Type = new CardIdentity(id) { alias = false, key = key, model = model, mySecret = mySecret };
            definition = def;
            All.Add(id, this);
            isAlternateImage = false;
            isAlternate = false;
        }

        internal void SetOrientation(CardOrientation value)
        {
            if (value != rot)
            {
                rot = value;
                OnPropertyChanged("Orientation");
            }
        }

        internal void SetFaceUp(bool faceUp)
        {
            if (this.faceUp != faceUp)
            {
                this.faceUp = faceUp;
                OnPropertyChanged("FaceUp");
                if (faceUp) PeekingPlayers.Clear();
            }
        }

        internal void SetOverrideGroupVisibility(bool overrides)
        {
            overrideGroupVisibility = overrides;
        }

        internal void SetTargetedBy(Player player)
        {
            if (this._target != player)
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
                default:    // could be GroupVisibilty.Owner
                    System.Diagnostics.Debug.Fail("[Card.SetVisibility] Invalid visibility!");
                    return;
            }
        }

        #region Markers

        private ObservableCollection<Marker> markers = new ObservableCollection<Marker>();

        public IList<Marker> Markers
        { get { return markers; } }

        internal void AddMarker(Data.MarkerModel model, ushort count)
        {
            Marker marker = markers.FirstOrDefault(m => m.Model.Equals(model));
            if (marker != null)
                marker.SetCount((ushort)(marker.Count + count));
            else if (count > 0)
                markers.Add(new Marker(this, model, count));
        }

        internal void AddMarker(Data.MarkerModel model)
        { AddMarker(model, 1); }

        internal int RemoveMarker(Marker marker, ushort count)
        {
            if (!markers.Contains(marker)) return 0;

            if (marker.Count <= count)
            {
                int realCount = marker.Count;
                markers.Remove(marker);
                return realCount;
            }

            marker.SetCount((ushort)(marker.Count - count));
            return count;
        }

        internal void RemoveMarker(Marker marker)
        { markers.Remove(marker); }

        internal Marker FindMarker(Guid id, string name)
        {
            return markers.FirstOrDefault(m =>
              m.Model.id == id &&
              (!(m.Model is DefaultMarkerModel) || m.Model.Name == name));
        }

        internal void SetMarker(Player player, Guid id, string name, int count)
        {
            int oldCount = 0;
            Marker marker = FindMarker(id, name);
            if (marker != null)
            {
                oldCount = marker.Count;
                marker.SetCount((ushort)count);
            }
            else if (count > 0)
            {
                Data.MarkerModel model = Program.Game.GetMarkerModel(id);
                if (model is DefaultMarkerModel) ((DefaultMarkerModel)model).SetName(name);
                AddMarker(model, (ushort)count);
            }
            if (count != oldCount)
                Program.TracePlayerEvent(player, "{0} sets {1} ({2}) markers {3} on {4}.",
                  player, count, (count - oldCount).ToString("+#;-#"), marker != null ? marker.Model.Name : name, this);
        }

        #endregion Markers

        internal void SetModel(Data.CardModel model)
        {
            Type.model = model;
            OnPropertyChanged("Picture");
        }

        internal bool IsVisibleToAll()
        {
            Group g = this.Group;
            return g.Visibility == GroupVisibility.Everybody || (g.Visibility == GroupVisibility.Undefined && FaceUp);
        }

        protected override void OnControllerChanged()
        {
            if (Selected && Controller != Player.LocalPlayer)
                Gui.Selection.Remove(this);
        }

        internal override void NotControlledError()
        {
            Controls.Tooltip.PopupError("You don't control this card.");
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
                Player p = Player.Find((byte)(Type.key >> 16));
                Program.Client.Rpc.RevealToReq(p, players.ToArray(), this, Crypto.Encrypt(Type.key, p.PublicKey));
            }
            // Else pass to every viewer
            else
            {
                Player[] pArray = new Player[1];
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

        public class NameComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                return string.Compare(((Card)x).Name, ((Card)y).Name);
            }
        }

        public class RealNameComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                return string.Compare(((Card)x).RealName, ((Card)y).RealName);
            }
        }

        #endregion Comparers
    }
}