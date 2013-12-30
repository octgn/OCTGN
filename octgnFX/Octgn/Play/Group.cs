using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using Octgn.Controls;
using Octgn.Extentions;

namespace Octgn.Play
{
    using Octgn.DataNew.Entities;

    public abstract class Group : ControllableObject, IEnumerable<Card>
    {
        #region Non-public fields

        private static readonly KeyGestureConverter KeyConverter = new KeyGestureConverter();

        // List of cards in this group        

        internal DataNew.Entities.Group Def;

        // when a group is locked, one cannot manipulate it anymore (e.g. during shuffles and other non-atomic actions)

        internal Dictionary<int, List<Card>> LookedAt = new Dictionary<int, List<Card>>();
        // Cards being looked at, key is a unique identifier for each "look"; Note: cards may have left the group in the meantime, which is not important

        internal List<Player> Viewers = new List<Player>(2);
        private bool _locked;
        protected ObservableCollection<Card> cards = new ObservableCollection<Card>();
        // List of players who can see cards in this group, or null where this is irrelevant (Visibility.Undefined)

        protected GroupVisibility visibility; // Visibility of the group

        #endregion

        #region Public interface

        // Find a group given its id

        // C'tor
        public readonly ActionShortcut[] CardShortcuts;
        public readonly ActionShortcut[] GroupShortcuts;

        internal Group(Player owner, DataNew.Entities.Group def)
            : base(owner)
        {
            Def = def;
            ResetVisibility();
            GroupShortcuts = CreateShortcuts(def.GroupActions);
            CardShortcuts = CreateShortcuts(def.CardActions);
            if (def.Shortcut != null)
                MoveToShortcut = (KeyGesture) KeyConverter.ConvertFromInvariantString(def.Shortcut);
        }

        public DataNew.Entities.Group Definition
        {
            get { return Def; }
        }

        // Group name
        public override string Name
        {
            get { return Def.Name; }
        }

        public KeyGesture MoveToShortcut { get; private set; }

        // Are cards visible when they arrive in this group ?                
        internal GroupVisibility Visibility
        {
            get { return visibility; }
			set
			{
			    visibility = value;
			}
        }

        // Is this group ordered ?
        public bool Ordered
        {
            get { return Def.Ordered; }
        }

        public ObservableCollection<Card> Cards
        {
            get { return cards; }
        }

        // Get a card in the group
        public Card this[int idx]
        {
            get
            {
				lock(cards)
					return cards[idx];
            }
        }

        public int Count
        {
            get
            {
				lock(cards)
					return cards.Count;
            }
        }

        public virtual void OnCardsChanged()
        {

        }

        internal new static Group Find(int id)
        {
            if (id == 0x01000000) return Program.GameEngine.Table;
            Player player = Player.Find((byte) (id >> 16));
            return player.IndexedGroups[(byte) id];
        }

        // Add a card to the group
        public void AddAt(Card card, int idx)
        {
            // Restore default orientation
            card.SetOrientation(CardOrientation.Rot0);

            // Set the card controllers
            CopyControllersTo(card);

            // Assign default visibility
            card.SetVisibility(visibility, Viewers);

            // Add the card to the group
            card.Group = this;
            lock (cards)
            {
                if (idx < 0 || (cards.Count == 0 && idx != 0) || (cards.Count > 0 && idx > cards.Count))
                {
                    Program.TraceWarning("Can't add card at index {0}, there is not a free slot there.", idx);
                    return;
                }
                cards.Insert(idx, card);
            }
            OnCardsChanged();
        }

        public void Add(Card card)
        {
            // Restore default orientation
            card.SetOrientation(CardOrientation.Rot0);

            // Set the card controllers
            CopyControllersTo(card);

            // Assign default visibility
            card.SetVisibility(visibility, Viewers);

            // Add the card to the group
            card.Group = this;
            lock (cards)
            {
                if (this.cards.Any(x => x.Id == card.Id) == false) cards.Add(card);
            }
            OnCardsChanged();
        }

        // Remove a card from the group
        public void Remove(Card card)
        {
            lock (cards)
            {
                if (!cards.Contains(card)) return;
                cards.Remove(card);
                card.Group = null;
            }
            OnCardsChanged();
        }

        public override string ToString()
        {
            return FullName;
        }

        #endregion

        #region Implementation

        // Get the Id of this group
        internal override int Id
        {
            get { return 0x01000000 | (Owner == null ? 0 : Owner.Id << 16) | Def.Id; }
        }

        internal bool Locked
        {
            get { return _locked; }
            set
            {
                if (_locked == value) return;
                _locked = value;
                if (value) KeepControl();
                else ReleaseControl();
            }
        }

        internal event EventHandler<ShuffleTraceEventArgs> ShuffledTrace;

        private static ActionShortcut[] CreateShortcuts(IEnumerable<IGroupAction> baseActionDef)
        {
            if (baseActionDef == null) return new ActionShortcut[0];

            IEnumerable<GroupAction> actionDef = baseActionDef
                .Flatten(x =>
                             {
                                 var y = x as GroupActionGroup;
                                 return y == null ? null : y.Children;
                             })
                .OfType<GroupAction>();

            IEnumerable<ActionShortcut> shortcuts = from action in actionDef
                                                    where action.Shortcut != null
                                                    select new ActionShortcut
                                                               {
                                                                   Key =
                                                                       (KeyGesture)
                                                                       KeyConverter.ConvertFromInvariantString(
                                                                           action.Shortcut),
                                                                   ActionDef = action
                                                               };
            return shortcuts.ToArray();
        }

