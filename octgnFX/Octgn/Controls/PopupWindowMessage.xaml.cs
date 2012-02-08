using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for PopupWindowMessage.xaml
    /// </summary>
    public partial class PopupWindowMessage
    {
        #region Delegates

        public delegate void HandlePopupWindowClose(object sender, bool XClosed);

        #endregion

        private Panel parentControl;
        private bool xclosed;

        public PopupWindowMessage()
        {
            InitializeComponent();
            /*
                <DoubleAnimation From="34" To="200" Duration="00:00:02"/>
                <DoubleAnimation From="0" To="300" Duration="00:00:02"/>
                <DoubleAnimation From="0" To="1" Duration="00:00:02"/>
             */
            Visibility = Visibility.Hidden;
        }

        public event HandlePopupWindowClose OnPopupWindowClose;

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public void ShowMessage(Panel ParentControl)
        {
            parentControl = ParentControl;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            SetValue(Grid.ColumnSpanProperty, 10);
            SetValue(Grid.RowSpanProperty, 10);
            Opacity = 0;
            Visibility = Visibility.Visible;
            ParentControl.Children.Add(this);
            var a = FindResource("sbShow") as Storyboard;
            if (a != null) a.Begin();
        }

        public void AddControl(UIElement e)
        {
            stackPanel1.Children.Add(e);
        }

        public void HideMessage()
        {
            var a = FindResource("sbHide") as Storyboard;
            if (a != null)
            {
                a.Completed += a_Completed;
                a.Begin();
            }
        }

        private void a_Completed(object sender, EventArgs e)
        {
            Visibility = Visibility.Hidden;
            if (parentControl != null)
                parentControl.Children.Remove(this);
            if (OnPopupWindowClose != null)
                OnPopupWindowClose(this, xclosed);
        }

        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            xclosed = true;
            HideMessage();
        }
    }
}