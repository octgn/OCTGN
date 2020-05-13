using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.DataNew.Entities;

namespace Octgn.Play.Gui
{
    partial class PileControl
    {
        private static readonly DoubleAnimation Anim = new DoubleAnimation
                                                           {
                                                               Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                                                               AccelerationRatio = 0.3,
                                                               DecelerationRatio = 0.3,
                                                               FillBehavior = FillBehavior.HoldEnd
                                                           };
        private FanPanel _fanPanel;

        public PileControl()
        {
            InitializeComponent();
            bottomZone.AddHandler(CardControl.CardOverEvent, new CardsEventHandler(OnCardOverBottom));
            bottomZone.AddHandler(CardControl.CardDroppedEvent, new CardsEventHandler(OnCardDroppedBottom));
            DensitySlider.ValueChanged += DensitySlider_ValueChanged;
            DataContextChanged += PileControl_DataContextChanged;
        }

        private void PileControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var pile = (e.OldValue as Pile);
            if (!(pile is null))
                pile.PropertyChanged -= PileControl_PropertyChanged;
            pile = (e.NewValue as Pile);
            if (!(pile is null))
                pile.PropertyChanged += PileControl_PropertyChanged;
        }

        private void PileControl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pile = (Pile)group;
            if (pile.ViewState == GroupViewState.Expanded)
            {
                expandButton.Visibility = Visibility.Hidden;
                DensitySlider.Visibility = Visibility.Visible;
                UpdateDensity();
            }
            else // if (pile.ViewState == GroupViewState.Pile)
            {
                DensitySlider.Visibility = Visibility.Collapsed;
                expandButton.Visibility = Visibility.Visible;
                Collapse();
            }
        }

        protected override void GroupChanged()
        {
            base.GroupChanged();

            var pile = (Pile) group;
            if (pile.ViewState == GroupViewState.Expanded)
            {
                expandButton.Visibility = Visibility.Hidden;
                DensitySlider.Visibility = Visibility.Visible;
            }
            else // if (pile.ViewState == GroupViewState.Pile)
            {
                DensitySlider.Visibility = Visibility.Collapsed;
                expandButton.Visibility = Visibility.Visible;
            }
            _fanPanel?.InvalidateArrange();
        }

        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            // Fix: capture the pile, it may sometimes be null when Completed executes.
            var capturedPile = (Pile) group;

            if (_fanPanel.HandDensity == 0)
            {
                var doubleAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                { EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn } };
                doubleAnimation.Completed += delegate
                                                 {
                                                     capturedPile.ViewState = GroupViewState.Collapsed;
                                                 };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
            }
            else // expanded
            {
                capturedPile.ViewState = GroupViewState.Pile;
            }
        }

        private void ExpandClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            (group as Pile).ViewState = GroupViewState.Expanded;
        }

        public override void ExecuteDefaultAction(Card card)
        {
            if (!ExecuteDefaultGroupAction()) ExecuteDefaultCardAction(card);
        }

        #region Card DnD

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            for (var i = 0; i < e.Cards.Length; i++)
            {
                e.CardSizes[i] = new Size(e.Cards[i].RealWidth * 100 / e.Cards[i].RealHeight, 100);
            }
            if (_fanPanel.HandDensity == 0)
            {
                if (bottomZone.Visibility != Visibility.Visible)
                {
                    bottomZone.Visibility = Visibility.Visible;
                    Anim.From = 0;
                    Anim.To = 0.4;
                    bottomZone.BeginAnimation(OpacityProperty, Anim);
                    Anim.From = null;
                }
                else
                {
                    Anim.To = 0.4;
                    bottomZone.BeginAnimation(OpacityProperty, Anim);
                }
            }
            else
            {
                //e.CardSize = new Size(100 * Program.GameEngine.Definition.DefaultSize.Width / Program.GameEngine.Definition.DefaultSize.Height, 100);
                _fanPanel.DisplayInsertIndicator(e.ClickedCard, _fanPanel.GetIndexFromPoint(Mouse.GetPosition(_fanPanel)));
            }
        }

        protected override void OnCardOut(object sender, CardsEventArgs e)
        {
            bottomZone.Visibility = Visibility.Collapsed;
            _fanPanel.HideInsertIndicator();
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (!@group.TryToManipulate()) return;
            int idx = _fanPanel.GetIndexFromPoint(Mouse.GetPosition(_fanPanel));
            var cards = e.Cards.ToArray();

            Card.MoveCardsTo(@group, cards, (args) =>
            {
                var c = args.Card;
                args.Index = idx;
                bool doNotIncrement = (c.Group == @group && @group.GetCardIndex(c) < idx);
                c.MoveTo(@group, e.FaceUp != null && e.FaceUp.Value, idx, false);
                // Fix: some cards (notably copies like token) may be deleted when they change group
                // in those case we should increment idx, otherwise an IndexOutOfRange exception may occur
                if (c.Group != @group)
                    doNotIncrement = true;
                if (!doNotIncrement) idx++;

            }, false);
        }

        private void OnCardDroppedBottom(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            //if (group.TryToManipulate())
            //    foreach (Card c in e.Cards)
            //        c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, group.Count,false);
            if (group.TryToManipulate())
            {
                var cards = e.Cards.ToArray();
                Card.MoveCardsTo(group, cards, 
                    Enumerable.Repeat(e.FaceUp ?? false,cards.Length).ToArray()
                    ,Enumerable.Range(group.Count,cards.Length).ToArray(),false);
            }
        }

        private void OnCardOverBottom(object sender, CardsEventArgs e)
        {
            OnCardOver(sender, e);
            Anim.To = 0.8;
            bottomZone.BeginAnimation(OpacityProperty, Anim);
        }

        #endregion

        internal override ItemContainerGenerator GetItemContainerGenerator()
        {
            return list.ItemContainerGenerator;
        }

        private void SaveFanPanel(object sender, RoutedEventArgs e)
        {
            _fanPanel = (FanPanel)sender;
            if ((group as Pile).ViewState == GroupViewState.Expanded)
                UpdateDensity();
            else
                Collapse();
        }

        private void DensitySlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            (sender as Slider).ValueChanged -= DensitySlider_ValueChanged;
        }
        private void DensitySlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            (sender as Slider).ValueChanged += DensitySlider_ValueChanged;
            if(!e.Canceled)
                SetDensity((sender as Slider).Value);
        }

        private void DensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SetDensity(e.NewValue);
        }

        private void SetDensity(double density)
        {
            if (density > 0)
            {
                var pile = group as Pile;
                pile.FanDensity = density;
                DensitySlider.Value = density;
                if (_fanPanel is null)
                    return;
                _fanPanel.HandDensity = density / 100;
                _fanPanel.InvalidateMeasure();
                this.InvalidateArrange();
            }
        }
        private void Collapse()
        {
            if (_fanPanel is null)
                return;
            _fanPanel.HandDensity = 0;
            _fanPanel.InvalidateMeasure();
            this.InvalidateArrange();
        }
        private void UpdateDensity()
        {
            DensitySlider.Value = (group as Pile).FanDensity;
            if (_fanPanel is null)
                return;
            _fanPanel.HandDensity = DensitySlider.Value / 100;
            _fanPanel.InvalidateMeasure();
            this.InvalidateArrange();
        }
    }
}