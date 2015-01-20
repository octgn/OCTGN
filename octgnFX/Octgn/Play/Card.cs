using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octgn.Play.Actions;
using Octgn.Play.Gui;
using Octgn.Utils;
using System.Reflection;

using Octgn.Core.DataExtensionMethods;
using Octgn.Core.Util;
using Octgn.DataNew.Entities;

using log4net;

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
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Static interface

        private static readonly Dictionary<int, Card> All = new Dictionary<int, Card>();

        //public static string DefaultFront
        //{
        //    get { return Program.GameEngine.Definition.DefaultSize.Front; }
        //}

        //public static string DefaultBack
        //{
        //    get { return Program.GameEngine.Definition.DefaultSize.Back; }
        //}

        internal new static Card Find(int id)
        {
            Card res;
            lock (All)
            {
                bool success = All.TryGetValue(id, out res);
                return success ? res : null;
            }
        }

        internal static void Reset()
        {
            lock (All)
                All.Clear();
        }

        internal static void Remove(Card card)
        {
            lock (All)
            {
                All.Remove(card.Id);
            }
        }

        internal static void MoveCardsTo(Group to, Card[] cards, bool[] faceup, bool isScriptMove)
        {
            var idxs = new int[cards.Length];
            if (to is Pile)
            {
                idxs = Enumerable.Repeat(0, cards.Length).ToArray();
            }
            else
            {
                for (var index = 0; index < cards.Length; index++)
                {
                    idxs[index] = to.Cards.Count + index;
                }
            }

            MoveCardsTo(to, cards, faceup, idxs, isScriptMove);
        }

        internal static void MoveCardsTo(Group to, Card[] cards, bool[] faceups, int[] idxs, bool isScriptMove)
        {
            var notMoved = new List<Card>();
            for (var i = 0; i < cards.Length; i++)
            {
                var c = cards[i];
                var lFaceUp = faceups[i];
                //converts negative indexes to count from the bottom for consistency with python behavior
                if (idxs[i] < 0) idxs[i] = to.Count + 1 + idxs[i];
                //over-large indecies reduced to place cards at end of pile
                if (idxs[i] >= to.Count) idxs[i] = to.Count;
                if (idxs[i] < 0) idxs[i] = 0;
                //move skipped if card already at location specified
                if (to == c.Group && idxs[i] < c.Group.Count && c.Group[idxs[i]] == c)
                {
                    notMoved.Add(c);
                    continue;
                }
                if (to.Visibility != GroupVisibility.Undefined) lFaceUp = c.FaceUp;
            }
            Program.Client.Rpc.MoveCardReq(cards.Select(x => x.Id).ToArray(), to, idxs, faceups, isScriptMove);
            new MoveCards(Player.LocalPlayer, cards, to, idxs, faceups, isScriptMove).Do();
            foreach (var c in cards.Where(x => notMoved.Contains(x) == false))
                Program.Client.Rpc.CardSwitchTo(Player.LocalPlayer, c, c.Alternate());
        }

        internal static void MoveCardsTo(Group to, Card[] cards, Action<MoveCardsArgs> it, bool isScriptMove)
        {
            var idxs = new int[cards.Length];
            var fups = new bool[cards.Length];
            MoveCardsArgs prev = null;
            for (var i = 0; i < cards.Length; i++)
            {
                var c = cards[i];
                var cur = new MoveCardsArgs(prev, c);
                it(cur);

                idxs[i] = cur.Index;
                fups[i] = cur.FaceUp;
            }
            MoveCardsTo(to, cards, fups, idxs, isScriptMove);
        }

        public static void MoveCardsToTable(Card[] cards, int[] x, int[] y, bool[] lFaceUp, int[] idx, bool isScriptMove)
        {
            Program.Client.Rpc.MoveCardAtReq(cards.Select(a => a.Id).ToArray(), x, y, idx, isScriptMove, lFaceUp);
            new MoveCards(Player.LocalPlayer, cards, x, y, idx, lFaceUp, isScriptMove).Do();
            foreach (var c in cards)
                Program.Client.Rpc.CardSwitchTo(Player.LocalPlayer, c, c.Alternate());
        }

        public static void MoveCardsToTable(Card[] cards, Action<MoveCardsArgs> it, bool isScriptMove)
        {
            var idxs = new int[cards.Length];
            var fups = new bool[cards.Length];
            var xs = new int[cards.Length];
            var ys = new int[cards.Length];
            MoveCardsArgs prev = null;
            for (var i = 0; i < cards.Length; i++)
            {
                var c = cards[i];
                var cur = new MoveCardsArgs(prev, c);
                it(cur);
                idxs[i] = cur.Index;
                fups[i] = cur.FaceUp;
                xs[i] = cur.X;
                ys[i] = cur.Y;
                prev = cur;
            }
            MoveCardsToTable(cards, xs, ys, fups, idxs, isScriptMove);
        }

        public class MoveCardsArgs
        {
            public MoveCardsArgs Prev { get; private set; }
            public Card Card { get; set; }
            public int Index { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public bool FaceUp { get; set; }

            public MoveCardsArgs(MoveCardsArgs prev, Card card)
            {
                Prev = prev;
                Card = card;
            }
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
        internal Octgn.DataNew.Entities.Card _alternateOf;
        private int numberOfSwitchWithAlternatesNotPerformed = 0;

        private CardOrientation _rot;
        private bool _selected;
        private CardIdentity _type;
        private double _x, _y;
        private bool? _isProxy;
        private bool _cardMoved;

        #endregion Private fields

        internal Card(Player owner, int id, DataNew.Entities.Card model, bool mySecret, string cardsize)            : base(owner)
        {
            _id = id;
            Type = new CardIdentity(id) { Model = model.Clone() };
            // var _definition = def;
            lock (All)
            {
                if (All.ContainsKey(id)) All[id] = this;
                else All.Add(id, this);
            }
            _alternateOf = null;
            numberOfSwitchWithAlternatesNotPerformed = 0;
            _isAlternateImage = false;
            _cardMoved = false;
            Size = Program.GameEngine.Definition.CardSizes[cardsize];
        }

        internal override int Id
        {
            get { return _id; }
        }

        public override string Name
        {
            get { return FaceUp && _type.Model != null ? _type.Model.PropertyName() : "Card"; }
        }

        public string RealName
        {
            get { return _type.Model != null ? _type.Model.PropertyName() : "Card"; }
        }
        public bool CardMoved
        {
            get { return _cardMoved; }
            set
            {
                _cardMoved = value;
            }
        }

        public CardSize Size { get; set; }

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
                        Program.GameMess.Warning("The same card identity is used for two different cards!");
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
                this.SwitchTo(Player.LocalPlayer);
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
            }
        }

        public bool OverrideGroupVisibility { get; private set; }

        public static Card[] AllCards()
        {
            lock (All)
            {
                return All.Select(x => x.Value).ToArray();
            }
        }

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

        public bool Anchored
        {
            get { return _anchored; }
        }

        public void SetAnchored(bool networked, bool anchored)
        {
            if (anchored == _anchored) return;
            if (!networked)
                Program.Client.Rpc.AnchorCard(this, Player.LocalPlayer, anchored);
            _anchored = anchored;
            OnPropertyChanged("Anchored");
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

        private string sleeveUrl;

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
        public string HighlightColorString
        {
            get
            {
                Color? colorOrNull = _highlight;
                if (colorOrNull == null) return "None";
                Color color = colorOrNull.Value;
                return string.Format("#{0:x2}{1:x2}{2:x2}", color.R, color.G, color.B);
            }
        }

        public bool IsHighlighted
        {
            get { return _selected || _highlight != null; }
        }

        public bool IsProxy()
        {
            if (_isProxy == null)
            {
                _isProxy = Type.Model.GetPicture().Equals(Type.Model.GetProxyPicture());
            }

            return _isProxy.GetValueOrDefault();
        }

        public ObservableCollection<Player> PeekingPlayers
        {
            get { return _playersPeeking; }
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

        public string[] Alternates()
        {
            if (_type.Model == null) return new string[0];
            return _type.Model.Properties.Select(x => x.Key).ToArray();
        }

        public string Alternate()
        {
            return this._type.Model == null ? "" : this._type.Model.Alternate;
        }

        public void SwitchTo(Player player, string alternate = "", bool notifyServer = true)
        {
            if (_type.Model == null) return;
            if (_type.Model.Alternate.ToLower() == alternate.ToLower()) return;
            if (player.Id == Player.LocalPlayer.Id)
                if (notifyServer)
                    Program.Client.Rpc.CardSwitchTo(player, this, alternate);
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

        public void MoveTo(Group to, bool lFaceUp, bool isScriptMove)
        {
            // Default: move cards to the end of hand, top of table; but top of piles (which is index 0)
            int toIdx = to is Pile ? 0 : to.Cards.Count;
            MoveTo(to, lFaceUp, toIdx, isScriptMove);
        }

        public void MoveTo(Group to, bool lFaceUp, int idx, bool isScriptMove)
        {
            //converts negative indexes to count from the bottom for consistency with python behavior
            if (idx < 0) idx = to.Count + 1 + idx;
            //over-large indecies reduced to place cards at end of pile
            if (idx >= to.Count) idx = to.Count;
            if (idx < 0) idx = 0;
            //move skipped if card already at location specified
            if (to == Group && idx < Group.Count && Group[idx] == this) return;
            if (to.Visibility != GroupVisibility.Undefined) lFaceUp = FaceUp;
            var cards = new Card[1] { this };
            Program.Client.Rpc.MoveCardReq(cards.Select(x => x.Id).ToArray(), to, new[] { idx }, new[] { lFaceUp }, isScriptMove);
            new MoveCards(Player.LocalPlayer, cards, to, new[] { idx }, new[] { lFaceUp }, isScriptMove).Do();
            Program.Client.Rpc.CardSwitchTo(Player.LocalPlayer, this, this.Alternate());
        }

        public void MoveToTable(int x, int y, bool lFaceUp, int idx, bool isScriptMove)
        {
            Program.Client.Rpc.MoveCardAtReq(new[] { Id }, new[] { x }, new[] { y }, new[] { idx }, isScriptMove, new[] { lFaceUp });
            new MoveCards(Player.LocalPlayer, new[] { this }, new[] { x }, new[] { y }, new[] { idx }, new[] { lFaceUp }, isScriptMove).Do();
            Program.Client.Rpc.CardSwitchTo(Player.LocalPlayer, this, this.Alternate());
        }

        public void MoveToTableRawStyle(int x, int y, bool lFaceUp, int idx, bool isScriptMove)
        {
            new MoveCards(Player.LocalPlayer, new[] { this }, new[] { x }, new[] { y }, new[] { idx }, new[] { lFaceUp }, isScriptMove, true).Do();
        }

        public int GetIndex()
        {
            return Group.GetCardIndex(this);
        }

        public void SetIndex(int idx)
        {
            if (Group != null)
                Group.SetCardIndex(this, idx);
        }

        public void Peek()
        {
            if (FaceUp) return;
            if (!PeekingPlayers.Contains(Player.LocalPlayer))
                PeekingPlayers.Add(Player.LocalPlayer);
            Program.Client.Rpc.PeekReq(this);
            Program.GameMess.PlayerEvent(Player.LocalPlayer, "peeked at {0}.", this.Type.Model);
        }

        internal BitmapImage GetBitmapImage(bool up, bool proxyOnly = false)
        {
            if (!up)
            {
                if (string.IsNullOrWhiteSpace(sleeveUrl))					return Program.GameEngine.GetCardBack(this.Size.Name);                BitmapImage b = null;
                Library.X.Instance.Try(() => b = ImageUtils.CreateFrozenBitmap(new Uri(sleeveUrl)));
                if (b == null)
                    return Program.GameEngine.GetCardBack(this.Size.Name);
                return b;
            }
            if (Type == null || Type.Model == null) return Program.GameEngine.GetCardFront(this.Size.Name);
            BitmapImage bmpo = null;
            Octgn.Library.X.Instance.Try(() =>
            {
                //Uri imgUrl = null;
                //imgUrl = proxyOnly ? new Uri(Type.Model.GetProxyPicture()) : new Uri(Type.Model.GetPicture());
                ImageUtils.GetCardImage(Type.Model, x => bmpo = x,proxyOnly);
            });

            return bmpo ?? Program.GameEngine.GetCardFront(this.Size.Name);
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
                    break;
                case GroupVisibility.Undefined:
                    //if (FaceUp) Reveal();
                    break;
                case GroupVisibility.Custom:
                    SetFaceUp(viewers.Contains(Player.LocalPlayer));
                    //RevealTo(viewers);
                    break;
                default: // could be GroupVisibilty.Owner
                    Debug.Fail("[Card.SetVisibility] Invalid visibility!");
                    return;
            }
        }

        internal void SetModel(DataNew.Entities.Card model)
        {
            Log.Info("SetModel event happened!");
            Type.Model = model;
			// Not sure if we should update the card size here...not sure if it's necesary.
            OnPropertyChanged("Picture");//This should be changed - the model is much more than just the picture.
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
            //Tooltip.PopupError("You don't control this card.");
        }

        internal override bool TryToManipulate()
        {
            // FIX (jods): Containing group has to be manipulable as well,
            // e.g. during a shuffle a pile is locked
            return Group.TryToManipulate() && base.TryToManipulate();
        }

        #region Comparers

        #region Nested type: NameComparer

        public class NameComparer : IComparer
        {
            #region IComparer Members

            public int Compare(object x, object y)
            {
                return String.CompareOrdinal(((Card)x).Name, ((Card)y).Name);
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
                return String.CompareOrdinal(((Card)x).RealName, ((Card)y).RealName);
            }

            #endregion
        }

        #endregion

        #endregion Comparers

        #region Markers

        private readonly ObservableCollection<Marker> _markers = new ObservableCollection<Marker>();
        private readonly List<Marker> _removedMarkers = new List<Marker>();
        private bool _anchored;

        public IList<Marker> Markers
        {
            get { return _markers; }
        }

        public string MarkersString
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("{");
                if (_markers.Count > 0)
                {
                    int counter = 0;
                    foreach (Marker m in _markers)
                    {
                        sb.AppendFormat("{0}:{1}", m.Model.ModelString(), m.Count);
                        counter++;
                        if (counter != _markers.Count) sb.Append(",");
                    }
                }
                sb.Append("}");
                return sb.ToString();
            }
        }
        //public Dictionary<Tuple<string,string>,int> MarkersDict
        //{
        //    get
        //    {
        //        Dictionary<Tuple<string, string>, int> markertuple = new Dictionary<Tuple<string,string>,int>();
        //        if (_markers.Count > 0)
        //        {
        //            foreach (Marker m in _markers)
        //            {
        //                Tuple<string, string> key = Tuple.Create<string, string>(m.Model.Name, m.Model.Id.ToString());
        //                markertuple.Add(key, m.Count);
        //            }
        //        }
        //        return markertuple;
        //    }
        //}
        internal void AddMarker(DataNew.Entities.Marker model, ushort count)
        {
            Marker marker = _markers.FirstOrDefault(m => m.Model.Equals(model));
            if (marker != null)
                marker.SetCount((ushort)(marker.Count + count));
            else if (count > 0)
                _markers.Add(new Marker(this, model, count));
        }

        internal void AddMarker(DataNew.Entities.Marker model)
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
                _removedMarkers.Add(marker);
                return realCount;
            }

            marker.SetCount((ushort)(marker.Count - count));
            return count;
        }

        internal void RemoveMarker(Marker marker)
        {
            _markers.Remove(marker);
            _removedMarkers.Add(marker);
        }

        internal Marker FindMarker(Guid lId, string name)
        {
            return _markers.FirstOrDefault(m =>
                                           m.Model.Id == lId &&
                                           (!(m.Model is DefaultMarkerModel) || m.Model.Name == name));
        }

        internal Marker FindRemovedMarker(Guid lId, string name)
        {
            return _removedMarkers.FirstOrDefault(m =>
                                           m.Model.Id == lId &&
                                           (!(m.Model is DefaultMarkerModel) || m.Model.Name == name));
        }

        internal void SetMarker(Player player, Guid lId, string name, int count, bool notify = true)
        {
            int oldCount = 0;
            Marker marker = FindMarker(lId, name);
            if (marker != null)
            {
                oldCount = marker.Count;
                marker.SetCount((ushort)count);
            }
            else if (count > 0)
            {
                DataNew.Entities.Marker model = Program.GameEngine.GetMarkerModel(lId);
                var defaultMarkerModel = model as DefaultMarkerModel;
                if (defaultMarkerModel != null)
                    (defaultMarkerModel).SetName(name);
                AddMarker(model, (ushort)count);
            }
        }



        #endregion Markers

        internal bool hasProperty(string propertyName)
        {
            return (Type.Model.HasProperty(propertyName));
        }

        public void SetSleeve(int sleeveId)
        {
            try
            {
                var sleeve = SleeveManager.Instance.SleeveFromId(sleeveId);
                if (sleeve != null)
                {
                    SetSleeve(sleeve.Url);
                }
            }
            catch (Exception e)
            {
                Log.Warn("SetSleeve " + sleeveId, e);
            }
        }

        public void SetSleeve(string pleeveUrl)
        {
            try
            {
                this.sleeveUrl = pleeveUrl;
                OnPropertyChanged("FaceUp");
            }
            catch (Exception e)
            {
                Log.Warn("SetSleeve " + (sleeveUrl ?? ""), e);
                throw;
            }
        }
    }
}