using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Octgn.Play.Actions;
using Octgn.Play.Gui;
using Octgn.Utils;

namespace Octgn.Play
{
    using System.Reflection;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.PlayTable;
    using Octgn.PlayTable.Controls;

    using log4net;

    public sealed class Card : ControllableObject, IPlayCard
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Static interface

        #endregion Static interface

        #region Private fields

        private readonly int _id;

        public List<IPlayPlayer> PlayersLooking
        {
            get
            {
                return this.playersLooking;
            }
            internal set
            {
                this.playersLooking = value;
            }
        }

        internal List<IPlayPlayer> playersLooking = new List<IPlayPlayer>(1);
         // List of players looking at this card currently. A player may appear more than once since he can have more than one window opened
        private readonly ObservableCollection<IPlayPlayer> _playersPeeking = new ObservableCollection<IPlayPlayer>();
        // List of players, who had peeked at this card. The list is reset when the card changes group.
        internal bool MayBeConsideredFaceUp;
        /* For better responsiveness, turning a card face down is applied immediately,
															   without waiting on the server.
															   If a script tries to print the card's lName, when the message arrives the card is already
															   face down although it should still be up. */

        
        //private CardDef _definition;
        private bool _faceUp;
        private IPlayGroup _group;
        private Color? _highlight;
        private bool _isAlternateImage;
        internal Octgn.DataNew.Entities.Card _alternateOf;
        private int numberOfSwitchWithAlternatesNotPerformed = 0;

        private CardOrientation _rot;
        private bool _selected;
        private CardIdentity _type;
        private double _x, _y;

        #endregion Private fields
        
        internal Card(IPlayPlayer owner, int id, ulong key,  DataNew.Entities.Card model, bool mySecret)
            : base(owner)
        {
            _id = id;
            Type = new CardIdentity(id) {Alias = false, Key = key, Model = model.Clone() , MySecret = mySecret};
            // var _definition = def;
            Program.Card.All.Add(id, this);
            _alternateOf = null;
            numberOfSwitchWithAlternatesNotPerformed = 0;
            _isAlternateImage = false;
        }

        //public bool IsAlternateImage
        //{
        //    get { return _isAlternateImage; }
        //    set
        //    {
        //        if (value == _isAlternateImage) return;
        //        Program.Client.Rpc.IsAlternateImage(this, value);

        //        _isAlternateImage = value;
        //        OnPropertyChanged("Picture");
        //    }
        //}

        public override int Id
        {
            get { return _id; }
        }

        public override string Name
        {
            get { return FaceUp && _type.Model != null ? _type.Model.PropertyName() : "Card"; }
        }

        public string RealName
        {
            get { return _type.Model != null ? _type.Model.PropertyName(): "Card"; }
        }

