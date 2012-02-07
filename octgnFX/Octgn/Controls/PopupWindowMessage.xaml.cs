using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Octgn.Controls
{
    /// <summary>
    /// Interaction logic for PopupWindowMessage.xaml
    /// </summary>
    public partial class PopupWindowMessage : UserControl
    {
        public delegate void HandlePopupWindowClose(object sender, bool XClosed);
        public event HandlePopupWindowClose OnPopupWindowClose;
        private Panel parentControl;
        private bool xclosed = false;

        public PopupWindowMessage()
        {
            InitializeComponent();
            /*
                <DoubleAnimation From="34" To="200" Duration="00:00:02"/>
                <DoubleAnimation From="0" To="300" Duration="00:00:02"/>
                <DoubleAnimation From="0" To="1" Duration="00:00:02"/>
             */
            Visibility = System.Windows.Visibility.Hidden;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public void ShowMessage(Panel ParentControl)
        {
            parentControl = ParentControl;
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            VerticalAlignment = System.Windows.VerticalAlignment.Center;
            SetValue(Grid.ColumnSpanProperty, 10);
            SetValue(Grid.RowSpanProperty, 10);
            Opacity = 0;
            this.Visibility = System.Windows.Visibility.Visible;
            ParentControl.Children.Add(this);
            Storyboard a = this.FindResource("sbShow") as Storyboard;
            a.Begin();
        }

        public void AddControl(UIElement e)
        {
            stackPanel1.Children.Add(e);
        }

        public void HideMessage()
        {
            Storyboard a = this.FindResource("sbHide") as Storyboard;
            a.Completed += new System.EventHandler(a_Completed);
            a.Begin();
        }

        private void a_Completed(object sender, System.EventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            if (parentControl != null)
                parentControl.Children.Remove(this);
            if (OnPopupWindowClose != null)
                OnPopupWindowClose(this, xclosed);
        }

        private void image1_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            xclosed = true;
            HideMessage();
        }
    }
}