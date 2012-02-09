using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            for (int i = Selected.Count - 1; i >= 0; --i)
                action(Selected[i]);
        }

        public static IEnumerable<CardControl> GetCardControls(GroupControl ctrl)
        {
            if (IsEmpty()) yield break;
            ObservableCollection<Card> groupCards = ctrl.Group.Cards;
            ItemContainerGenerator generator = ctrl.GetItemContainerGenerator();
            for (int i = 0; i < groupCards.Count; ++i)
                if (groupCards[i].Selected)
                {
                    DependencyObject container = generator.ContainerFromIndex(i);
                    var cardCtrl = (CardControl) VisualTreeHelper.GetChild(container, 0);
                    yield return cardCtrl;
                }
        }

        public static IEnumerable<CardControl> GetCardControls(GroupControl ctrl, CardControl empty)
        {
            if (IsEmpty())
                yield return empty;
            else
                foreach (CardControl cardCtrl in GetCardControls(ctrl))
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
            List<Card> cards = ExtendToSelection(card).ToList();
            foreach (Card c in cards)
                action(c);
        }
    }
}