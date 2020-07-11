using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Octgn.Play.Gui.Adorners
{
    internal class ArrowAdorner : Adorner
    {
        private static readonly DependencyPropertyDescriptor IsInvertedPropertyDescriptor =
            DependencyPropertyDescriptor.FromProperty(CardControl.IsInvertedProperty, typeof (CardControl));

        private readonly ArrowControl _arrow = new ArrowControl {ToPoint = new Point(200, 200)};
        private readonly Player _player;
        private CardControl _toCard;

        public ArrowAdorner(Player player, UIElement adornedElement)
            : base(adornedElement)
        {
            _player = player;
            IsHitTestVisible = false;

            UpdateStartPoint(this, EventArgs.Empty);
            var cardCtrl = (CardControl) AdornedElement;
            cardCtrl.rotate90.Changed += UpdateStartPoint;
            IsInvertedPropertyDescriptor.AddValueChanged(cardCtrl, UpdateToPoint);

            Unloaded += delegate
                            {
                                var adornedCard = ((CardControl) AdornedElement);
                                adornedCard.rotate90.Changed -= UpdateStartPoint;
                                IsInvertedPropertyDescriptor.RemoveValueChanged(adornedCard, UpdateToPoint);
                                if (_toCard == null) return;
                                DependencyPropertyDescriptor leftDescriptor =
                                    DependencyPropertyDescriptor.FromProperty(Canvas.LeftProperty, typeof (Canvas));
                                DependencyPropertyDescriptor topDescriptor =
                                    DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty, typeof (Canvas));
                                leftDescriptor.RemoveValueChanged(VisualTreeHelper.GetParent(_toCard), UpdateToPoint);
                                topDescriptor.RemoveValueChanged(VisualTreeHelper.GetParent(_toCard), UpdateToPoint);
                                leftDescriptor.RemoveValueChanged(VisualTreeHelper.GetParent(adornedCard),
                                                                  UpdateToPoint);
                                topDescriptor.RemoveValueChanged(VisualTreeHelper.GetParent(adornedCard),
                                                                 UpdateToPoint);
                                _toCard.rotate90.Changed -= UpdateToPoint;
                                _toCard.Unloaded -= RemoveFromLayer;
                            };
        }

        private void UpdateStartPoint(object sender, EventArgs e)
        {
            _arrow.FromPoint = ((CardControl) AdornedElement).GetMiddlePoint(false);
        }

        private void UpdateToPoint(object sender, EventArgs e)
        {
            // Fix: toCard may be null in some rare circumstances. E.g.:
            // 1. I start the creation of an arrow from an opponent's card
            // 2. Before I drop the arrow he moves his card across the middle line
            // 3. the IsInverted property change triggers a call to UpdateToPoint, although there still is no toCard.
            // in those cases, we simply have nothing to do.
            if (_toCard == null) return;

            var fromCard = (CardControl) AdornedElement;
            var fromCtrl = (UIElement) VisualTreeHelper.GetParent(fromCard);
            var toCtrl = (UIElement) VisualTreeHelper.GetParent(_toCard);
            double deltaX = Canvas.GetLeft(toCtrl) - Canvas.GetLeft(fromCtrl);
            double deltaY = Canvas.GetTop(toCtrl) - Canvas.GetTop(fromCtrl);
            Point toMiddlePt = _toCard.GetMiddlePoint(_toCard.IsInverted ^ fromCard.IsInverted);
            _arrow.ToPoint = fromCard.IsInverted
                                 ? toMiddlePt - new Vector(deltaX, deltaY)
                                 : toMiddlePt + new Vector(deltaX, deltaY);
        }

        public void UpdateToPoint(Point pt)
        {
            _arrow.ToPoint = pt;
        }

        public void LinkToCard(CardControl cardCtrl)
        {
            _toCard = cardCtrl;
            UpdateToPoint(this, EventArgs.Empty);
            cardCtrl.rotate90.Changed += UpdateToPoint;
            var adornedCard = (CardControl) AdornedElement;
            DependencyPropertyDescriptor leftDescriptor = DependencyPropertyDescriptor.FromProperty(
                Canvas.LeftProperty, typeof (Canvas));
            DependencyPropertyDescriptor topDescriptor = DependencyPropertyDescriptor.FromProperty(Canvas.TopProperty,
                                                                                                   typeof (Canvas));
            leftDescriptor.AddValueChanged(VisualTreeHelper.GetParent(cardCtrl), UpdateToPoint);
            topDescriptor.AddValueChanged(VisualTreeHelper.GetParent(cardCtrl), UpdateToPoint);
            leftDescriptor.AddValueChanged(VisualTreeHelper.GetParent(adornedCard), UpdateToPoint);
            topDescriptor.AddValueChanged(VisualTreeHelper.GetParent(adornedCard), UpdateToPoint);
            cardCtrl.Unloaded += RemoveFromLayer;
        }

        protected void RemoveFromLayer(object sender, EventArgs e)
        {
            RemoveFromLayer();
        }

        public void RemoveFromLayer()
        {
            // Fix (jods): Parent is null when both cards are simultaneously removed from the table,
            // and the toCard.Unloaded event happens before the adorned card Unloaded event.
            // This can be repro for example by resetting the game
            if (Parent != null)
                ((AdornerLayer) Parent).Remove(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawGeometry(_player.TransparentBrush, new Pen(_player.Brush, 2), _arrow.Shape.Data);
        }
    }
}