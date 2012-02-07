﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Octgn.Controls;
using Octgn.Utils;
using System;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Data;

namespace Octgn.Play.Gui
{
    public enum PilePosition
    { All, Top, Bottom }

    public partial class GroupWindow : ChildWindow
    {
        private PilePosition position;
        private int count, id;
        private Group group;
        private bool shouldNotifyClose = false;

        public GroupWindow()
        {
            InitializeComponent();
        }

        public GroupWindow(Group group, PilePosition position, int count)
            : this()
        {
            id = Program.Game.GetUniqueId();
            this.position = position;
            this.count = count;
            DataContext = this.group = group;

            switch (position)
            {
                case PilePosition.All:
                    Title = group.FullName;
                    cardsList.Cards = group.Cards;
                    break;
                case PilePosition.Top:
                    Title = string.Format("{0} Top {1} cards", group.FullName, count);
                    cardsList.Cards = new ObservableCollection<Card>(group.Take(count));
                    cardsList.RestrictDrop = true;
                    break;
                case PilePosition.Bottom:
                    Title = string.Format("{0} Bottom {1} cards", group.FullName, count);
                    cardsList.Cards = new ObservableCollection<Card>(group.Skip(Math.Max(0, group.Count - count)));
                    cardsList.RestrictDrop = true;
                    break;
            }

            if (cardsList.Cards != group.Cards)
                ((INotifyCollectionChanged)group.Cards).CollectionChanged += CardsChanged;

            // The shuffle link is confusing at best when a subset of the group is displayed
            if (position != PilePosition.All)
                shuffleLink.Visibility = Visibility.Collapsed;

            // If the whole group is visible to everyone, there's nothing to be done, really.
            if (group.Visibility == GroupVisibility.Everybody)
                return;

            SendLookAtRpc(true);
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (shouldNotifyClose)
                SendLookAtRpc(false);
            ((INotifyCollectionChanged)group.Cards).CollectionChanged -= CardsChanged;
        }

        private void SendLookAtRpc(bool look)
        {
            shouldNotifyClose = look;
            switch (position)
            {
                case PilePosition.All:
                    Program.Client.Rpc.LookAtReq(id, group, look);
                    break;
                case PilePosition.Top:
                    Program.Client.Rpc.LookAtTopReq(id, group, count, look);
                    break;
                case PilePosition.Bottom:
                    Program.Client.Rpc.LookAtBottomReq(id, group, count, look);
                    break;
            }
        }

        private void CloseClicked(object sender, System.Windows.RoutedEventArgs e)
        { Close(); }

        private void CloseAndShuffleClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
            var pile = group as Pile;
            if (pile != null) pile.Shuffle();
        }

        private void FilterChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CardControl.SetAnimateLoad(this, false);

            string filter = filterBox.Text;
            if (filter == "")
            {
                watermark.Visibility = Visibility.Visible;
                cardsList.FilterCards = null;
            }
            else
            {
                var textProperties = Program.Game.Definition.CardDefinition.Properties.Values
                                            .Where(p => p.Type == Octgn.Data.PropertyType.String && !p.IgnoreText)
                                            .Select(p => p.Name);
                watermark.Visibility = Visibility.Hidden;
                cardsList.FilterCards = (c) =>
                {
                    if (!cardsList.IsAlwaysUp && !c.FaceUp)
                        return false;
                    if (c.RealName.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        return true;
                    foreach (string property in textProperties)
                    {
                        var propertyValue = (string)c.GetProperty(property);
                        if (propertyValue == null) continue;
                        if (propertyValue.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            return true;
                    }
                    return false;
                };
                alphaOrderBtn.IsChecked = true;
            }
        }

        private void FilterBoxPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                e.Handled = true;
                filterBox.Text = "";
            }
        }

        private void PositionOrderChecked(object sender, RoutedEventArgs e)
        {
            if (alphaOrderBtn == null) return;  // Happens during the control initialization, when the event is first called
            alphaOrderBtn.IsChecked = false;
            cardsList.SortByName = false;
            filterBox.Text = "";
        }

        private void AlphaOrderChecked(object sender, RoutedEventArgs e)
        {
            CardControl.SetAnimateLoad(this, false);

            positionOrderBtn.IsChecked = false;
            cardsList.SortByName = true;
        }

        public void CardsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableCollection<Card> cards = cardsList.Cards;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldItems.Count > 1) throw new NotSupportedException("Can't move more than 1 card at a time");
                    var card = e.OldItems[0] as Card;
                    if (!cards.Contains(card)) break;

                    if ((position == PilePosition.Top && e.NewStartingIndex >= count) ||
                        (position == PilePosition.Bottom && e.NewStartingIndex < group.Count - count))
                    {
                        cards.Remove(card);
                        count--;
                    }
                    else
                    {
                        var src = sender as ICollection<Card>;
                        int oldIndex = cards.IndexOf(card);
                        int newIndex = src.Where(c => cards.Contains(c)).IndexOf(card);
                        cards.Move(oldIndex, newIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    foreach (Card c in e.OldItems)
                        cards.Remove(c);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    cards.Clear();
                    break;
            }
        }
    }

    [ValueConversion(typeof(Player), typeof(Visibility))]
    public class ControllerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == Player.LocalPlayer ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }


}
