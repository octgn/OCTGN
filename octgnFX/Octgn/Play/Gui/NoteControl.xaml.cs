using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using Octgn.Core;

namespace Octgn.Play.Gui
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    using log4net;

    using Octgn.Play.Gui.Adorners;

    /// <summary>
    /// Interaction logic for NoteControl.xaml
    /// </summary>
    public partial class NoteControl : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly DoubleAnimation hideAnimation;
        private readonly DoubleAnimation showAnimation;

        public NoteControl():this("")
        {

        }

        public NoteControl(string message)
        {
            this.hideAnimation = new DoubleAnimation();
            hideAnimation.From = 1;
            hideAnimation.To = 0;
            hideAnimation.Duration = new Duration(TimeSpan.FromSeconds(.5));
            hideAnimation.AutoReverse = false;
            hideAnimation.Completed += this.HideAnimationCompleted;

            this.showAnimation = new DoubleAnimation();
            showAnimation.From = 0;
            showAnimation.To = 1;
            showAnimation.Duration = new Duration(TimeSpan.FromSeconds(.5));
            showAnimation.AutoReverse = false;
            InitializeComponent();
            TextBox.Text = message;
            this.Loaded += OnLoaded;
            this.MouseEnter += OnMouseEnter;
            this.MouseLeave += NoteControl_MouseLeave;            
        }

        void NoteControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.IsMouseOver) return;
            var sb = Resources["ShrinkBar"] as Storyboard;
            sb.Begin(TopBar);
        }

        private void OnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            var sb = Resources["GrowBar"] as Storyboard;
            sb.Begin(TopBar);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Loaded -= OnLoaded;
            this.MainGrid.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Program.GameEngine.Definition.NoteBackgroundColor));
            this.TextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Program.GameEngine.Definition.NoteForegroundColor));

            if (Prefs.UseGameFonts)
            {
                var font = Program.GameEngine.Definition.Fonts.FirstOrDefault(
                    x => x.Target.Equals("notes", StringComparison.InvariantCultureIgnoreCase));
                if (font != null)
                {
                    Log.Info("Loading font");
                    if (!File.Exists(font.Src))
                    {
                        Log.WarnFormat("Note Font {0} does not exist", font.Src);
                        return;
                    }
                    var pf = new System.Drawing.Text.PrivateFontCollection();
                    pf.AddFontFile(font.Src);
                    if (pf.Families.Length > 0)
                    {
                        Log.Info("Loaded font into collection");
                    }
                    string font1 = "file:///" + Path.GetDirectoryName(font.Src) + "/#" + pf.Families[0].Name;
                    Log.Info(string.Format("Loading font with path: {0}", font1).Replace("\\", "/"));
                    this.TextBox.FontFamily = new FontFamily(font1.Replace("\\", "/"));
                    this.TextBox.FontSize = font.Size;
                    Log.Info(string.Format("Loaded font with source: {0}", this.TextBox.FontFamily.Source));
                }
            }
            this.BeginAnimation(OpacityProperty,showAnimation);
        }

        public NoteAdorner Adorner { get; set; }

        void HideAnimationCompleted(object sender, EventArgs e)
        {
            (this.Parent as Canvas).Children.Remove(this);
        }

        private void CloseClick(object sender, RoutedEventArgs e)
        {
            this.BeginAnimation(OpacityProperty, hideAnimation);
        }

        public void Dispose()
        {
            hideAnimation.Completed -= this.HideAnimationCompleted;
            this.MouseEnter -= OnMouseEnter;
            this.MouseLeave -= NoteControl_MouseLeave;
            Adorner.Dispose();
        }

        private void OnScroll(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void OnScrollPreview(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (e.Delta > 0)
                {
                    if(this.TextBox.FontSize + 1 < 240)
                        this.TextBox.FontSize++;
                }
                else
                {
                    if(this.TextBox.FontSize - 1 > 0)
                        this.TextBox.FontSize--;
                }
                e.Handled = true;
            }
            //e.Handled = true;
        }
    }
}