        protected override void OnControllerChanged()
        {
			lock(cards)
				foreach (Card c in cards) CopyControllersTo(c);
            OnCardsChanged();
        }

        internal override void NotControlledError()
        {
            Tooltip.PopupError("You don't control this group.");
        }

        internal void FreezeCardsVisibility(bool notifyServer)
        {
            if (notifyServer)
                Program.Client.Rpc.FreezeCardsVisibility(this);
        }

        internal void SetVisibility(bool? visible, bool notifyServer)
        {
            if (notifyServer)
                Program.Client.Rpc.GroupVisReq(this, visible.HasValue, visible.GetValueOrDefault());
            if (!visible.HasValue)
            {
                visibility = GroupVisibility.Undefined;
                Viewers.Clear();
            }
            else if (visible.Value)
            {
                visibility = GroupVisibility.Everybody;
                Viewers.Clear();
                Viewers.AddRange(Player.All);
            }
            else
            {
                visibility = GroupVisibility.Nobody;
                Viewers.Clear();
            }

            lock (cards)
            {
                foreach (Card c in cards.Where(c => !c.OverrideGroupVisibility)) 
                    c.SetVisibility(visibility, Viewers);
            }
            OnCardsChanged();
        }

        internal void AddViewer(Player player, bool notifyServer)
        {
            if (visibility != GroupVisibility.Custom)
            {
                visibility = GroupVisibility.Custom;
                Viewers.Clear();
            }
            else if (Viewers.Contains(player)) return;
            if (notifyServer)
                Program.Client.Rpc.GroupVisAddReq(this, player);
            visibility = GroupVisibility.Custom;
            Viewers.Add(player);
            lock (cards)
            {
                foreach (Card c in cards.Where(c => !c.OverrideGroupVisibility))
                    c.SetVisibility(visibility, Viewers);
                
            } OnCardsChanged();
        }

        internal void RemoveViewer(Player player, bool notifyServer)
        {
            if (!Viewers.Contains(player)) return;
            if (notifyServer)
                Program.Client.Rpc.GroupVisRemoveReq(this, player);
            Viewers.Remove(player);
            visibility = Viewers.Count == 0 ? GroupVisibility.Nobody : GroupVisibility.Custom;
            if (player == Player.LocalPlayer)
                lock (cards)
                {
                    foreach (Card c in cards)
                        c.SetFaceUp(false);
                    
                } 
            OnCardsChanged();
        }

        internal void OnShuffled()
        {
            // Remove player looking at the cards, if any (doesn't remove the need to remove those from the dictionary!)
            foreach (List<Card> list in LookedAt.Values)
                list.Clear();
            foreach (Card c in Cards)
            {
                c.PlayersLooking.Clear();
                c.PeekingPlayers.Clear();
            }

            // Notify trace event listeners
            var shuffledArgs = new ShuffleTraceEventArgs {TraceNotification = true};
            if (ShuffledTrace != null)
                ShuffledTrace(this, shuffledArgs);
            // Trace if required
            if (shuffledArgs.TraceNotification)
                Program.TracePlayerEvent(Owner, "{0} is shuffled", FullName);
            OnCardsChanged();
        }

        internal void SetCardIndex(Card card, int idx)
        {
            lock (cards)
            {
                int currentIdx = cards.IndexOf(card);
                if (currentIdx == idx || currentIdx == -1) return;
                if (idx >= cards.Count) idx = cards.Count - 1;
                cards.Move(currentIdx, idx);
            }
            OnCardsChanged();
        }

        internal int GetCardIndex(Card card)
        {
			lock(cards)
				return cards.IndexOf(card);
        }

        internal Card FindByCardIdentity(CardIdentity identity)
        {
			lock(cards)
				return cards.FirstOrDefault(c => c.Type == identity);
        }

        private void ResetVisibility()
        {
            Viewers.Clear();
            if (Def.Visibility == GroupVisibility.Owner)
            {
                visibility = GroupVisibility.Custom;
                Viewers.Add(Owner);
            }
            else
                visibility = Def.Visibility;
            OnCardsChanged();
        }

        // Hard-reset: removes all cards from this group, without destroying them
        internal void Reset()
        {
            cards.Clear();
            ResetVisibility();
            OnCardsChanged();
        }

        internal int FindNextFreeSlot(int slot)
        {
            lock (cards)
            {
                for (int i = slot + 1; i != slot; ++i)
                {
                    if (i >= cards.Count) i = 0;
                    if (cards[i].Type == null) return i;
                }
            }
            throw new InvalidOperationException("There's no more free slot!");
        }

        #endregion

        #region IEnumerable<Card> Members

        public IEnumerator<Card> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        #endregion
    }

    public class ShuffleTraceEventArgs : EventArgs
    {
        public bool TraceNotification { get; set; }
    }

    internal class ShuffleTraceChatHandler
    {
        public Inline Line { get; set; }

        public void ReplaceText(object sender, ShuffleTraceEventArgs e)
        {
            e.TraceNotification = false;
            var group = (Group) sender;
            group.ShuffledTrace -= ReplaceText;
            var run = (Run) Line;
            run.Text = string.Format("{0} is shuffled", group.FullName);
        }
    }

    public class ActionShortcut
    {
        public KeyGesture Key { get; set; }
        public IGroupAction ActionDef { get; set; }
    }
}