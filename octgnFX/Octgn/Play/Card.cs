﻿using System;
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

        private readonly int _id;

        internal List<Player> PlayersLooking = new List<Player>(1);
         // List of players looking at this card currently. A player may appear more than once since he can have more than one window opened
        private readonly ObservableCollection<Player> _playersPeeking = new ObservableCollection<Player>();
        // List of players, who had peeked at this card. The list is reset when the card changes group.
        internal bool MayBeConsideredFaceUp;
        /* For better responsiveness, turning a card face down is applied immediately,
															   without waiting on the server.
															   If a script tries to print the card's lName, when the message arrives the card is already
															   face down although it should still be up. */

        
        //private CardDef _definition;
        private bool _faceUp;
        private Group _group;
        private Color? _highlight;
        private bool _isAlternateImage;
        internal CardModel _alternateOf;
        private int numberOfSwitchWithAlternatesNotPerformed = 0;

        private CardOrientation _rot;
        private bool _selected;
        private CardIdentity _type;
        private double _x, _y;

        #endregion Private fields
        
        internal Card(Player owner, int id, ulong key, CardDef def, CardModel model, bool mySecret)
            : base(owner)
        {
            _id = id;
            Type = new CardIdentity(id) {Alias = false, Key = key, Model = model, MySecret = mySecret};
            // var _definition = def;
            All.Add(id, this);
            _alternateOf = null;
            numberOfSwitchWithAlternatesNotPerformed = 0;
            _isAlternateImage = false;
        }

        public bool IsAlternateImage
        {
            get { return _isAlternateImage; }
            set
            {
                if (value == _isAlternateImage) return;
                Program.Client.Rpc.IsAlternateImage(this, value);

                _isAlternateImage = value;
                OnPropertyChanged("Picture");
            }
        }

        internal override int Id
        {
            get { return _id; }
        }

        public override string Name
        {
            get { return FaceUp && _type.Model != null ? _type.Model.Name : "Card"; }
        }

        public string RealName
        {
            get { return _type.Model != null ? _type.Model.Name : "Card"; }
        }

        internal CardIdentity Type
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

        internal bool DeleteWhenLeavesGroup { get; set; }

        public Group Group
        {
            get { return _group; }
            internal set
            {
                if (value == _group) return;
                if (_group != null)
                {
                    // Remove the card from peeking lists
                    foreach (List<Card> lookedCards in _group.LookedAt.Values)
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
                    IsAlternateImage = false;
                    //if (revertToOriginalOnGroupChange) TODO
                    //{ SetModel(_alternateOf); }
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
                new Turn(Player.LocalPlayer, this, value).Do();
                if (value) catchUpOnAlternateSwitches();
            }
        }
        //Okay, someone please explain to me why we have the setter above and the set function below? (V)_V
        internal void SetFaceUp(bool lFaceUp)
        {
            if (_faceUp == lFaceUp) return;
            _faceUp = lFaceUp;
            OnPropertyChanged("FaceUp");
            if (lFaceUp)
            {
                PeekingPlayers.Clear();
                catchUpOnAlternateSwitches();
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
                new Rotate(Player.LocalPlayer, this, value).Do();
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

        public Player TargetedBy { get; private set; }

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
            get { return _playersPeeking; }
        }

        public string Picture
        {
            get
            {
                if (IsAlternateImage)
                    return Type.Model.AlternatePicture;
                if (!FaceUp) return DefaultBack;
                return Type.Model == null ? DefaultFront : Type.Model.Picture;
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
            if (_type.Model == null) return null;
            if (name == "Name") return _type.Model.Name;
            return name == "Id" ? _type.Model.Id : _type.Model.Properties[name];
        }

        public void MoveTo(Group to, bool lFaceUp)
        {
            // Default: move cards to the end of hand, top of table; but top of piles (which is index 0)
            int toIdx = to is Pile ? 0 : to.Cards.Count;
            MoveTo(to, lFaceUp, toIdx);
        }

        public void MoveTo(Group to, bool lFaceUp, int idx)
        {
            if (to == Group && idx < Group.Count && Group[idx] == this) return;
            if (to.Visibility != GroupVisibility.Undefined) lFaceUp = FaceUp;
            Program.Client.Rpc.MoveCardReq(this, to, idx, lFaceUp);
            new MoveCard(Player.LocalPlayer, this, to, idx, lFaceUp).Do();
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

        private static void PeekContinuation(object sender, RevealEventArgs e)
        {
            var identity = (CardIdentity) sender;
            identity.Revealed -= PeekContinuation;
            if (e.NewIdentity.Model == null)
            {
                e.NewIdentity.Revealed += PeekContinuation;
                return;
            }
            Program.TracePlayerEvent(Player.LocalPlayer, "You peeked at {0}.", e.NewIdentity.Model);
        }

        internal string GetPicture(bool up)
        {
            if (IsAlternateImage)
                return Type.Model.AlternatePicture;
            if (!up) return DefaultBack;
            if (Type == null || Type.Model == null) return DefaultFront;
            return Type.Model.Picture;
        }

        internal BitmapImage GetBitmapImage(bool up)
        {
            if (IsAlternateImage)
            {
                var bmp = new BitmapImage(new Uri(Type.Model.AlternatePicture)) {CacheOption = BitmapCacheOption.OnLoad};
                bmp.Freeze();
                return bmp;
            }
            if (!up) return Program.Game.CardBackBitmap;
            if (Type == null || Type.Model == null) return Program.Game.CardFrontBitmap;
            var bmpo = new BitmapImage(new Uri(Type.Model.Picture)) {CacheOption = BitmapCacheOption.OnLoad};
            bmpo.Freeze();
            return bmpo;
        }

        internal void SetOrientation(CardOrientation value)
        {
            if (value == _rot) return;
            _rot = value;
            OnPropertyChanged("Orientation");
        }

        internal void SetOverrideGroupVisibility(bool overrides)
        {
            OverrideGroupVisibility = overrides;
        }

        internal void SetTargetedBy(Player player)
        {
            if (TargetedBy == player) return;
            TargetedBy = player;
            OnPropertyChanged("TargetedBy");
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
#if (DEBUG)
            Debug.WriteLine("SetModel event happened!");
#endif
            bool processSwitches = false;
            if (Type.Model == null) processSwitches = true;//if there is no current model, we've built up unperformed Alternate Switches
            Type.Model = model;
            OnPropertyChanged("Picture");//This should be changed - the model is much more than just the picture.
            if (processSwitches) catchUpOnAlternateSwitches();
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
            return Group.TryToManipulate() && base.TryToManipulate();
        }

        internal void Reveal()
        {
            // Check if the type is already being revealed.
            // This may happen e.g. when moving a card between zones faster than the network latency.
            // It then leads to bugs if not taken good care of.
            if (Type.Revealing) return;

#if (DEBUG)
            Debug.WriteLine("REVEAL event about to fire!");
#endif
            Type.Revealing = true;
            if (!Type.MySecret) return;
            Program.Client.Rpc.Reveal(this, _type.Key, _type.Alias ? Guid.Empty : isAlternate() ? _alternateOf.Id : _type.Model.Id);
        }

        internal void RevealTo(IEnumerable<Player> players)
        {
            // If it's not our secret, we can't reveal it!
            if (!Type.MySecret)
            {
                // If the type is public and it's being revealed to myself,
                // trigger the OnReveal event (e.g. during a Peek of a known face-down card)
                if (Type.Model != null && players.Contains(Player.LocalPlayer))
                    Type.OnRevealed(Type);
                return;
            }

            // If it's an alias pass it to the one who created it
            if (Type.Alias)
            {
                Player p = Player.Find((byte) (Type.Key >> 16));
                Program.Client.Rpc.RevealToReq(p, players.ToArray(), this, Crypto.Encrypt(Type.Key, p.PublicKey));
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

        private readonly ObservableCollection<Marker> _markers = new ObservableCollection<Marker>();

        public IList<Marker> Markers
        {
            get { return _markers; }
        }

        internal void AddMarker(MarkerModel model, ushort count)
        {
            Marker marker = _markers.FirstOrDefault(m => m.Model.Equals(model));
            if (marker != null)
                marker.SetCount((ushort) (marker.Count + count));
            else if (count > 0)
                _markers.Add(new Marker(this, model, count));
        }

        internal void AddMarker(MarkerModel model)
        {
            AddMarker(model, 1);
        }

        internal int RemoveMarker(Marker marker, ushort count)
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

        internal void RemoveMarker(Marker marker)
        {
            _markers.Remove(marker);
        }

        internal Marker FindMarker(Guid lId, string name)
        {
            return _markers.FirstOrDefault(m =>
                                           m.Model.Id == lId &&
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

        internal bool hasProperty(string propertyName)
        {
            return (Type.Model.hasProperty(propertyName));
        }

        /// <summary>
        /// Switches the underlying card model with some predefined Alternate
        /// Returns true if the model was switched (or the switch was recorded to be performed when applicable)
        /// Returns false if it did nothing.
        /// </summary>
        /// <returns></returns>
        internal bool SwitchWithAlternate()
        {//This function will change the underlying Model of a Card to some predefined alternate version.
#if (DEBUG)
            Debug.WriteLine("Attempting to SwitchWithAlternate on " + Name);
#endif
            if (_faceUp)
            {
                if (Type.Model.hasProperty("Alternate"))
                {//if there is an alternate, we want to switch to it
                    if (_alternateOf == null)
                    {//Switching to first alternate
                        _alternateOf = Type.Model;
#if (DEBUG)
                        Debug.WriteLine("Switching for the first time!");
#endif
                    }
                    else
                    {//Not the first, not the last
#if (DEBUG)
                        Debug.WriteLine("Not the first, not the last.");
#endif
                    }
                    SetModel(Database.GetCardById(Type.Model.Alternate));
                    return true;
                }
                //if there is no alternate, we might have reached the end of the chain
                else if (_alternateOf != null)
                {//Then we've come from somewhere, and we want to go back.
                    SetModel(_alternateOf);
                    _alternateOf = null;
#if (DEBUG)
                    Debug.WriteLine("Reached the end of the chain - Going back to the original");
#endif
                    return true;
                }
                //if we don't have a specified alternate, and we haven't come from an alternate, do nothing.
#if (DEBUG)
                Debug.WriteLine("No Alternate, No Original - Doin' Nothin.");
#endif
                return false;
            }
            else //if not face up
            {
                numberOfSwitchWithAlternatesNotPerformed++;//the number of switches
#if (DEBUG)
                Debug.WriteLine("Not FaceUp. Catching the missed switch. New Number: " + numberOfSwitchWithAlternatesNotPerformed);
#endif
                return true;
            }
        }

        public bool isAlternate()
        {
#if (DEBUG)
            Debug.WriteLine(this.Name + " is Alternate? " + (_alternateOf != null));
#endif
            return (_alternateOf != null);//If there is an original version, return true.
        }
        private bool catchUpOnAlternateSwitches()
        {
#if (DEBUG)
            Debug.WriteLine("Time to catch up on the Alternate Switches we missed! Number: " + numberOfSwitchWithAlternatesNotPerformed);
#endif
            try
            {
                while( 0 < numberOfSwitchWithAlternatesNotPerformed)
                {
                    SwitchWithAlternate();
                    numberOfSwitchWithAlternatesNotPerformed--;
                }
                return true;
            }
            catch(Exception e)
            {
                Debug.WriteLine("Unable To Catchup on Alternate Switches at this time. Please try again Later.");
            }
            return false;
        }
    }
}