        public CardIdentity Type
        {
            get { return _type; }
            set
            {
                if (_type != null)
                {
                    // Free the old identity
                    _type.InUse = false;
                    // Make changes in the Card hashtable
                    // jods: tying the lId to the CardIdentity is buggy. Trying to change behavior.
                    // All.Remove(type.lId);
                }
                if (value != null)
                {
                    if (value.InUse)
                        Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.Event,
                                                 "The same card identity is used for two different cards!");
                    // Acquire the new identity
                    value.InUse = true;
                    // Make changes in the Card hashtable
                    // jods: tying the lId to the CardIdentity is buggy. Trying to change behavior.
                    //All.Add(value.lId, this);
                }
                // Set the value
                _type = value;
                OnPropertyChanged("Picture");
            }
        }

        public bool DeleteWhenLeavesGroup { get; set; }

        public IPlayGroup Group
        {
            get { return _group; }
            set
            {
                if (value == _group) return;
                if (_group != null)
                {
                    // Remove the card from peeking lists
                    foreach (List<IPlayCard> lookedCards in _group.LookedAt.Values)
                        lookedCards.Remove(this);
                }
                _group = value;
                // Clear the target status
                SetTargetedBy(null);
                // Clear highlights
                SetHighlight(null);
                // Remove all markers (TODO: should this be configurable per game?)
                _markers.Clear();
                // Remove from selection (if any)
                Selection.Remove(this);
                // Remove any player looking at the card (if any)
                PlayersLooking.Clear();
                // Clear peeking (if any)
                PeekingPlayers.Clear();
                //Switch back to original image.
                this.SwitchTo(Program.Player.LocalPlayer);
            }
        }

        public bool FaceUp
        {
            get { return _faceUp; }
            set
            {
                if (_faceUp == value) return;
                Program.Client.Rpc.TurnReq(this, value);
                if (_faceUp) MayBeConsideredFaceUp = true; // See comment for mayBeConsideredFaceUp
                new Turn(Program.Player.LocalPlayer, this, value).Do();
            }
        }
        //Okay, someone please explain to me why we have the setter above and the set function below? (V)_V
        public void SetFaceUp(bool lFaceUp)
        {
            if (_faceUp == lFaceUp) return;
            _faceUp = lFaceUp;
            OnPropertyChanged("FaceUp");
            if (lFaceUp)
            {
                PeekingPlayers.Clear();
            }
        }

        public bool OverrideGroupVisibility { get; private set; }

        public CardOrientation Orientation
        {
            get { return _rot; }
            set
            {
                if (value == _rot) return;
                Program.Client.Rpc.RotateReq(this, value);
                new Rotate(Program.Player.LocalPlayer, this, value).Do();
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
                if (Math.Abs(_x - value) <= double.Epsilon) return;
                _x = value;
                OnPropertyChanged("X");
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                if (Math.Abs(_y - value) <= double.Epsilon) return;
                _y = value;
                OnPropertyChanged("Y");
            }
        }

        public IPlayPlayer TargetedBy { get; private set; }

        public bool TargetsOtherCards { get; set; }

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

        public ObservableCollection<IPlayPlayer> PeekingPlayers
        {
            get { return _playersPeeking; }
        }

        public string Picture
        {
            get
            {
                if (!FaceUp) return Program.Card.DefaultBack;
                return Type.Model == null ? Program.Card.DefaultFront : Type.Model.GetPicture();
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
            if (TargetedBy == Program.Player.LocalPlayer) return;
            Program.Client.Rpc.TargetReq(this);
            new Target(Program.Player.LocalPlayer, this, null, true).Do();
        }

        public void Untarget()
        {
            if (TargetedBy == null && !TargetsOtherCards) return;
            Program.Client.Rpc.UntargetReq(this);
            new Target(Program.Player.LocalPlayer, this, null, false).Do();
        }

        public void Target(IPlayCard otherCard)
        {
            if (otherCard == null)
            {
                Target();
                return;
            }
            Program.Client.Rpc.TargetArrowReq(this, otherCard);
            new Target(Program.Player.LocalPlayer, this, otherCard, true).Do();
        }

        public override string ToString()
        {
            return Name;
        }

        public string[] Alternates()
        {
            if(_type.Model == null)return new string[0];
            return _type.Model.Properties.Select(x => x.Key).ToArray();
        }

        public string Alternate()
        {
            return this._type.Model == null ? "" : this._type.Model.Alternate;
        }

        public void SwitchTo(IPlayPlayer player, string alternate = "")
        {
            if (_type.Model == null) return;
            if (_type.Model.Alternate.ToLower() == alternate.ToLower()) return;
            if(player.Id == Program.Player.LocalPlayer.Id)
                Program.Client.Rpc.CardSwitchTo(player,this,alternate);
            _type.Model.SetPropertySet(alternate);
            this.OnPropertyChanged("Picture");
        }

        public object GetProperty(string name)
        {
            if (_type.Model == null) return null;
            if (name == "Name") return _type.Model.PropertyName();
            if (name == "Id") return _type.Model.Id;
            var prop = _type.Model.PropertySet().FirstOrDefault(x => x.Key.Name == name);
            //var prop = _type.Model.Properties.FirstOrDefault(x => x.Key.Name == name);
            return prop.Value;
        }

        public void MoveTo(IPlayGroup to, bool lFaceUp)
        {
            // Default: move cards to the end of hand, top of table; but top of piles (which is index 0)
            int toIdx = to is Pile ? 0 : to.Cards.Count;
            MoveTo(to, lFaceUp, toIdx);
        }

        public void MoveTo(IPlayGroup to, bool lFaceUp, int idx)
        {
            if (to == Group && idx < Group.Count && Group[idx] == this) return;
            if (to.Visibility != GroupVisibility.Undefined) lFaceUp = FaceUp;
            Program.Client.Rpc.MoveCardReq(this, to, idx, lFaceUp);
            new MoveCard(Program.Player.LocalPlayer, this, to, idx, lFaceUp).Do();
        }

        public void MoveToTable(int x, int y, bool lFaceUp, int idx)
        {
            Program.Client.Rpc.MoveCardAtReq(this, x, y, idx, lFaceUp);
            new MoveCard(Program.Player.LocalPlayer, this, x, y, idx, lFaceUp).Do();
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

        private static void PeekContinuation(object sender, RevealEventArgs e)
        {
            var identity = (CardIdentity) sender;
            identity.Revealed -= PeekContinuation;
            if (e.NewIdentity.Model == null)
            {
                e.NewIdentity.Revealed += PeekContinuation;
                return;
            }
            Program.Trace.TracePlayerEvent(Program.Player.LocalPlayer, "You peeked at {0}.", e.NewIdentity.Model);
        }

        public string GetPicture(bool up)
        {
            if (!up) return Program.Card.DefaultBack;
            if (Type == null || Type.Model == null) return Program.Card.DefaultFront;
            return Type.Model.GetPicture();
        }

        internal BitmapImage GetBitmapImage(bool up)
        {
            if (!up) return Program.GameEngine.CardBackBitmap;
            if (Type == null || Type.Model == null) return Program.GameEngine.CardFrontBitmap;
            var bmpo = new BitmapImage(new Uri(Type.Model.GetPicture())) {CacheOption = BitmapCacheOption.OnLoad};
            bmpo.Freeze();
            return bmpo;
        }

        public void SetOrientation(CardOrientation value)
        {
            if (value == _rot) return;
            _rot = value;
            OnPropertyChanged("Orientation");
        }

        public void SetOverrideGroupVisibility(bool overrides)
        {
            OverrideGroupVisibility = overrides;
        }

        public void SetTargetedBy(IPlayPlayer player)
        {
            if (TargetedBy == player) return;
            TargetedBy = player;
            OnPropertyChanged("TargetedBy");
        }

        public void SetHighlight(Color? value)
        {
            if (value == _highlight) return;
            bool currentState = IsHighlighted;
            _highlight = value;
            OnPropertyChanged("HighlightColor");
            if (currentState != IsHighlighted) OnPropertyChanged("IsHighlighted");
        }

        public void SetVisibility(GroupVisibility visibility, List<IPlayPlayer> viewers)
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
                    SetFaceUp(viewers.Contains(Program.Player.LocalPlayer));
                    RevealTo(viewers);
                    break;
                default: // could be GroupVisibilty.Owner
                    Debug.Fail("[Card.SetVisibility] Invalid visibility!");
                    return;
            }
        }

        public void SetModel(DataNew.Entities.Card model)
        {
            Log.Info("SetModel event happened!");
            Type.Model = model;
            OnPropertyChanged("Picture");//This should be changed - the model is much more than just the picture.
        }

        public bool IsVisibleToAll()
        {
            IPlayGroup g = Group;
            return g.Visibility == GroupVisibility.Everybody || (g.Visibility == GroupVisibility.Undefined && FaceUp);
        }

        public override void OnControllerChanged()
        {
            if (Selected && Controller != Program.Player.LocalPlayer)
                Selection.Remove(this);
        }

        public override void NotControlledError()
        {
            Tooltip.PopupError("You don't control this card.");
        }

        public override bool TryToManipulate()
        {
            // FIX (jods): Containing group has to be manipulable as well,
            // e.g. during a shuffle a pile is locked
            return Group.TryToManipulate() && base.TryToManipulate();
        }

        public void Reveal()
        {
            // Check if the type is already being revealed.
            // This may happen e.g. when moving a card between zones faster than the network latency.
            // It then leads to bugs if not taken good care of.
            if (Type.Revealing) return;

            Log.Info("REVEAL event about to fire!");
            Type.Revealing = true;
            if (!Type.MySecret) return;
            Program.Client.Rpc.Reveal(this, _type.Key, _type.Alias ? Guid.Empty : _type.Model.Id);
        }

        public void RevealTo(IEnumerable<IPlayPlayer> players)
        {
            // If it's not our secret, we can't reveal it!
            if (!Type.MySecret)
            {
                // If the type is public and it's being revealed to myself,
                // trigger the OnReveal event (e.g. during a Peek of a known face-down card)
                if (Type.Model != null && players.Contains(Program.Player.LocalPlayer))
                    Type.OnRevealed(Type);
                return;
            }

            // If it's an alias pass it to the one who created it
            if (Type.Alias)
            {
                IPlayPlayer p = Program.Player.Find((byte) (Type.Key >> 16));
                Program.Client.Rpc.RevealToReq(p, players.ToArray(), this, Crypto.Encrypt(Type.Key, p.PublicKey));
            }
                // Else pass to every viewer
            else
            {
                var pArray = new IPlayPlayer[1];
                foreach (IPlayPlayer p in players)
                {
                    if (p == Program.Player.LocalPlayer)
                        Type.OnRevealed(Type);
                    else
                    {
                        pArray[0] = p;
                        Program.Client.Rpc.RevealToReq(p, pArray, this, Crypto.Encrypt(Type.Model.Id, p.PublicKey));
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
                return String.CompareOrdinal(((Card) x).Name, ((Card) y).Name);
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
                return String.CompareOrdinal(((Card) x).RealName, ((Card) y).RealName);
            }

            #endregion
        }

        #endregion

        #endregion Comparers

        #region Markers

        private readonly ObservableCollection<PlayMarker> _markers = new ObservableCollection<PlayMarker>();

        public IList<PlayMarker> Markers
        {
            get { return _markers; }
        }

        public void AddMarker(DataNew.Entities.Marker model, ushort count)
        {
            PlayMarker marker = _markers.FirstOrDefault(m => m.Model.Equals(model));
            if (marker != null)
                marker.SetCount((ushort) (marker.Count + count));
            else if (count > 0)
                _markers.Add(new PlayMarker(this, model, count));
        }

        public void AddMarker(DataNew.Entities.Marker model)
        {
            AddMarker(model, 1);
        }

        public int RemoveMarker(PlayMarker marker, ushort count)
        {
            if (!_markers.Contains(marker)) return 0;

            if (marker.Count <= count)
            {
                int realCount = marker.Count;
                _markers.Remove(marker);
                return realCount;
            }

            marker.SetCount((ushort) (marker.Count - count));
            return count;
        }

        public void RemoveMarker(PlayMarker marker)
        {
            _markers.Remove(marker);
        }

        public PlayMarker FindMarker(Guid lId, string name)
        {
            return _markers.FirstOrDefault(m =>
                                           m.Model.Id == lId &&
                                           (!(m.Model is DefaultMarkerModel) || m.Model.Name == name));
        }

        public void SetMarker(IPlayPlayer player, Guid lId, string name, int count)
        {
            int oldCount = 0;
            PlayMarker marker = FindMarker(lId, name);
            if (marker != null)
            {
                oldCount = marker.Count;
                marker.SetCount((ushort) count);
            }
            else if (count > 0)
            {
                DataNew.Entities.Marker model = Program.GameEngine.GetMarkerModel(lId);
                var defaultMarkerModel = model as DefaultMarkerModel;
                if (defaultMarkerModel != null)
                    (defaultMarkerModel).SetName(name);
                AddMarker(model, (ushort) count);
            }
            if (count != oldCount)
                Program.Trace.TracePlayerEvent(player, "{0} sets {1} ({2}) markers {3} on {4}.",
                                         player, count, (count - oldCount).ToString("+#;-#"),
                                         marker != null ? marker.Model.Name : name, this);
        }

        #endregion Markers

        public bool hasProperty(string propertyName)
        {
            return (Type.Model.HasProperty(propertyName));
        }
    }
}