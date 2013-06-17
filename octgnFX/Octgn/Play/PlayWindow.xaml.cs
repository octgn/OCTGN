using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using Octgn.Extentions;
using Octgn.Play.Dialogs;
using Octgn.Play.Gui;
using Octgn.Scripting;
using Octgn.Utils;

namespace Octgn.Play
{
    using System.Timers;
    using System.Windows.Navigation;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;
    using Octgn.Windows;

    using log4net;
    using Octgn.Controls;

    public partial class PlayWindow
    {
        private bool _isLocal;
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import]
        protected Engine ScriptEngine;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning restore 649

        #region Dependency Properties

        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(PlayWindow),
                                        new UIPropertyMetadata(false));

        public bool ShowSubscribeMessage
        {
            get { return (bool)GetValue(ShowSubscribeMessageProperty); }
            set { SetValue(ShowSubscribeMessageProperty, value); }
        }

        public static readonly DependencyProperty ShowSubscribeMessageProperty =
            DependencyProperty.Register("ShowSubscribeMessage", typeof(bool), typeof(PlayWindow),
                                        new UIPropertyMetadata(false));


        #endregion

        private SolidColorBrush _backBrush = new SolidColorBrush(Color.FromArgb(210, 33, 33, 33));
        private SolidColorBrush _offBackBrush = new SolidColorBrush(Color.FromArgb(55, 33, 33, 33));
        private Storyboard _fadeIn, _fadeOut;
        private static System.Collections.ArrayList fontName = new System.Collections.ArrayList();

        internal GameLog GameLogWindow = new GameLog();

        public PlayWindow(bool islocal = false)
            : base()
        {
            GameLogWindow.Show();
            GameLogWindow.Visibility = Visibility.Hidden;
            Program.Dispatcher = Dispatcher;
            DataContext = Program.GameEngine;
            InitializeComponent();
            _isLocal = islocal;
            //Application.Current.MainWindow = this;
            Version oversion = Assembly.GetExecutingAssembly().GetName().Version;
            Title = "Octgn  version : " + oversion + " : " + Program.GameEngine.Definition.Name;
            Program.GameEngine.ComposeParts(this);
            this.Loaded += OnLoaded;
            this.chat.MouseEnter += ChatOnMouseEnter;
            this.chat.MouseLeave += ChatOnMouseLeave;
            this.playerTabs.MouseEnter += PlayerTabsOnMouseEnter;
            this.playerTabs.MouseLeave += PlayerTabsOnMouseLeave;
            SubscriptionModule.Get().IsSubbedChanged += OnIsSubbedChanged;
            //SubTimer = new Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            //SubTimer.Elapsed += SubTimerOnElapsed;
        }

        private void SubTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(()=>this.SubTimerOnElapsed(sender,elapsedEventArgs)));
                return;
            }
            if (Program.LobbyClient != null && Program.LobbyClient.IsConnected
                && SubscriptionModule.Get().IsSubscribed == false)
            {
                SubscribeMessage.Visibility = Visibility.Visible;
            }
        }

        private void OnIsSubbedChanged(bool b)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    if (!Program.LobbyClient.IsConnected)
                    {
                        ShowSubscribeMessage = false;
                        return;
                    }
                    if (b) ShowSubscribeMessage = false;
                    else
                    {
                        ShowSubscribeMessage = true;
                    }
                }));
        }

        private void PlayerTabsOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            playerTabs.Background = _offBackBrush;
        }

        private void PlayerTabsOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            playerTabs.Background = _backBrush;
        }

        private void ChatOnMouseLeave(object sender, MouseEventArgs mouseEventArgs)
        {
            chat.Background = _offBackBrush;
        }

        private void ChatOnMouseEnter(object sender, MouseEventArgs mouseEventArgs)
        {
            chat.Background = _backBrush;
        }

        private void OnLoaded(object sen, RoutedEventArgs routedEventArgs)
        {
            this.OnIsSubbedChanged(SubscriptionModule.Get().IsSubscribed ?? false);
            this.Loaded -= OnLoaded;
            _fadeIn = (Storyboard)Resources["ImageFadeIn"];
            _fadeOut = (Storyboard)Resources["ImageFadeOut"];

            cardViewer.Source = ExtensionMethods.BitmapFromUri(new Uri(Program.GameEngine.Definition.CardBack));
            if (Program.GameEngine.Definition.CardCornerRadius > 0)
                cardViewer.Clip = new RectangleGeometry();
            AddHandler(CardControl.CardHoveredEvent, new CardEventHandler(CardHovered));
            AddHandler(CardRun.ViewCardModelEvent, new EventHandler<CardModelEventArgs>(ViewCardModel));

            Loaded += (sender, args) => Keyboard.Focus(table);
            // Solve various issues, like disabled menus or non-available keyboard shortcuts

            GroupControl.groupFont = new FontFamily("Segoe UI");
            GroupControl.fontsize = 12;
            chat.output.FontFamily = new FontFamily("Seqoe UI");
            chat.output.FontSize = 12;
            chat.watermark.FontFamily = new FontFamily("Sequo UI");
            MenuConsole.Visibility = Visibility.Visible;
            Log.Info(string.Format("Found #{0} amount of fonts", Program.GameEngine.Definition.Fonts.Count));
            if (Program.GameEngine.Definition.Fonts.Count > 0)
            {
                UpdateFont();
            }
            //SubTimer.Start();

#if(!DEBUG)
            // Show the Scripting console in dev only
            if (Application.Current.Properties["ArbitraryArgName"] == null) return;
            string fname = Application.Current.Properties["ArbitraryArgName"].ToString();
            if (fname != "/developer") return;
#endif

        }

        private void UpdateFont()
        {
            System.Drawing.Text.PrivateFontCollection context = new System.Drawing.Text.PrivateFontCollection();
            System.Drawing.Text.PrivateFontCollection chatname = new System.Drawing.Text.PrivateFontCollection();

            var game = Program.GameEngine.Definition;

            int chatFontsize = 12;
            int contextFontsize = 12;

            foreach (Font font in game.Fonts)
            {
                Log.Info(string.Format("Found font with target({0}) and has path({1})", font.Target, font.Src));
                if (!File.Exists(font.Src)) continue;
                if (font.Target.ToLower().Equals("chat"))
                {
                    Log.Info("Loading font");
                    chatFontsize = font.Size;
                    chatname.AddFontFile(font.Src);
                    if (chatname.Families.Length > 0)
                    {
                        Log.Info("Loaded font into collection");
                    }
                    string font1 = "file:///" + Path.GetDirectoryName(font.Src) + "/#" + chatname.Families[0].Name;
                    Log.Info(string.Format("Loading font with path: {0}", font1).Replace("\\", "/"));
                    chat.output.FontFamily = new FontFamily(font1.Replace("\\", "/"));
                    chat.output.FontSize = chatFontsize;
                    Log.Info(string.Format("Loaded font with source: {0}", chat.output.FontFamily.Source));
                }
                if (font.Target.ToLower().Equals("context"))
                {
                    Log.Info(string.Format("Loading font"));
                    contextFontsize = font.Size;
                    context.AddFontFile(font.Src);
                    if (context.Families.Length > 0)
                    {
                        Log.Info("Loaded font into collection");
                    }
                    string font1 = "file:///" + Path.GetDirectoryName(font.Src) + "/#" + context.Families[0].Name;
                    Log.Info(string.Format("Loading font with path: {0}", font1).Replace("\\", "/"));
                    chat.watermark.FontFamily = new FontFamily(font1.Replace("\\", "/"));
                    GroupControl.groupFont = new FontFamily(font1.Replace("\\", "/"));
                    GroupControl.fontsize = contextFontsize;
                    Log.Info(string.Format("Loaded font with source: {0}", GroupControl.groupFont.Source));
                }
            }
        }

        private void InitializePlayerSummary(object sender, EventArgs e)
        {
            var textBlock = (TextBlock)sender;
            var player = textBlock.DataContext as Player;
            if (player != null && player.IsGlobalPlayer)
            {
                textBlock.Visibility = Visibility.Collapsed;
                return;
            }

            var def = Program.GameEngine.Definition.Player;
            string format = def.IndicatorsFormat;
            if (format == null)
            {
                textBlock.Visibility = Visibility.Collapsed;
                return;
            }

            var multi = new MultiBinding();
            int placeholder = 0;
            format = Regex.Replace(format, @"{#([^}]*)}", delegate(Match match)
                                                              {
                                                                  string name = match.Groups[1].Value;
                                                                  if (player != null)
                                                                  {
                                                                      Counter counter =
                                                                          player.Counters.FirstOrDefault(
                                                                              c => c.Name == name);
                                                                      if (counter != null)
                                                                      {
                                                                          multi.Bindings.Add(new Binding("Value") { Source = counter });
                                                                          return "{" + placeholder++ + "}";
                                                                      }
                                                                  }
                                                                  if (player != null)
                                                                  {
                                                                      Group group =
                                                                          player.IndexedGroups.FirstOrDefault(
                                                                              g => g.Name == name);
                                                                      if (@group != null)
                                                                      {
                                                                          multi.Bindings.Add(new Binding("Count") { Source = @group.Cards });
                                                                          return "{" + placeholder++ + "}";
                                                                      }
                                                                  }
                                                                  return "?";
                                                              });
            multi.StringFormat = format;
            textBlock.SetBinding(TextBlock.TextProperty, multi);
        }

        protected void Close(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            GameLogWindow.RealClose();
            //SubTimer.Stop();
            //SubTimer.Elapsed -= this.SubTimerOnElapsed;
            Close();
        }

        public void ShowGameLog(object sender, RoutedEventArgs routedEventArgs)
        {
            GameLogWindow.Visibility = Visibility.Visible;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (TopMostMessageBox.Show(
                "Are you sure you want to quit?",
                "Octgn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
                e.Cancel = true;
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            WindowManager.PlayWindow = null;
            Program.StopGame();
            // Fix: Don't do this earlier (e.g. in OnClosing) because an animation (e.g. card turn) may try to access Program.Game           
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var loadDirectory = Program.GameEngine.Definition.GetDefaultDeckPath();

            // Show the dialog to choose the file

            var ofd = new OpenFileDialog
                          {
                              Filter = "Octgn deck files (*.o8d) | *.o8d",
                              InitialDirectory = loadDirectory
                          };
            //ofd.InitialDirectory = Program.Game.Definition.DecksPath;
            if (ofd.ShowDialog() != true) return;
            // Try to load the file contents
            try
            {
                var game = GameManager.Get().GetById(Program.GameEngine.Definition.Id);
                var newDeck = new Deck().Load(game, ofd.FileName);
                //DataNew.Entities.Deck newDeck = Deck.Load(ofd.FileName,
                //                         Program.GamesRepository.Games.First(g => g.Id == Program.Game.Definition.Id));
                // Load the deck into the game
                Program.GameEngine.LoadDeck(newDeck);
            }
            catch (DeckException ex)
            {
                TopMostMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                TopMostMessageBox.Show("Octgn couldn't load the deck.\r\nDetails:\r\n\r\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimitedGame(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (LimitedDialog.Singleton == null)
                new LimitedDialog { Owner = this }.Show();
            else
                LimitedDialog.Singleton.Activate();
        }

        private void ToggleFullScreen(object sender, RoutedEventArgs e)
        {
            if (IsFullScreen)
            {
                Topmost = false;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Normal;
                //menuRow.Height = GridLength.Auto;
                this.TitleBarVisibility = Visibility.Visible;
            }
            else
            {
                //Topmost = true;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                //menuRow.Height = new GridLength(2);
                this.TitleBarVisibility = Visibility.Collapsed;
            }
            IsFullScreen = !IsFullScreen;
        }

        private void ResetGame(object sender, RoutedEventArgs e)
        {
            // Prompt for a confirmation
            if (MessageBoxResult.Yes ==
                TopMostMessageBox.Show("The current game will end. Are you sure you want to continue?",
                                "Confirmation", MessageBoxButton.YesNo))
            {
                Program.Client.Rpc.ResetReq();
            }
        }

        protected void MouseEnteredMenu(object sender, RoutedEventArgs e)
        {
            if (!IsFullScreen) return;
            menuRow.Height = GridLength.Auto;
        }

        protected void MouseLeftMenu(object sender, RoutedEventArgs e)
        {
            if (!IsFullScreen) return;
            menuRow.Height = new GridLength(2);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.OriginalSource is TextBox)
                return; // Do not tinker with the keyboard events when the focus is inside a textbox
            if (e.IsRepeat)
                return;
            IInputElement mouseOver = Mouse.DirectlyOver;
            var te = new TableKeyEventArgs(this, e);
            if (mouseOver != null) mouseOver.RaiseEvent(te);
            if (te.Handled) return;

            // If the event was unhandled, check if there's a selection and try to apply a shortcut action to it
            if (!Selection.IsEmpty() && Selection.Source.CanManipulate())
            {
                ActionShortcut match =
                    Selection.Source.CardShortcuts.FirstOrDefault(
                        shortcut => shortcut.Key.Matches(this, te.KeyEventArgs));
                if (match != null)
                {
                    if (match.ActionDef.AsAction().Execute != null)
                        ScriptEngine.ExecuteOnCards(match.ActionDef.AsAction().Execute, Selection.Cards);
                    else if (match.ActionDef.AsAction().BatchExecute != null)
                        ScriptEngine.ExecuteOnBatch(match.ActionDef.AsAction().BatchExecute, Selection.Cards);
                    e.Handled = true;
                    return;
                }
            }

            // The event was still unhandled, try all groups, starting with the table
            table.RaiseEvent(te);
            if (te.Handled) return;
            foreach (Group g in Player.LocalPlayer.Groups.Where(g => g.CanManipulate()))
            {
                ActionShortcut a = g.GroupShortcuts.FirstOrDefault(shortcut => shortcut.Key.Matches(this, e));
                if (a == null) continue;
                if (a.ActionDef.AsAction().Execute != null)
                    ScriptEngine.ExecuteOnGroup(a.ActionDef.AsAction().Execute, g);
                e.Handled = true;
                return;
            }
        }

        private void CardHovered(object sender, CardEventArgs e)
        {
            if (e.Card == null && e.CardModel == null)
            {
                _fadeOut.Begin(outerCardViewer, HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                Point mousePt = Mouse.GetPosition(table);
                if (mousePt.X < 0.4 * clientArea.ActualWidth)
                    outerCardViewer.HorizontalAlignment = cardViewer.HorizontalAlignment = HorizontalAlignment.Right;
                else if (mousePt.X > 0.6 * clientArea.ActualWidth)
                    outerCardViewer.HorizontalAlignment = cardViewer.HorizontalAlignment = HorizontalAlignment.Left;

                var ctrl = e.OriginalSource as CardControl;
                if (e.Card != null)
                {
                    var img =
                        e.Card.GetBitmapImage(
                            ctrl != null && ctrl.IsAlwaysUp
                            || (e.Card.FaceUp || e.Card.PeekingPlayers.Contains(Player.LocalPlayer)));
                    ShowCardPicture(img);
                }
                else
                {
                    var img = ImageUtils.CreateFrozenBitmap(new Uri(e.CardModel.GetPicture()));
                    this.ShowCardPicture(img);
                }
            }
        }

        private void ViewCardModel(object sender, CardModelEventArgs e)
        {
            if (e.CardModel == null)
                _fadeOut.Begin(outerCardViewer, HandoffBehavior.SnapshotAndReplace);
            else
                ShowCardPicture(ImageUtils.CreateFrozenBitmap(new Uri(e.CardModel.GetPicture())));
        }

        private void ShowCardPicture(BitmapSource img)
        {
            cardViewer.Height = img.PixelHeight;
            cardViewer.Width = img.PixelWidth;
            cardViewer.Source = img;

            _fadeIn.Begin(outerCardViewer, HandoffBehavior.SnapshotAndReplace);

            if (cardViewer.Clip == null) return;
            var clipRect = ((RectangleGeometry)cardViewer.Clip);
            double height = Math.Min(cardViewer.MaxHeight, cardViewer.Height);
            double width = cardViewer.Width * height / cardViewer.Height;
            clipRect.Rect = new Rect(new Size(width, height));
            clipRect.RadiusX = clipRect.RadiusY = Program.GameEngine.Definition.CardCornerRadius * height / Program.GameEngine.Definition.CardHeight;
        }

        private void NextTurnClicked(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            var targetPlayer = (Player)btn.DataContext;
            if (Program.GameEngine.TurnPlayer == null || Program.GameEngine.TurnPlayer == Player.LocalPlayer)
                Program.Client.Rpc.NextTurn(targetPlayer);
            else
            {
                Program.Client.Rpc.StopTurnReq(Program.GameEngine.TurnNumber, btn.IsChecked != null && btn.IsChecked.Value);
                if (btn.IsChecked != null) Program.GameEngine.StopTurn = btn.IsChecked.Value;
            }
        }

        private void ActivateChat(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            chat.FocusInput();
        }

        private void ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            //var wnd = new AboutWindow() { Owner = this };
            //wnd.ShowDialog();
            Program.LaunchUrl(AppConfig.WebsitePath);
        }

        private void ConsoleClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var wnd = new InteractiveConsole { Owner = this };
            wnd.Show();
        }

        internal void ShowBackstage(UIElement ui)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    this.table.Visibility = Visibility.Collapsed;
                    this.wndManager.Visibility = Visibility.Collapsed
                        ;
                    this.backstage.Child = ui;
                    this.LimitedBackstage.Visibility = Visibility.Visible;
                    backstage.Visibility = Visibility.Visible;
                }));
        }

        internal void HideBackstage()
        {

            table.Visibility = Visibility.Visible;
            wndManager.Visibility = Visibility.Visible;
            LimitedBackstage.Visibility = Visibility.Collapsed;
            backstage.Visibility = Visibility.Collapsed;
            backstage.Child = null;

            Keyboard.Focus(table); // Solve various issues, like disabled menus or non-available keyboard shortcuts
        }

        #region Limited

        protected void LimitedSaveClicked(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
                          {
                              AddExtension = true,
                              Filter = "Octgn decks|*.o8d",
                              InitialDirectory = Program.GameEngine.Definition.GetDefaultDeckPath()
                          };
            if (!sfd.ShowDialog().GetValueOrDefault()) return;

            var dlg = backstage.Child as PickCardsDialog;
            try
            {
                if (dlg != null) dlg.LimitedDeck.Save(GameManager.Get().GetById(Program.GameEngine.Definition.Id), sfd.FileName);
            }
            catch (UserMessageException ex)
            {
                TopMostMessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected void LimitedOkClicked(object sender, EventArgs e)
        {
            var dlg = backstage.Child as PickCardsDialog;
            if (dlg != null) Program.GameEngine.LoadDeck(dlg.LimitedDeck);
            HideBackstage();
        }

        protected void LimitedCancelClicked(object sender, EventArgs e)
        {
            Program.Client.Rpc.CancelLimitedReq();
            HideBackstage();
        }

        #endregion

        private void KillJoshJohnson(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var s = sender as FrameworkElement;
            if (s == null) return;
            var document = s.DataContext as Document;
            if (document == null) return;
            var wnd = new RulesWindow(document) { Owner = this };
            wnd.ShowDialog();

        }

        private bool chatIsMaxed = false;

        private void ChatSplitDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (chatIsMaxed)
            {
                ChatGridEmptyPart.Height = new GridLength(100, GridUnitType.Star);
                ChatGridChatPart.Height = new GridLength(playerTabs.ActualHeight);
                ChatSplit.DragIncrement = 1;
                chatIsMaxed = false;
            }
            else
            {
                ChatGridEmptyPart.Height = new GridLength(0, GridUnitType.Star);
                ChatGridChatPart.Height = new GridLength(100, GridUnitType.Star);
                ChatSplit.DragIncrement = 10000;
                chatIsMaxed = true;
            }
        }

        private void MenuChangeBackgroundFromFileClick(object sender, RoutedEventArgs e)
        {
            var sub = SubscriptionModule.Get().IsSubscribed ?? false;
            if (!sub)
            {
                TopMostMessageBox.Show("You must be subscribed to do that.", "OCTGN", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var fo = new OpenFileDialog();
            fo.Filter = "All Images|*.BMP;*.JPG;*.JPEG;*.PNG|BMP Files: (*.BMP)|*.BMP|JPEG Files: (*.JPG;*.JPEG)|*.JPG;*.JPEG|PNG Files: (*.PNG)|*.PNG";
            if ((bool)fo.ShowDialog())
            {
                if (File.Exists(fo.FileName))
                {
                    this.table.SetBackground(fo.FileName, "uniformToFill");
                    Prefs.DefaultGameBack = fo.FileName;
                }
            }
        }

        private void MenuChangeBackgroundReset(object sender, RoutedEventArgs e)
        {
            this.table.ResetBackground();
            Prefs.DefaultGameBack = "";
        }

        private void SubscribeNavigate(object sender, RequestNavigateEventArgs e)
        {
            var url = SubscriptionModule.Get().GetSubscribeUrl(new SubType() { Description = "", Name = "" });
            if (url != null)
            {
                Program.LaunchUrl(url);
            }
        }
    }

    internal class CanPlayConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var turnPlayer = values[0] as Player;
            var player = values[1] as Player;

            string styleKey;
            if (player == Player.GlobalPlayer)
                styleKey = "InvisibleButton";
            else if (turnPlayer == null)
                styleKey = "PlayButton";
            else if (turnPlayer == Player.LocalPlayer)
                styleKey = "PlayButton";
            else
                styleKey = turnPlayer == player ? "PauseButton" : "InvisibleButton";
            return Application.Current.FindResource(styleKey);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class ScaleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            var d = (double)value;
            double scale = double.Parse((string)parameter, CultureInfo.InvariantCulture);
            return d * scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}