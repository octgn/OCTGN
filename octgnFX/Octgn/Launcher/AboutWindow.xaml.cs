using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Page
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Program.LauncherWindow.Navigate(new Login());
        } 

        #region Users

        private void imgDrilus_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));            
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, hide);
            textDrilus.BeginAnimation(OpacityProperty, animation);
        }

        private void imgDrilus_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(0.25, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation show = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, show);
            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textDrilus.BeginAnimation(OpacityProperty, hide);
        }               

        private void imgRalig98_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, hide);
            textRalig98.BeginAnimation(OpacityProperty, animation);
        }

        private void imgRalig98_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(0.25, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation show = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, show);
            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textRalig98.BeginAnimation(OpacityProperty, hide);
        }

        #endregion
    }
}
