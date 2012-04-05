using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Octgn.Launcher;

namespace Octgn.Windows
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

        private void imgGravecorp_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, hide);
            textGravecorp.BeginAnimation(OpacityProperty, animation);
        }

        private void imgGravecorp_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(0.25, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation show = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, show);
            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textGravecorp.BeginAnimation(OpacityProperty, hide);
        }

        private void imgD0c_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, hide);
            textD0c.BeginAnimation(OpacityProperty, animation);
        }

        private void imgD0c_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(0.25, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation show = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, show);
            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textD0c.BeginAnimation(OpacityProperty, hide);
        }

        private void imgOther_MouseEnter(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, hide);
            textOther.BeginAnimation(OpacityProperty, animation);
        }

        private void imgOther_MouseLeave(object sender, MouseEventArgs e)
        {
            Image image = (Image)sender;
            DoubleAnimation animation = new DoubleAnimation(0.25, TimeSpan.FromSeconds(0.5));
            image.BeginAnimation(OpacityProperty, animation);

            DoubleAnimation show = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            textAbout.BeginAnimation(OpacityProperty, show);
            DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            textOther.BeginAnimation(OpacityProperty, hide);
        }

        #endregion                                
    }
}
