using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Octgn.Annotations;
using Octgn.Utils;

namespace Octgn.Controls.WindowDecorators
{
    public partial class OctgnDecorator_2_Frame : INotifyPropertyChanged
    {
        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(OctgnDecorator_2_Frame), new UIPropertyMetadata(null, ContentChangedCallback));

        public new object Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }

        private static void ContentChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var window = (OctgnDecorator_2_Frame)property;
            window.ContentBorder.Child = (UIElement)args.NewValue;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof (string), typeof (OctgnDecorator_2_Frame), new PropertyMetadata(default(string)));

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof (ImageSource), typeof (OctgnDecorator_2_Frame), new PropertyMetadata(default(ImageSource)));

        public ImageSource Icon
        {
            get { return (ImageSource) GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty MinimizeButtonVisibleProperty = DependencyProperty.Register(
            "MinimizeButtonVisible", typeof (bool), typeof (OctgnDecorator_2_Frame), new PropertyMetadata(true));

        public bool MinimizeButtonVisible
        {
            get { return (bool) GetValue(MinimizeButtonVisibleProperty); }
            set { SetValue(MinimizeButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty ResizeButtonVisibleProperty = DependencyProperty.Register(
            "ResizeButtonVisible", typeof(bool), typeof(OctgnDecorator_2_Frame), new PropertyMetadata(true));

        public bool ResizeButtonVisible
        {
            get { return (bool) GetValue(ResizeButtonVisibleProperty); }
            set { SetValue(ResizeButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibleProperty = DependencyProperty.Register(
            "CloseButtonVisible", typeof(bool), typeof(OctgnDecorator_2_Frame), new PropertyMetadata(true));

        public bool CloseButtonVisible
        {
            get { return (bool) GetValue(CloseButtonVisibleProperty); }
            set { SetValue(CloseButtonVisibleProperty, value); }
        }

        public static readonly DependencyProperty CanResizeProperty = DependencyProperty.Register(
            "CanResize", typeof (bool), typeof (OctgnDecorator_2_Frame), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background", typeof(Brush), typeof(OctgnDecorator_2_Frame), new PropertyMetadata(default(Brush)));

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public bool CanResize
        {
            get { return (bool) GetValue(CanResizeProperty); }
            set { SetValue(CanResizeProperty, value); }
        }

        public OctgnDecorator_2_Frame()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<Win32.ResizeDirection> Resize;

        private void DragMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (!this.CanResize)
            {
                return;
            }

            if (mouseButtonEventArgs.LeftButton != MouseButtonState.Pressed) return;
            if (mouseButtonEventArgs.MiddleButton == MouseButtonState.Pressed || mouseButtonEventArgs.RightButton == MouseButtonState.Pressed) return;

            var s = sender as Rectangle;
            if (s == null)
            {
                return;
            }
            switch (s.Name)
            {
                case "DragTopLeft":
                    OnResize(Win32.ResizeDirection.TopLeft);
                    break;
                case "DragTop":
                    OnResize(Win32.ResizeDirection.Top);
                    break;
                case "DragTopRight":
                    OnResize(Win32.ResizeDirection.TopRight);
                    break;
                case "DragLeft":
                    OnResize(Win32.ResizeDirection.Left);
                    break;
                case "DragRight":
                    OnResize(Win32.ResizeDirection.Right);
                    break;
                case "DragBottomLeft":
                    OnResize(Win32.ResizeDirection.BottomLeft);
                    break;
                case "DragBottom":
                    OnResize(Win32.ResizeDirection.Bottom);
                    break;
                case "DragBottomRight":
                    OnResize(Win32.ResizeDirection.BottomRight);
                    break;
            }
        }

        protected virtual void OnResize(Win32.ResizeDirection obj)
        {
            Action<Win32.ResizeDirection> handler = Resize;
            if (handler != null) handler(obj);
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
