using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Octgn.DataNew.Entities;

namespace Octgn.Play.Gui
{
    public partial class PlayerControl
    {
        public PlayerControl()
        {
            InitializeComponent();
            DataContextChanged += HookUpViewStateChangedListener;
        }

        private void HookUpViewStateChangedListener(object sender, DependencyPropertyChangedEventArgs e)
        {
            var player = e.OldValue as Player;
            if (player != null)
            {
                player = (Player) e.OldValue;
                foreach (Pile group in player.Groups.OfType<Pile>())
                    group.PropertyChanged -= GroupPropertyChanged;
            }

            player = e.NewValue as Player;
            if (player == null) return;
            foreach (Pile group in player.Groups.OfType<Pile>())
                group.PropertyChanged += GroupPropertyChanged;
        }

        private void GroupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "ViewState") return;
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
                double currentWidth = collapsedList.ActualWidth;
                collapsedList.UpdateLayout();
                double newWidth = collapsedList.ActualWidth;
                if (Math.Abs(newWidth - currentWidth) > 2)
                {
                    var anim = new DoubleAnimation(currentWidth, newWidth, TimeSpan.FromMilliseconds(300),
                                                   FillBehavior.Stop) {DecelerationRatio = 0.5};
                    collapsedList.BeginAnimation(WidthProperty, anim);
                }
            }
        }

        private void IsCollapsed(object sender, FilterEventArgs e)
        {
            var pile = e.Item as Pile;
            if (pile == null) e.Accepted = false;
            else e.Accepted = pile.ViewState == GroupViewState.Collapsed;
        }

        private void IsExpanded(object sender, FilterEventArgs e)
        {
            var pile = e.Item as Pile;
            if (pile == null) e.Accepted = false;
            else e.Accepted = pile.ViewState != GroupViewState.Collapsed;
        }
        
        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            System.Windows.Controls.ScrollViewer scv = (System.Windows.Controls.ScrollViewer)sender;
            scv.ScrollToHorizontalOffset(scv.HorizontalOffset - e.Delta/4);
            e.Handled = true;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size temp = base.MeasureOverride(constraint);
            temp.Height = gd.RowDefinitions[0].ActualHeight; //counters row
            temp.Height += 28;                               // + info-bars at base of card areas, cards themselves should be optional
            if (cardScroller.ComputedHorizontalScrollBarVisibility == System.Windows.Visibility.Visible)
            {
                temp.Height += SystemParameters.ScrollHeight;
            }
            return temp;
        }
    }

    public class PileControlSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement elemnt = container as FrameworkElement;
            Pile pile = item as Pile;
            if (pile.ViewState == GroupViewState.Pile)
            {
                return elemnt.FindResource("PileControl") as DataTemplate;
            }
            else
            {
                return elemnt.FindResource("ExpandedControl") as DataTemplate;
            }
        }
    }
}