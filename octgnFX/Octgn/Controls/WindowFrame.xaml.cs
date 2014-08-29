using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Octgn.Extentions;
using Octgn.Annotations;

namespace Octgn.Controls
{
    public partial class WindowFrame : INotifyPropertyChanged
    {
        #region Content Property

        public static new readonly DependencyProperty ContentProperty;

        public new object Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (WindowFrame)property;
            window.ContentBorder.Child = (UIElement)args.NewValue;
        }
        #endregion


        public ImageSource Icon
        {
            get { return icon; }
            set
            {
                if (value == icon) return;
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (value == title) return;
                title = value;
                OnPropertyChanged("Title");
            }
        }

        private string title;

        private ImageSource icon;



        static WindowFrame()
        {
            // this checks whether application runs in design mode or not; if not the DependencyProperties are initialized
            if (!ControlExtensions.IsInDesignMode())
            {
                ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(WindowFrame), new UIPropertyMetadata(null, ContentChangedCallback));
                //BackgroundProperty = DependencyProperty.Register("Background", typeof(object), typeof(OctgnChrome), new UIPropertyMetadata(Brushes.Transparent, BackgroundChangedCallback));
            }
        }

        public WindowFrame()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
