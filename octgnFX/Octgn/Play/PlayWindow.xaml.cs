using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
    using Microsoft.Windows.Controls.Ribbon;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using Octgn.Library.Exceptions;
    using log4net;

    public partial class PlayWindow
    {
        private bool _isLocal;
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import] protected Engine ScriptEngine;
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning restore 649

        #region Dependency Properties

        public bool IsFullScreen
        {
            get { return (bool) GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof (bool), typeof (PlayWindow),
                                        new UIPropertyMetadata(false));

        #endregion


        private Storyboard _fadeIn, _fadeOut;
        private static System.Collections.ArrayList fontName = new System.Collections.ArrayList();
        public PlayWindow(bool islocal = false)
            : base()
        {
            Program.Dispatcher = Dispatcher;
            DataContext = Program.GameEngine;
            InitializeComponent();
            _isLocal = islocal;
            //Application.Current.MainWindow = this;
            Version oversion = Assembly.GetExecutingAssembly().GetName().Version;
            Title = "Octgn  version : " + oversion + " : " + Program.GameEngine.Definition.Name;
            Program.GameEngine.ComposeParts(this);            
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sen, RoutedEventArgs routedEventArgs)
        {

            _fadeIn = (Storyboard) Resources["ImageFadeIn"];
            _fadeOut = (Storyboard) Resources["ImageFadeOut"];

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
            Console.Visibility = Visibility.Visible;
            Log.Info(string.Format("Found #{0} amount of fonts", Program.GameEngine.Definition.Fonts.Count));
            if (Program.GameEngine.Definition.Fonts.Count > 0)
            {
                UpdateFont();
            }

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
                    chat.watermark.FontFamily = new FontFamily(font1.Replace("\\","/"));
                    GroupControl.groupFont = new FontFamily(font1.Replace("\\", "/"));
                    GroupControl.fontsize = contextFontsize;
                    Log.Info(string.Format("Loaded font with source: {0}", GroupControl.groupFont.Source));
                }
            }
        }

        private void InitializePlayerSummary(object sender, EventArgs e)
        {
            var textBlock = (TextBlock) sender;
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
                                                                          multi.Bindings.Add(new Binding("Value")
                                                                                                 {Source = counter});
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
                                                                          multi.Bindings.Add(new Binding("Count")
                                                                                                 {Source = @group.Cards});
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

            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to quit?",
                "Octgn",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
                e.Cancel = true;
            // Fix for this bug: http://wpf.codeplex.com/workitem/14078
            ribbon.IsMinimized = false;
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
                var newDeck = new Deck().Load(game,ofd.FileName);
                //DataNew.Entities.Deck newDeck = Deck.Load(ofd.FileName,
                //                         Program.GamesRepository.Games.First(g => g.Id == Program.Game.Definition.Id));
                // Load the deck into the game
                Program.GameEngine.LoadDeck(newDeck);
            }
            catch (DeckException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Octgn couldn't load the deck.\r\nDetails:\r\n\r\n" + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimitedGame(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (LimitedDialog.Singleton == null)
                new LimitedDialog {Owner = this}.Show();
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
                menuRow.Height = GridLength.Auto;
                this.TitleBarVisibility = Visibility.Visible;
            }
            else
            {
                Topmost = true;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
                menuRow.Height = new GridLength(2);
                this.TitleBarVisibility = Visibility.Collapsed;
            }
            IsFullScreen = !IsFullScreen;
        }

        private void ResetGame(object sender, RoutedEventArgs e)
        {
            // Prompt for a confirmation
            if (MessageBoxResult.Yes ==
                MessageBox.Show("The current game will end. Are you sure you want to continue?",
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
                if (mousePt.X < 0.4*clientArea.ActualWidth)
                    outerCardViewer.HorizontalAlignment = cardViewer.HorizontalAlignment = HorizontalAlignment.Right;
                else if (mousePt.X > 0.6*clientArea.ActualWidth)
                    outerCardViewer.HorizontalAlignment = cardViewer.HorizontalAlignment = HorizontalAlignment.Left;

                var ctrl = e.OriginalSource as CardControl;                  
                if (e.Card != null )
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
            var clipRect = ((RectangleGeometry) cardViewer.Clip);
            double height = Math.Min(cardViewer.MaxHeight, cardViewer.Height);
            double width = cardViewer.Width*height/cardViewer.Height;
            clipRect.Rect = new Rect(new Size(width, height));
            clipRect.RadiusX = clipRect.RadiusY = Program.GameEngine.Definition.CardCornerRadius * height / Program.GameEngine.Definition.CardHeight;
        }

        private void NextTurnClicked(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton) sender;
            var targetPlayer = (Player) btn.DataContext;
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
            Process.Start(AppConfig.WebsitePath);
        }

        private void ConsoleClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var wnd = new InteractiveConsole {Owner = this};
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
                    this.backstage.Visibility = Visibility.Visible;
                    if (!(ui is PickCardsDialog)) return;
                    this.limitedTab.Visibility = Visibility.Visible;
                    this.ribbon.SelectedItem = this.limitedTab;
                }));
        }

        internal void HideBackstage()
        {
            limitedTab.Visibility = Visibility.Collapsed;

            table.Visibility = Visibility.Visible;
            wndManager.Visibility = Visibility.Visible;
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
                MessageBox.Show(ex.Message, "Error",MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void RibbonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tab = ribbon.SelectedItem as RibbonTab;
            if (tab == null) return;
            tab.HeaderStyle = Resources["SelectedHeaderColor"] as Style;
            foreach (var t in ribbon.Items.OfType<RibbonTab>())
            {
                if (!(Equals(t, tab)))
                {
                    t.HeaderStyle = Resources["NormalHeaderColor"] as Style;
                }

            }
        }

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
            var d = (double) value;
            double scale = double.Parse((string) parameter, CultureInfo.InvariantCulture);
            return d*scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}