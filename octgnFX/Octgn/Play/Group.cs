using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using Octgn.Controls;
using Octgn.Definitions;
using Octgn.Utils;

namespace Octgn.Play
{
    public abstract class Group : ControllableObject, IEnumerable<Card>
    {
        #region Non-public fields

        private static readonly KeyGestureConverter KeyConverter = new KeyGestureConverter();

        // TODO: Should be Cards
        // List of cards in this group        

        internal GroupDef Def;
        internal int FilledShuffleSlots;
        internal bool HasReceivedFirstShuffledMessage;

        // when a group is locked, one cannot manipulate it anymore (e.g. during shuffles and other non-atomic actions)

        internal Dictionary<int, List<Card>> LookedAt = new Dictionary<int, List<Card>>();
        // Cards being looked at, key is a unique identifier for each "look"; Note: cards may have left the group in the meantime, which is not important

        internal short[] MyShufflePos; // Stores positions suggested by this client during a shuffle [transient]

        internal List<Player> Viewers = new List<Player>(2);
        private bool _locked;
        protected ObservableCollection<Card> cards = new ObservableCollection<Card>();
        // List of players who can see cards in this group, or null where this is irrelevant (Visibility.Undefined)

        // TODO: Should be Visibility 
        protected GroupVisibility visibility; // Visibility of the group

        #endregion

        #region Public interface

        // Find a group given its id

        // C'tor
        public readonly ActionShortcut[] CardShortcuts;
        public readonly ActionShortcut[] GroupShortcuts;

        internal Group(Player owner, GroupDef def)
            : base(owner)
        {
            Def = def;
            ResetVisibility();
            GroupShortcuts = CreateShortcuts(def.GroupActions);
            CardShortcuts = CreateShortcuts(def.CardActions);
            if (def.Shortcut != null)
                MoveToShortcut = (KeyGesture) KeyConverter.ConvertFromInvariantString(def.Shortcut);
        }

        public GroupDef Definition
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
            get { return cards[idx]; }
        }

        public int Count
        {
            get { return cards.Count; }
        }

        internal new static Group Find(int id)
        {
            if (id == 0x01000000) return Program.Game.Table;
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
            cards.Insert(idx, card);
        }

        // Remove a card from the group
        public void Remove(Card card)
        {
            if (!cards.Contains(card)) return;
            cards.Remove(card);
            card.Group = null;
        }

        public override string ToString()
        {
            return FullName;
        }

        #endregion

        #region Implementation

        // True if a UnaliasGrp message was received
        internal bool PreparingShuffle;

        // True if the localPlayer is the one who wants to shuffle        
        internal bool WantToShuffle;

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
        internal event EventHandler Shuffled;

        private static ActionShortcut[] CreateShortcuts(IEnumerable<BaseActionDef> baseActionDef)
        {
            if (baseActionDef == null) return new ActionShortcut[0];

            IEnumerable<ActionDef> actionDef = baseActionDef
                .Flatten(x =>
                             {
                                 var y = x as ActionGroupDef;
                                 return y == null ? null : y.Children;
                             })
                .OfType<ActionDef>();

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
            foreach (Card c in cards) CopyControllersTo(c);
        }

        internal override bool CanManipulate()
        {
            return !WantToShuffle && base.CanManipulate();
        }

        internal override bool TryToManipulate()
        {
            if (WantToShuffle)
            {
                Tooltip.PopupError("Wait until shuffle completes.");
                return false;
            }
            return base.TryToManipulate();
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

            foreach (Card c in cards.Where(c => !c.OverrideGroupVisibility))
                c.SetVisibility(visibility, Viewers);
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
            foreach (Card c in cards.Where(c => !c.OverrideGroupVisibility))
                c.SetVisibility(visibility, Viewers);
        }

        internal void RemoveViewer(Player player, bool notifyServer)
        {
            if (!Viewers.Contains(player)) return;
            if (notifyServer)
                Program.Client.Rpc.GroupVisRemoveReq(this, player);
            Viewers.Remove(player);
            visibility = Viewers.Count == 0 ? GroupVisibility.Nobody : GroupVisibility.Custom;
            if (player == Player.LocalPlayer)
                foreach (Card c in cards)
                    c.SetFaceUp(false);
        }

        internal void OnShuffled()
        {
            // Remove player looking at the cards, if any (doesn't remove the need to remove those from the dictionary!)
            foreach (var list in LookedAt.Values)
                list.Clear();
            foreach (Card c in Cards)
                c.PlayersLooking.Clear();

            // Notify trace event listeners
            var shuffledArgs = new ShuffleTraceEventArgs {TraceNotification = true};
            if (ShuffledTrace != null)
                ShuffledTrace(this, shuffledArgs);
            // Trace if required
            if (shuffledArgs.TraceNotification)
                Program.TracePlayerEvent(Owner, "{0} is shuffled", FullName);

            WantToShuffle = Locked = false;

            // Notify completion (e.g. to resume scripts execution)
            if (Shuffled != null)
                Shuffled(this, EventArgs.Empty);
        }

        internal void SetCardIndex(Card card, int idx)
        {
            int currentIdx = cards.IndexOf(card);
            if (currentIdx == idx || currentIdx == -1) return;
            if (idx >= cards.Count) idx = cards.Count - 1;
            cards.Move(currentIdx, idx);
        }

        internal int GetCardIndex(Card card)
        {
            return cards.IndexOf(card);
        }

        internal Card FindByCardIdentity(CardIdentity identity)
        {
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
        }

        // Hard-reset: removes all cards from this group, without destroying them
        internal void Reset()
        {
            cards.Clear();
            ResetVisibility();
        }

        internal int FindNextFreeSlot(int slot)
        {
            for (int i = slot + 1; i != slot; ++i)
            {
                if (i >= cards.Count) i = 0;
                if (cards[i].Type == null) return i;
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
        public ActionDef ActionDef { get; set; }
    }
}