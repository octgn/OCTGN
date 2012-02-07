using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Play.Gui
{
    static class Selection
    {
        private static List<Card> selected = new List<Card>(4);

        public static Group Source { get; private set; }

        public static void Add(Card c)
        {
            // Only allow selection from one group at a time
            if (Source != c.Group)
            {
                Source = c.Group;
                Clear();
            }
            if (c.Selected) return;
            c.Selected = true;
            selected.Add(c);
        }

        public static void Remove(Card c)
        {
            if (!c.Selected) return;
            c.Selected = false;
            selected.Remove(c);
        }

        public static void RemoveAll(Predicate<Card> match)
        {
            selected.ForEach(c => c.Selected = !match(c));
            selected.RemoveAll(match);
        }

        public static void Clear()
        {
            selected.ForEach(c => c.Selected = false);
            selected.Clear();
        }

        public static bool IsEmpty()
        { return selected.Count == 0; }

        public static void ForEach(Action<Card> action)
        { selected.ForEach(action); }

        public static void ForEachModifiable(Action<Card> action)
        {
            for (int i = selected.Count - 1; i >= 0; --i)
                action(selected[i]);
        }

        public static IEnumerable<CardControl> GetCardControls(GroupControl ctrl)
        {
            if (IsEmpty()) yield break;
            var groupCards = ctrl.Group.Cards;
            var generator = ctrl.GetItemContainerGenerator();
            for (int i = 0; i < groupCards.Count; ++i)
                if (groupCards[i].Selected)
                {
                    var container = generator.ContainerFromIndex(i);
                    var cardCtrl = (CardControl)System.Windows.Media.VisualTreeHelper.GetChild(container, 0);
                    yield return cardCtrl;
                }
        }

        public static IEnumerable<CardControl> GetCardControls(GroupControl ctrl, CardControl empty)
        {
            if (IsEmpty())
                yield return empty;
            else
                foreach (var cardCtrl in GetCardControls(ctrl))
                    yield return cardCtrl;
        }

        public static IEnumerable<Card> ExtendToSelection(Card card)
        {
            if (selected.Contains(card)) return selected;
            return Enumerable.Repeat(card, 1);
        }

        public static IEnumerable<Card> Cards
        { get { return selected; } }

        public static void Do(Action<Card> action, Card card)
        {
            // Because some actions may modify the selection (which breaks the enumeration),
            // we make a local copy of the selection
            var cards = ExtendToSelection(card).ToList();
            foreach (Card c in cards)
                action(c);
        }
    }
}
