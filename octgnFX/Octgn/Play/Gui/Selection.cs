using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Octgn.Play.Gui
{
    internal static class Selection
    {
        private static readonly List<Card> Selected = new List<Card>(4);

        public static Group Source { get; private set; }

        public static IEnumerable<Card> Cards
        {
            get { return Selected; }
        }

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
            Selected.Add(c);
        }

        public static void Remove(Card c)
        {
            if (!c.Selected) return;
            c.Selected = false;
            Selected.Remove(c);
        }

        public static void RemoveAll(Predicate<Card> match)
        {
            Selected.ForEach(c => c.Selected = !match(c));
            Selected.RemoveAll(match);
        }

        public static void Clear()
        {
            Selected.ForEach(c => c.Selected = false);
            Selected.Clear();
        }

        public static bool IsEmpty()
        {
            return Selected.Count == 0;
        }

        public static void ForEach(Action<Card> action)
        {
            Selected.ForEach(action);
        }

        public static void ForEachModifiable(Action<Card> action)
        {
            for (var i = Selected.Count - 1; i >= 0; --i)
                action(Selected[i]);
        }

        public static IEnumerable<CardControl> GetCardControls(GroupControl ctrl)
        {
            if (IsEmpty()) yield break;
            var groupCards = ctrl.Group.Cards;
            var generator = ctrl.GetItemContainerGenerator();
            for (var i = 0; i < groupCards.Count; ++i)
                if (groupCards[i].Selected)
                {
                    var container = generator.ContainerFromIndex(i);
                    var cardCtrl = (CardControl) VisualTreeHelper.GetChild(container, 0);
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
            return Selected.Contains(card) ? Selected : Enumerable.Repeat(card, 1);
        }

        public static void Do(Action<Card> action, Card card)
        {
            // Because some actions may modify the selection (which breaks the enumeration),
            // we make a local copy of the selection
            var cards = ExtendToSelection(card).ToList();
            foreach (var c in cards)
                action(c);
        }
    }
}