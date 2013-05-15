using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Octgn.Extentions;

namespace Octgn.Play
{
    using System.Windows.Documents;

    using Octgn.DataNew.Entities;
    using Octgn.PlayTable;
    using Octgn.PlayTable.Controls;

    public abstract class Group : ControllableObject, IPlayGroup
    {
        #region Non-public fields

        private static readonly KeyGestureConverter KeyConverter = new KeyGestureConverter();

        // List of cards in this group        

        internal DataNew.Entities.Group Def;
        public int FilledShuffleSlots { get; set; }
        public bool HasReceivedFirstShuffledMessage { get; set; }

        // when a group is locked, one cannot manipulate it anymore (e.g. during shuffles and other non-atomic actions)

        public Dictionary<int, List<IPlayCard>> LookedAt
        {
            get
            {
                return this.lookedAt;
            }
            internal set
            {
                this.lookedAt = value;
            }
        }

        internal Dictionary<int, List<IPlayCard>> lookedAt = new Dictionary<int, List<IPlayCard>>();
        // Cards being looked at, key is a unique identifier for each "look"; Note: cards may have left the group in the meantime, which is not important
        
        // Stores positions suggested by this client during a shuffle [transient]
        public short[] MyShufflePos { get; set; }

        public List<IPlayPlayer> Viewers
        {
            get
            {
                return viewers;
            }
            set
            {
                viewers = value;
            }
        }
        private  List<IPlayPlayer> viewers = new List<IPlayPlayer>(2);
        private bool _locked;
        protected ObservableCollection<IPlayCard> cards = new ObservableCollection<IPlayCard>();
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
                MoveToShortcut = (KeyGesture)KeyConverter.ConvertFromInvariantString(def.Shortcut);
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
        public GroupVisibility Visibility
        {
            get { return visibility; }
        }

        // Is this group ordered ?
        public bool Ordered
        {
            get { return Def.Ordered; }
        }

        public ObservableCollection<IPlayCard> Cards
        {
            get { return cards; }
        }

        // Get a card in the group
        public IPlayCard this[int idx]
        {
            get { return cards[idx]; }
        }

        public int Count
        {
            get { return cards.Count; }
        }

        // Add a card to the group
        public void AddAt(IPlayCard card, int idx)
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
        public void Remove(IPlayCard card)
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
        public bool PreparingShuffle { get; set; }

        // True if the localPlayer is the one who wants to shuffle        
        public bool WantToShuffle { get; set; }

        // Get the Id of this group
        public override int Id
        {
            get { return 0x01000000 | (Owner == null ? 0 : Owner.Id << 16) | Def.Id; }
        }

        public bool Locked
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

        public event EventHandler<TraceEventArgs> ShuffledTrace;

        public event EventHandler Shuffled;

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

        public override void OnControllerChanged()
        {
            foreach (IPlayCard c in cards) CopyControllersTo(c);
        }

        public override bool CanManipulate()
        {
            return !WantToShuffle && base.CanManipulate();
        }

        public override bool TryToManipulate()
        {
            if (WantToShuffle)
            {
                Tooltip.PopupError("Wait until shuffle completes.");
                return false;
            }
            return base.TryToManipulate();
        }

        public override void NotControlledError()
        {
            Tooltip.PopupError("You don't control this group.");
        }

        public void FreezeCardsVisibility(bool notifyServer)
        {
            if (notifyServer)
                Program.Client.Rpc.FreezeCardsVisibility(this);
        }

        public void SetVisibility(bool? visible, bool notifyServer)
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
                Viewers.AddRange(Program.Player.All);
            }
            else
            {
                visibility = GroupVisibility.Nobody;
                Viewers.Clear();
            }

            foreach (IPlayCard c in cards.Where(c => !c.OverrideGroupVisibility))
                c.SetVisibility(visibility, Viewers);
        }

        public void AddViewer(IPlayPlayer player, bool notifyServer)
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
            foreach (IPlayCard c in cards.Where(c => !c.OverrideGroupVisibility))
                c.SetVisibility(visibility, Viewers);
        }

        public void RemoveViewer(IPlayPlayer player, bool notifyServer)
        {
            if (!Viewers.Contains(player)) return;
            if (notifyServer)
                Program.Client.Rpc.GroupVisRemoveReq(this, player);
            Viewers.Remove(player);
            visibility = Viewers.Count == 0 ? GroupVisibility.Nobody : GroupVisibility.Custom;
            if (player == Program.Player.LocalPlayer)
                foreach (IPlayCard c in cards)
                    c.SetFaceUp(false);
        }

        public void OnShuffled()
        {
            // Remove player looking at the cards, if any (doesn't remove the need to remove those from the dictionary!)
            foreach (List<IPlayCard> list in LookedAt.Values)
                list.Clear();
            foreach (IPlayCard c in Cards)
                c.PlayersLooking.Clear();

            // Notify trace event listeners
            var shuffledArgs = new TraceEventArgs { TraceNotification = true };
            if (ShuffledTrace != null)
                ShuffledTrace(this, shuffledArgs);
            // Trace if required
            if (shuffledArgs.TraceNotification)
                Program.Trace.TracePlayerEvent(Owner, "{0} is shuffled", FullName);

            WantToShuffle = Locked = false;

            // Notify completion (e.g. to resume scripts execution)
            if (Shuffled != null)
                Shuffled(this, EventArgs.Empty);
        }

        public void SetCardIndex(IPlayCard card, int idx)
        {
            int currentIdx = cards.IndexOf(card);
            if (currentIdx == idx || currentIdx == -1) return;
            if (idx >= cards.Count) idx = cards.Count - 1;
            cards.Move(currentIdx, idx);
        }

        public int GetCardIndex(IPlayCard card)
        {
            return cards.IndexOf(card);
        }

        public IPlayCard FindByCardIdentity(CardIdentity identity)
        {
            return cards.FirstOrDefault(c => c.Type == identity);
        }

        public void ResetVisibility()
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
        public void Reset()
        {
            cards.Clear();
            ResetVisibility();
        }

        public int FindNextFreeSlot(int slot)
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

        public IEnumerator<IPlayCard> GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cards.GetEnumerator();
        }

        #endregion
    }

    public class ActionShortcut
    {
        public KeyGesture Key { get; set; }
        public IGroupAction ActionDef { get; set; }
    }

    internal class ShuffleTraceChatHandler : ITraceChatHandler
    {
        public Inline Line { get; set; }

        public void Set(string message)
        {
            Line = new Run(message);
        }

        public void ReplaceText(object sender, TraceEventArgs e)
        {
            e.TraceNotification = false;
            var group = (IPlayGroup)sender;
            group.ShuffledTrace -= this.ReplaceText;
            var run = (Run)this.Line;
            run.Text = string.Format("{0} is shuffled", group.FullName);
        }
    }

    public class ShuffleTraceEventArgs : EventArgs
    {
    }
}