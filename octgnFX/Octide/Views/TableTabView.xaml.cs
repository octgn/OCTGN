using System.Windows.Controls;
using Octide.ViewModel;

namespace Octide.Views
{
    using System.Windows;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;

    public partial class TableTabView : UserControl
    {
        private bool mouseDown = false;
        private Point mouseDownOffset = new Point();
        private bool isDragging = false;
        private Canvas cardDragging = null;

        public TableTabView()
        {
            InitializeComponent();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            Point center = e.GetPosition(cardsView);

            var mess = new MouseWheelTableZoom(e.Delta,center);
			Messenger.Default.Send(mess);

            base.OnMouseWheel(e);
        }

        private void ButtonResetZoomClick(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TableTabViewModel;
            vm.Zoom = 1;
        }

        private void CardMouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            cardDragging = sender as Canvas;
            mouseDownOffset = e.GetPosition(cardDragging);
            cardsView.CaptureMouse();
        }

        private void CardMouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void CardsViewMouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            base.OnMouseMove(e);//?
            e.Handled = true;

            var cur = e.GetPosition(cardsView);
            if (!isDragging)
            {
                isDragging = true;
            }
            if (isDragging)
            {
                var vm = cardDragging.DataContext as CardViewModel;

                vm.X = cur.X - mouseDownOffset.X;
                vm.Y = cur.Y - mouseDownOffset.Y;
                vm.RefreshValues();
            }
        }

        private void ControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            isDragging = false;
            cardDragging = null;
            cardsView.ReleaseMouseCapture();
        }

        private void ButtonCreateNewCard(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TableTabViewModel;
            vm.NewCard();
        }

        private void ButtonResetCards(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TableTabViewModel;
            vm.ResetCards();
        }
    }
}