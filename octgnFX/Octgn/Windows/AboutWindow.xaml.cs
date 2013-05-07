using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Octgn.Launcher;

namespace Octgn.Windows
{
    using System.Diagnostics;
    using System.Windows.Navigation;

    using Octgn.Controls;

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : OctgnChrome
    {
        private Image currentImage = null;

        public AboutWindow()
        {
            InitializeComponent();
            imgDrilus.MouseUp += PictureMouseUp;
            imgD0c.MouseUp += PictureMouseUp;
            imgGravecorp.MouseUp += PictureMouseUp;
            imgOther.MouseUp += PictureMouseUp;
            imgRalig98.MouseUp += PictureMouseUp;
        }

        private void PictureMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (currentImage == null)
            {
                currentImage = sender as Image;
            }
            else if (currentImage == sender)
            {
                HideImage(currentImage);
                currentImage = null;
            }
            else
            {
                HideImage(currentImage);
                ShowImage(sender as Image);
                currentImage = sender as Image;
            }
        }

        private void ShowImage(Image i)
        {
            foreach (var c in this.MainGrid.Children)
            {
                var tb = c as TextBlock;
                if (tb != null)
                {
                    if (tb.Name == i.Name.Replace("img", "text"))
                    {
                        DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                        i.BeginAnimation(OpacityProperty, animation);

                        DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                        textAbout.BeginAnimation(OpacityProperty, hide);
                        tb.BeginAnimation(OpacityProperty, animation);
                        return;
                    }
                }
            }

        }
        private void HideImage(Image i)
        {
            foreach (var c in this.MainGrid.Children)
            {
                var tb = c as TextBlock;
                if (tb != null)
                {
                    if (tb.Name == i.Name.Replace("img", "text"))
                    {
                        DoubleAnimation animation = new DoubleAnimation(0.25, TimeSpan.FromSeconds(0.5));
                        i.BeginAnimation(OpacityProperty, animation);

                        DoubleAnimation show = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                        textAbout.BeginAnimation(OpacityProperty, show);
                        DoubleAnimation hide = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
                        tb.BeginAnimation(OpacityProperty, hide);
                        return;
                    }
                }
            }
        }

        private void imgMouseEnter(object sender, MouseEventArgs e)
        {
            if (currentImage != null) return;
            var s = sender as Image;
            ShowImage(s);
        }

        private void imgMouseLeave(object sender, MouseEventArgs e)
        {
            if (currentImage != null) return;
            var s = sender as Image;
            HideImage(s);
        }

        #region Users

        #endregion

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Program.LaunchUrl(e.Uri.ToString());
        }       
    }
}
