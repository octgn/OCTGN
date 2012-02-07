using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using SimplestDragDrop;
using Skylabs;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem : UserControl
    {
        public static DependencyProperty UsernameProperty = DependencyProperty.Register(
            "UserName", typeof (string), typeof (FriendListItem));

        public static DependencyProperty CustomStatusProperty = DependencyProperty.Register(
            "CustomStatus", typeof (string), typeof (FriendListItem));

        public static DependencyProperty PictureProperty = DependencyProperty.Register(
            "Picture", typeof (ImageSource), typeof (FriendListItem));

        public static DependencyProperty StatusPictureProperty = DependencyProperty.Register(
            "StatusPicture", typeof (ImageSource), typeof (FriendListItem));

        private DragAdorner _adorner;
        private bool _dragHasLeftScope;
        private Window _dragdropWindow;
        private AdornerLayer _layer;
        private Point _startPoint;

        private User m_User = new User();

        public FriendListItem()
        {
            InitializeComponent();
            ThisUser = new User();
        }

        public bool IsDragging { get; set; }
        public FrameworkElement DragScope { get; set; }

        public User ThisUser
        {
            get { return m_User; }
            set
            {
                m_User = value;
                SetValue(CustomStatusProperty, value.CustomStatus);
                string h = ValueConverters.HashEmailAddress(value.Email.ToLower().Trim());
                string guri = "http://www.gravatar.com/avatar/" + h + "?s=64&r=x";
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
                SetValue(UsernameProperty, value.DisplayName);
                switch (value.Status)
                {
                    case UserStatus.Away:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusAway.png";
                        break;
                    case UserStatus.DoNotDisturb:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusDND.png";
                        break;
                    case UserStatus.Online:
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusOnline.png";
                        break;
                    default: //Offline or anything else
                        guri = @"pack://application:,,,/OCTGN;component/Resources/statusOffline.png";
                        break;
                }
                SetValue(StatusPictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void flistitem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        private void flistitem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void flistitem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    //StartDragInProcAdorner(e);
                    StartDragWindow(e);
                }
            }
        }

        private void DragSource_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            UpdateWindowLocation();
        }


        private void StartDragWindow(MouseEventArgs e)
        {
            GiveFeedbackEventHandler feedbackhandler = DragSource_GiveFeedback;
            GiveFeedback += feedbackhandler;
            QueryContinueDragEventHandler queryhandler = DragSource_QueryContinueDrag;
            QueryContinueDrag += queryhandler;
            IsDragging = true;
            CreateDragDropWindow(this);
            //DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
            var data = new DataObject(DataFormats.Text, m_User.Uid.ToString(CultureInfo.InvariantCulture));
            //DataObject data = new DataObject(this.m_User);
            _dragdropWindow.Show();
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            DestroyDragDropWindow();
            IsDragging = false;
            GiveFeedback -= feedbackhandler;
            QueryContinueDrag -= queryhandler;
        }


        private void CreateDragDropWindow(Visual dragElement)
        {
            Debug.Assert(_dragdropWindow == null);
            Debug.Assert(dragElement != null);
            // TODO: FE? or UIE??   FE cause I am lazy on size . 
            Debug.Assert(dragElement is FrameworkElement);

            _dragdropWindow = new Window();
            _dragdropWindow.WindowStyle = WindowStyle.None;
            _dragdropWindow.AllowsTransparency = true;
            _dragdropWindow.AllowDrop = false;
            _dragdropWindow.Background = Brushes.WhiteSmoke;
            _dragdropWindow.IsHitTestVisible = false;
            _dragdropWindow.SizeToContent = SizeToContent.WidthAndHeight;
            _dragdropWindow.Topmost = true;
            _dragdropWindow.ShowInTaskbar = false;

            _dragdropWindow.SourceInitialized += delegate
                                                     {
                                                         //TODO assert that we can do this.. 
                                                         PresentationSource windowSource =
                                                             PresentationSource.FromVisual(_dragdropWindow);
                                                         var hwndSource = (HwndSource) windowSource;
                                                         if (hwndSource != null)
                                                         {
                                                             IntPtr handle = hwndSource.Handle;

                                                             Int32 styles = Win32.GetWindowLong(handle,
                                                                                                Win32.GWL_EXSTYLE);
                                                             Win32.SetWindowLong(handle, Win32.GWL_EXSTYLE,
                                                                                 styles | Win32.WS_EX_LAYERED |
                                                                                 Win32.WS_EX_TRANSPARENT);
                                                         }
                                                     };

            var r = new Rectangle();
            r.Width = ((FrameworkElement) dragElement).ActualWidth;
            r.Height = ((FrameworkElement) dragElement).ActualHeight;
            r.Fill = new VisualBrush(dragElement);
            _dragdropWindow.Content = r;


            // put the window in the right place to start
            UpdateWindowLocation();
        }

        private void DestroyDragDropWindow()
        {
            if (_dragdropWindow != null)
            {
                _dragdropWindow.Close();
                _dragdropWindow = null;
            }
        }

        private void UpdateWindowLocation()
        {
            if (_dragdropWindow != null)
            {
                Win32.POINT p;
                if (!Win32.GetCursorPos(out p))
                {
                    return;
                }
                _dragdropWindow.Left = p.X;
                _dragdropWindow.Top = p.Y;
            }
        }

        private void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            Debug.WriteLine("DragSource_GiveFeedback " + e.Effects.ToString());

            if (DragScope == null)
            {
                //This loads the cursor from a stream .. 
                /*
                    if (_allOpsCursor == null)
                    {
                        using (Stream cursorStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SimplestDragDrop.DDIcon.cur"))
                        {
                            _allOpsCursor = new Cursor(cursorStream);
                        }
                    }
                    Mouse.SetCursor(_allOpsCursor);
                    
                    e.UseDefaultCursors = false;
                     * */
                e.Handled = true;
            }
            else // This code is called when we are using a custom cursor  (either a window or adorner ) 
            {
                e.UseDefaultCursors = false;
                e.Handled = true;
            }
        }

        private void StartDragInProcAdorner(MouseEventArgs e)
        {
            // Let's define our DragScope .. In this case it is every thing inside our main window .. 
            DragScope = Program.ClientWindow.Content as FrameworkElement;
            Debug.Assert(DragScope != null);

            // We enable Drag & Drop in our scope ...  We are not implementing Drop, so it is OK, but this allows us to get DragOver 
            bool previousDrop = DragScope.AllowDrop;
            DragScope.AllowDrop = true;

            // Let's wire our usual events.. 
            // GiveFeedback just tells it to use no standard cursors..  

            GiveFeedbackEventHandler feedbackhandler = DragSource_GiveFeedback;
            GiveFeedback += feedbackhandler;

            // The DragOver event ... 
            DragEventHandler draghandler = Window1_DragOver;
            DragScope.PreviewDragOver += draghandler;

            // Drag Leave is optional, but write up explains why I like it .. 
            DragEventHandler dragleavehandler = DragScope_DragLeave;
            DragScope.DragLeave += dragleavehandler;

            // QueryContinue Drag goes with drag leave... 
            QueryContinueDragEventHandler queryhandler = DragScope_QueryContinueDrag;
            DragScope.QueryContinueDrag += queryhandler;

            //Here we create our adorner.. 
            _adorner = new DragAdorner(DragScope, this, true, 0.5);
            _layer = AdornerLayer.GetAdornerLayer(DragScope);
            _layer.Add(_adorner);


            IsDragging = true;
            _dragHasLeftScope = false;
            //Finally lets drag drop 
            var data = new DataObject(DataFormats.Text, "abcd");
            DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

            // Clean up our mess :) 
            DragScope.AllowDrop = previousDrop;
            AdornerLayer.GetAdornerLayer(DragScope).Remove(_adorner);
            _adorner = null;

            GiveFeedback -= feedbackhandler;
            DragScope.DragLeave -= dragleavehandler;
            DragScope.QueryContinueDrag -= queryhandler;
            DragScope.PreviewDragOver -= draghandler;

            IsDragging = false;
        }

        private void DragScope_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (_dragHasLeftScope)
            {
                e.Action = DragAction.Cancel;
                e.Handled = true;
            }
        }


        private void DragScope_DragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource == DragScope)
            {
                Point p = e.GetPosition(DragScope);
                Rect r = VisualTreeHelper.GetContentBounds(DragScope);
                if (!r.Contains(p))
                {
                    _dragHasLeftScope = true;
                    e.Handled = true;
                }
            }
        }


        private void Window1_DragOver(object sender, DragEventArgs args)
        {
            if (_adorner != null)
            {
                _adorner.LeftOffset = args.GetPosition(DragScope).X /* - _startPoint.X */;
                _adorner.TopOffset = args.GetPosition(DragScope).Y /* - _startPoint.Y */;
            }
        }
    }
}