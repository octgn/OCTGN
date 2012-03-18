using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Skylabs.Lobby;
using agsXMPP;
using agsXMPP.protocol.client;

namespace Octgn.Controls
{
    /// <summary>
    ///   Interaction logic for FriendListItem.xaml
    /// </summary>
    public partial class FriendListItem
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

        private NewUser _mUser = new NewUser(new Jid(""));
        private Point _startPoint;

        public FriendListItem()
        {
            InitializeComponent();
            ThisUser = new NewUser(new Jid(""));
        }

        public bool IsDragging { get; set; }
        public FrameworkElement DragScope { get; set; }

        public NewUser ThisUser
        {
            get { return _mUser; }
            set
            {
                _mUser = value;
                SetValue(CustomStatusProperty, value.CustomStatus);
                string h = ValueConverters.HashEmailAddress(_mUser.Email);
                string guri = "http://www.gravatar.com/avatar/" + h + "?s=64&r=x&salt=";
                SetValue(PictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
                SetValue(UsernameProperty, value.User.User);
                switch (value.Status)
                {
                    case UserStatus.Away:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusAway.png";
                        break;
                    case UserStatus.DoNotDisturb:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusDND.png";
                        break;
                    case UserStatus.Online:
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusOnline.png";
                        break;
                    default: //Offline or anything else
                        guri = @"pack://application:,,,/Octgn;component/Resources/statusOffline.png";
                        break;
                }
                SetValue(StatusPictureProperty, new ImageSourceConverter().ConvertFromString(guri) as ImageSource);
            }
        }

        private void UserControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Focus();
        }

        private void FlistitemMouseUp(object sender, MouseButtonEventArgs e)
        {
            Focus();
        }

        private void FlistitemPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        private void FlistitemPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || IsDragging) return;
            Point position = e.GetPosition(null);

            if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                //StartDragInProcAdorner(e);
                StartDragWindow(e);
            }
        }

        private void DragSourceQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            UpdateWindowLocation();
        }


        private void StartDragWindow(MouseEventArgs e)
        {
            GiveFeedbackEventHandler feedbackhandler = DragSourceGiveFeedback;
            GiveFeedback += feedbackhandler;
            QueryContinueDragEventHandler queryhandler = DragSourceQueryContinueDrag;
            QueryContinueDrag += queryhandler;
            IsDragging = true;
            CreateDragDropWindow(this);
            //DataObject data = new DataObject(System.Windows.DataFormats.Text.ToString(), "abcd");
            var data = new DataObject(DataFormats.Text, _mUser.User.Bare);
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
            Debug.Assert(dragElement is FrameworkElement);

            _dragdropWindow = new Window
                                  {
                                      WindowStyle = WindowStyle.None,
                                      AllowsTransparency = true,
                                      AllowDrop = false,
                                      Background = Brushes.WhiteSmoke,
                                      IsHitTestVisible = false,
                                      SizeToContent = SizeToContent.WidthAndHeight,
                                      Topmost = true,
                                      ShowInTaskbar = false
                                  };

            _dragdropWindow.SourceInitialized += delegate
                                                     {
                                                         PresentationSource windowSource =
                                                             PresentationSource.FromVisual(_dragdropWindow);
                                                         var hwndSource = (HwndSource) windowSource;
                                                         if (hwndSource == null) return;
                                                         IntPtr handle = hwndSource.Handle;

                                                         Int32 styles = Win32.GetWindowLong(handle,
                                                                                            Win32.GWL_EXSTYLE);
                                                         Win32.SetWindowLong(handle, Win32.GWL_EXSTYLE,
                                                                             styles | Win32.WS_EX_LAYERED |
                                                                             Win32.WS_EX_TRANSPARENT);
                                                     };

            var r = new Rectangle
                        {
                            Width = ((FrameworkElement) dragElement).ActualWidth,
                            Height = ((FrameworkElement) dragElement).ActualHeight,
                            Fill = new VisualBrush(dragElement)
                        };
            _dragdropWindow.Content = r;


            // put the window in the right place to start
            UpdateWindowLocation();
        }

        private void DestroyDragDropWindow()
        {
            if (_dragdropWindow == null) return;
            _dragdropWindow.Close();
            _dragdropWindow = null;
        }

        private void UpdateWindowLocation()
        {
            if (_dragdropWindow == null) return;
            Win32.POINT p;
            if (!Win32.GetCursorPos(out p))
            {
                return;
            }
            _dragdropWindow.Left = p.X;
            _dragdropWindow.Top = p.Y;
        }

        private void DragSourceGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            Debug.WriteLine("DragSource_GiveFeedback " + e.Effects);

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
            bool previousDrop = DragScope != null && DragScope.AllowDrop;
            if (DragScope != null) DragScope.AllowDrop = true;

            // Let's wire our usual events.. 
            // GiveFeedback just tells it to use no standard cursors..  

            GiveFeedbackEventHandler feedbackhandler = DragSourceGiveFeedback;
            GiveFeedback += feedbackhandler;

            // The DragOver event ... 
            DragEventHandler draghandler = Window1DragOver;
            DragScope.PreviewDragOver += draghandler;

            // Drag Leave is optional, but write up explains why I like it .. 
            DragEventHandler dragleavehandler = DragScopeDragLeave;
            DragScope.DragLeave += dragleavehandler;

            // QueryContinue Drag goes with drag leave... 
            QueryContinueDragEventHandler queryhandler = DragScopeQueryContinueDrag;
            DragScope.QueryContinueDrag += queryhandler;

            //Here we create our adorner.. 
            _adorner = new DragAdorner(DragScope, this, true, 0.5);
            _layer = AdornerLayer.GetAdornerLayer(DragScope);
            _layer.Add(_adorner);


            IsDragging = true;
            _dragHasLeftScope = false;
            //Finally lets drag drop 
            //var data = new DataObject(DataFormats.Text, "abcd");
            //DragDropEffects de = DragDrop.DoDragDrop(this, data, DragDropEffects.Move);

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

        private void DragScopeQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (!_dragHasLeftScope) return;
            e.Action = DragAction.Cancel;
            e.Handled = true;
        }


        private void DragScopeDragLeave(object sender, DragEventArgs e)
        {
            if (e.OriginalSource != DragScope) return;
            Point p = e.GetPosition(DragScope);
            Rect r = VisualTreeHelper.GetContentBounds(DragScope);
            if (r.Contains(p)) return;
            _dragHasLeftScope = true;
            e.Handled = true;
        }


        private void Window1DragOver(object sender, DragEventArgs args)
        {
            if (_adorner == null) return;
            _adorner.LeftOffset = args.GetPosition(DragScope).X /* - _startPoint.X */;
            _adorner.TopOffset = args.GetPosition(DragScope).Y /* - _startPoint.Y */;
        }

        private void image1_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
        }

        private void image1_MouseUp(object sender, MouseButtonEventArgs e)
        {

            Program.LClient.RemoveFriend(ThisUser);
        }
    }
}