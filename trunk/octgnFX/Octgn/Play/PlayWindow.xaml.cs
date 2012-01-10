using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Octgn.Play.Dialogs;
using Octgn.Play.Gui;
using System.Windows.Media;
using System.ComponentModel.Composition;

namespace Octgn.Play
{
  public partial class PlayWindow : Window
  {
#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

    [Import]
    protected Scripting.Engine scriptEngine;


    private Boolean _keyDown = false;
#pragma warning restore 649

    #region Dependency Properties

    public bool IsFullScreen
    {
      get { return (bool)GetValue(IsFullScreenProperty); }
      set { SetValue(IsFullScreenProperty, value); }
    }

    public static readonly DependencyProperty IsFullScreenProperty =
        DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(PlayWindow), new UIPropertyMetadata(false));

    #endregion

    public PlayWindow()
    {
      InitializeComponent();
      //Application.Current.MainWindow = this;

      Program.Game.ComposeParts(this);
    }

    private Storyboard fadeIn, fadeOut;

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);
      Program.Dispatcher = this.Dispatcher;

      fadeIn = (Storyboard)Resources["ImageFadeIn"];
      fadeOut = (Storyboard)Resources["ImageFadeOut"];

      cardViewer.Source = new BitmapImage(new Uri(Program.Game.Definition.CardDefinition.Back));
      if (Program.Game.Definition.CardDefinition.CornerRadius > 0)
        cardViewer.Clip = new RectangleGeometry();
      AddHandler(CardControl.CardHoveredEvent, new CardEventHandler(CardHovered));
      AddHandler(CardRun.ViewCardModelEvent, new EventHandler<CardModelEventArgs>(ViewCardModel));

      Loaded += (sender, args) => Keyboard.Focus(table); // Solve various issues, like disabled menus or non-available keyboard shortcuts

      // Show the Scripting console in dev only
      if (Application.Current.Properties["ArbitraryArgName"] != null)
      {
          string fname = Application.Current.Properties["ArbitraryArgName"].ToString();          
          if (fname == "/developer")
          {              
              Loaded += (sender, args) =>
              {
                  var wnd = new InteractiveConsole { Owner = this };
                  wnd.Show();
              };
          }
      }
    }

    private void InitializePlayerSummary(object sender, EventArgs e)
    {
      var textBlock = (TextBlock)sender;
      var player = textBlock.DataContext as Player;
      if (player.IsGlobalPlayer)
      {
        textBlock.Visibility = Visibility.Collapsed;
        return;
      }

      Octgn.Definitions.PlayerDef def = Program.Game.Definition.PlayerDefinition;
      string format = def.IndicatorsFormat;
      if (format == null)
      { textBlock.Visibility = Visibility.Collapsed; return; }

      var multi = new MultiBinding();
      int placeholder = 0;
      format = Regex.Replace(format, @"{#([^}]*)}", delegate(Match match)
      {
        string name = match.Groups[1].Value;
        var counter = player.Counters.FirstOrDefault(c => c.Name == name);
        if (counter !=  null)
        {
          multi.Bindings.Add(new Binding("Value") { Source = counter });
          return "{" + placeholder++ + "}";
        }
        var group = player.IndexedGroups.FirstOrDefault(g => g.Name == name);
        if (group != null)
        {
          multi.Bindings.Add(new Binding("Count") { Source = group.Cards });
          return "{" + placeholder++ + "}";
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

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      // Fix for this bug: http://wpf.codeplex.com/workitem/14078
      ribbon.IsMinimized = false;

      base.OnClosing(e);
      
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);
        Program.PlayWindow = null;
      Program.StopGame(); // Fix: Don't do this earlier (e.g. in OnClosing) because an animation (e.g. card turn) may try to access Program.Game
    }

    private void Open(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      // Show the dialog to choose the file
      Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
      ofd.Filter = "OCTGN deck files (*.o8d) | *.o8d";
      //ofd.InitialDirectory = Program.Game.Definition.DecksPath;
      ofd.InitialDirectory = Registry.ReadValue("lastFolder");
      if (ofd.ShowDialog() != true) return;
      Registry.WriteValue("lastFolder", System.IO.Path.GetDirectoryName(ofd.FileName));
      // Try to load the file contents
      Data.Deck newDeck;
      try
      {
        newDeck = Data.Deck.Load(ofd.FileName,
          Program.GamesRepository.Games.First(g => g.Id == Program.Game.Definition.Id));
        // Load the deck into the game
        Program.Game.LoadDeck(newDeck);
      }
      catch (Data.DeckException ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      catch (Exception ex)
      {
        MessageBox.Show("OCTGN couldn't load the deck.\r\nDetails:\r\n\r\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        WindowStyle = WindowStyle.SingleBorderWindow;
        WindowState = WindowState.Normal;
        menuRow.Height = GridLength.Auto;        
      }
      else
      {
        Topmost = true;
        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        menuRow.Height = new GridLength(2);
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

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
    }
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);
      if (e.OriginalSource is TextBox) return;  // Do not tinker with the keyboard events when the focus is inside a textbox
      if (e.IsRepeat)
          return;
      IInputElement mouseOver = Mouse.DirectlyOver;
      var te = new TableKeyEventArgs(this, e);
      if (mouseOver != null) mouseOver.RaiseEvent(te);
      if (te.Handled) return;

      // If the event was unhandled, check if there's a selection and try to apply a shortcut action to it
      if (!Selection.IsEmpty() && Selection.Source.CanManipulate())
      {
        var match = Selection.Source.CardShortcuts.FirstOrDefault(shortcut => shortcut.Key.Matches(this, te.KeyEventArgs));
        if (match != null)
        {
          if (match.ActionDef.Execute != null)
            scriptEngine.ExecuteOnCards(match.ActionDef.Execute, Selection.Cards);
          else if (match.ActionDef.BatchExecute != null)
            scriptEngine.ExecuteOnBatch(match.ActionDef.BatchExecute, Selection.Cards);
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
        if (a.ActionDef.Execute != null)
           scriptEngine.ExecuteOnGroup(a.ActionDef.Execute, g);
        e.Handled = true;
        return;
      }
    }

    private void CardHovered(object sender, CardEventArgs e)
    {
      if (e.Card == null && e.CardModel == null)
      {
        fadeOut.Begin(outerCardViewer, HandoffBehavior.SnapshotAndReplace);
      }
      else
      {
        Point mousePt = Mouse.GetPosition(table);
        if (mousePt.X < 0.4 * clientArea.ActualWidth)
          outerCardViewer.HorizontalAlignment = cardViewer.HorizontalAlignment = HorizontalAlignment.Right;
        else if (mousePt.X > 0.6 * clientArea.ActualWidth)
          outerCardViewer.HorizontalAlignment = cardViewer.HorizontalAlignment = HorizontalAlignment.Left;

        var ctrl = e.OriginalSource as CardControl;
        BitmapImage img = e.Card != null ?
          e.Card.GetBitmapImage(ctrl != null && ctrl.IsAlwaysUp ? true : e.Card.FaceUp || e.Card.PeekingPlayers.Contains(Player.LocalPlayer)) :
          ImageUtils.CreateFrozenBitmap(new Uri(e.CardModel.Picture));
        ShowCardPicture(img);
      }
    }

    private void ViewCardModel(object sender, CardModelEventArgs e)
    {
      if (e.CardModel == null)
        fadeOut.Begin(outerCardViewer, HandoffBehavior.SnapshotAndReplace);
      else
        ShowCardPicture( ImageUtils.CreateFrozenBitmap(new Uri(e.CardModel.Picture)));
    }

    private void ShowCardPicture(BitmapImage img)
    {
      cardViewer.Height = img.PixelHeight; cardViewer.Width = img.PixelWidth;
      cardViewer.Source = img;

      fadeIn.Begin(outerCardViewer, HandoffBehavior.SnapshotAndReplace);

      if (cardViewer.Clip != null)
      {
        var cardDef = Program.Game.Definition.CardDefinition;
        var clipRect = ((RectangleGeometry)cardViewer.Clip);
        double height = Math.Min(cardViewer.MaxHeight, cardViewer.Height);
        double width = cardViewer.Width * height / cardViewer.Height;
        clipRect.Rect = new Rect(new Size(width, height));
        clipRect.RadiusX = clipRect.RadiusY = cardDef.CornerRadius * height / cardDef.Height;
      }
    }

    private void NextTurnClicked(object sender, RoutedEventArgs e)
    {
      var btn = (ToggleButton)sender;
      var targetPlayer = (Player)btn.DataContext;
      if (Program.Game.TurnPlayer == null || Program.Game.TurnPlayer == Player.LocalPlayer)
        Program.Client.Rpc.NextTurn(targetPlayer);
      else
      {
        Program.Client.Rpc.StopTurnReq(Program.Game.TurnNumber, btn.IsChecked.Value);
        Program.Game.StopTurn = btn.IsChecked.Value;
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
      var wnd = new AboutWindow() { Owner = this };
      wnd.ShowDialog();
    }

    private void DonateClicked(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      System.Diagnostics.Process.Start("http://www.octgn.net/donate.php");
    }

    internal void ShowBackstage(UIElement ui)
    {
      table.Visibility = Visibility.Collapsed;
      wndManager.Visibility = Visibility.Collapsed;
      backstage.Child = ui;
      backstage.Visibility = Visibility.Visible;
      if (ui is PickCardsDialog)
      {
        limitedTab.Visibility = Visibility.Visible;
        ribbon.SelectedItem = limitedTab;
      }
    }

    internal void HideBackstage()
    {
      limitedTab.Visibility = System.Windows.Visibility.Collapsed;

      table.Visibility = Visibility.Visible;
      wndManager.Visibility = Visibility.Visible;
      backstage.Visibility = Visibility.Collapsed;
      backstage.Child = null;

      Keyboard.Focus(table); // Solve various issues, like disabled menus or non-available keyboard shortcuts
    }

    #region Limited

    protected void LimitedSaveClicked(object sender, EventArgs e)
    {
      var sfd = new Microsoft.Win32.SaveFileDialog
      {
        AddExtension = true,
        Filter = "OCTGN decks|*.o8d",
        InitialDirectory = Program.Game.Definition.DecksPath
      };
      if (!sfd.ShowDialog().GetValueOrDefault()) return;

      var dlg = backstage.Child as PickCardsDialog;
      try
      { 
        dlg.LimitedDeck.Save(sfd.FileName);  
      }
      catch (Exception ex)
      {
        MessageBox.Show("An error occured while trying to save the deck:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    protected void LimitedOkClicked(object sender, EventArgs e)
    {
      var dlg = backstage.Child as PickCardsDialog;
      Program.Game.LoadDeck(dlg.LimitedDeck);
      HideBackstage();
    }

    protected void LimitedCancelClicked(object sender, EventArgs e)
    {
      Program.Client.Rpc.CancelLimitedReq();
      HideBackstage();
    }

    #endregion
  }

  internal class CanPlayConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    { throw new NotImplementedException(); }
  }

  internal class ScaleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == null) return null;
      double d = (double)value;
      double scale = double.Parse((string)parameter, System.Globalization.CultureInfo.InvariantCulture);
      return d * scale;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    { throw new NotImplementedException(); }
  }
}