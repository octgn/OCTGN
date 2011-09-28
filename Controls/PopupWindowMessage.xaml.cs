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
        System.Windows.Media.Animation.DoubleAnimation OpenAnimation;
        System.Windows.Media.Animation.DoubleAnimation CloseAnimation;

        public PopupWindowMessage()
        {
            InitializeComponent();
            /*
                <DoubleAnimation From="34" To="200" Duration="00:00:02"/>
                <DoubleAnimation From="0" To="300" Duration="00:00:02"/>
                <DoubleAnimation From="0" To="1" Duration="00:00:02"/>
             */
            this.Opacity = 0;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public void ShowMessage(string title, string message)
        {
            DoubleAnimation o = new DoubleAnimation(0, 1, new Duration(new System.TimeSpan(0, 0, 2)));
            Storyboard.SetTarget(o, this);
            Storyboard.SetTargetProperty(o, new PropertyPath(UIElement.OpacityProperty));
            Storyboard sb = new Storyboard();
            sb.Children.Add(o);
            sb.Begin();
        }
    }
}