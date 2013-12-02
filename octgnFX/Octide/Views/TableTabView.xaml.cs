using System.Windows.Controls;

namespace Octide.Views
{
    using System.Windows;
    using System.Windows.Input;

    using GalaSoft.MvvmLight.Messaging;

    using Octide.Messages;

    public partial class TableTabView : UserControl
    {
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
    }
}