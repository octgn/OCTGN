// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.Core;
using Octgn.DataNew.Entities;
using Octgn.Utils;
using Octgn.Core.DataExtensionMethods;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Collections.Generic;
using Octgn.Controls;

namespace Octgn.Play.Dialogs
{

    public partial class PreviewCardWindow : DecorableWindow
    {
        public DataNew.Entities.Card VisualCard;
        public List<string> alternates;
        public bool isVisible;
        public bool showProxy;
        public PreviewCardWindow()
        {
            this.InitializeComponent();
            var size = Prefs.PreviewCardWindowLocation;
            Left = size.Left;
            Top = size.Top;
            Height = size.Height;
            Width = size.Width;
            AltName.Visibility = Visibility.Collapsed;
            AltLabel.Visibility = Visibility.Collapsed;
            LeftButton.Visibility = Visibility.Collapsed;
            RightButton.Visibility = Visibility.Collapsed;
            UpdateCardImage();
        }

        public void SetCard(Card card, bool up, bool isProxy = false)
        {
            isVisible = up;
            showProxy = isProxy;
            VisualCard = new DataNew.Entities.Card(card.Type.Model);
            if (VisualCard == null || up == false || VisualCard.PropertySets.Count < 2)
            {
                AltName.Visibility = Visibility.Collapsed;
                AltLabel.Visibility = Visibility.Collapsed;
                LeftButton.Visibility = Visibility.Collapsed;
                RightButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AltName.Visibility = Visibility.Visible;
                AltLabel.Visibility = Visibility.Visible;
                LeftButton.Visibility = Visibility.Visible;
                RightButton.Visibility = Visibility.Visible;
                alternates = VisualCard.PropertySets.Keys.ToList();
                AltName.Text = VisualCard.Alternate;
            }
            UpdateCardImage();
        }

        public void SetCard(ICard card, bool up, bool isProxy = false)
        {
            isVisible = up;
            showProxy = isProxy;
            VisualCard = new DataNew.Entities.Card(card);
            if (VisualCard == null || up == false || VisualCard.PropertySets.Count < 2)
            {
                AltName.Visibility = Visibility.Collapsed;
                AltLabel.Visibility = Visibility.Collapsed;
                LeftButton.Visibility = Visibility.Collapsed;
                RightButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                AltName.Visibility = Visibility.Visible;
                AltLabel.Visibility = Visibility.Visible;
                LeftButton.Visibility = Visibility.Visible;
                RightButton.Visibility = Visibility.Visible;
                alternates = VisualCard.PropertySets.Keys.ToList();
                AltName.Text = VisualCard.Alternate;
            }
            UpdateCardImage();

        }

        public void UpdateCardImage()
        {
            if (!isVisible || VisualCard == null) //if the player doesn't have visibility of the card
            {
                cardViewer.Source = Program.GameEngine.GetCardFront(VisualCard?.Size ?? Program.GameEngine.Definition.DefaultSize());
            }
            else
            {
                BitmapImage image = null;
                ImageUtils.GetCardImage(VisualCard, x => image = x, ProxyCheckbox.IsChecked == true || showProxy);
                cardViewer.Source = image ?? Program.GameEngine.GetCardFront(VisualCard.Size);
            }

        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Prefs.PreviewCardWindowLocation = new Rect(Left, Top, ActualWidth, ActualHeight);
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            var index = alternates.IndexOf(VisualCard.Alternate);
            index++;
            if (index >= alternates.Count)
                index = 0;
            VisualCard.Alternate = alternates[index];
            AltName.Text = VisualCard.Alternate;
            UpdateCardImage();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            var index = alternates.IndexOf(VisualCard.Alternate);
            if (index == 0)
            {
                index = alternates.Count;
            }
            VisualCard.Alternate = alternates[index - 1];
            AltName.Text = VisualCard.Alternate;
            UpdateCardImage();

        }

        private void ProxyCheckbox_Click(object sender, RoutedEventArgs e)
        {
            UpdateCardImage();
        }
    }
}