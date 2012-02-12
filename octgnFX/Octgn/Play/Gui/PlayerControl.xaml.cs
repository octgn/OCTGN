using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Octgn.Play.Gui
{
    public partial class PlayerControl
    {
        public PlayerControl()
        {
            InitializeComponent();
            DataContextChanged += HookUpCollapsedChangedListener;
        }

        private void HookUpCollapsedChangedListener(object sender, DependencyPropertyChangedEventArgs e)
        {
            var player = e.OldValue as Player;
            if (player != null)
            {
                player = (Player) e.OldValue;
                foreach (var group in player.Groups.OfType<Pile>())
                    group.PropertyChanged -= GroupPropertyChanged;
            }

            player = e.NewValue as Player;
            if (player == null) return;
            foreach (var group in player.Groups.OfType<Pile>())
                group.PropertyChanged += GroupPropertyChanged;
        }

        private void GroupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Collapsed") return;
            var cvs = (CollectionViewSource) FindResource("CollapsedGroups");
            if (cvs.View != null) cvs.View.Refresh();
            cvs = (CollectionViewSource) FindResource("ExpandedGroups");
            if (cvs.View != null) cvs.View.Refresh();

            if (collapsedList.Items.Count == 0 && collapsedList.ActualWidth > 0)
            {
                var anim = new DoubleAnimation(collapsedList.ActualWidth, 0, TimeSpan.FromMilliseconds(300),
                                               FillBehavior.Stop) {DecelerationRatio = 0.5};
                collapsedList.BeginAnimation(WidthProperty, anim);
            }
            else if (collapsedList.Items.Count > 0)
            {
                var currentWidth = collapsedList.ActualWidth;
                collapsedList.UpdateLayout();
                var newWidth = collapsedList.ActualWidth;
                if (Math.Abs(newWidth - currentWidth) > 2)
                {
                    var anim = new DoubleAnimation(currentWidth, newWidth, TimeSpan.FromMilliseconds(300),
                                                   FillBehavior.Stop) {DecelerationRatio = 0.5};
                    collapsedList.BeginAnimation(WidthProperty, anim);
                }
            }
        }

        private void IsCollapsedPile(object sender, FilterEventArgs e)
        {
            var pile = e.Item as Pile;
            e.Accepted = pile != null && pile.Collapsed;
        }

        private void IsExpandedPile(object sender, FilterEventArgs e)
        {
            var pile = e.Item as Pile;
            if (pile == null) e.Accepted = false;
            else e.Accepted = !pile.Collapsed;
        }
    }
}