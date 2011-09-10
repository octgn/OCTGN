using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IrcDotNet;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Octgn.IRC
{
  public partial class IrcWindow : Window
  {
    private IrcClient ircClient;
    private IrcChannel octgnChannel;
    private UserCollection users;
    private ObservableCollection<MessageSource> tabSource;

    public IrcWindow()
    {
      InitializeComponent();
      tabSource = new ObservableCollection<MessageSource>();

      Loaded += Connect;
      Unloaded += Disconnect;
    }

    private void InputKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Enter) return;
      e.Handled = true;
      var input = (TextBox)sender;
      
      if (tabs.SelectedIndex >= 0)
      {
        var activeTab = tabSource[tabs.SelectedIndex];
        ircClient.LocalUser.SendMessage(activeTab.IrcTarget, input.Text);
        DisplayLine(ircClient.LocalUser.NickName, input.Text, activeTab.IrcTarget);
      }

      input.Clear();
    }

    private MessageSource OpenTab(IIrcMessageTarget ircTarget)
    {
      var model = new MessageSource(ircTarget);
      tabSource.Add(model);
      var item = new TabItem { DataContext = model };
      var rtb = (FindResource("outputTemplate") as DataTemplate).LoadContent() as RichTextBox;
      model.OutputBox = rtb;
      item.Content = rtb;
      tabs.Items.Add(item);
      tabs.SelectedItem = item;
      return model;
    }

    private void DispatchedDisplayLine(string nick, string text, object target = null)
    {
      Dispatcher.BeginInvoke(new Action<string, string, IIrcMessageTarget>(DisplayLine), nick, text, target);
    }

    private void DisplayLine(string nick, string text, object target = null)
    {
      if (target == null && octgnChannel == null)
      {        
        connectLabel.Foreground = Brushes.Red;
        connectLabel.Text = text;
        return;
      }

      var ircTarget = target as IIrcMessageTarget;
      if (ircTarget == null) ircTarget = octgnChannel;
      if (ircTarget == null) return;

      MessageSource model = tabSource.FirstOrDefault(x => x.IrcTarget == ircTarget);
      if (model == null) model = OpenTab(ircTarget);      

      var outputBox = model.OutputBox;
      var table = (Table)outputBox.Document.Blocks.LastBlock;
      var rowGroup = table.RowGroups[0];
      var row = new TableRow();
      var cell = new TableCell { Style = (Style)FindResource("NickCell") };
      row.Cells.Add(cell);
      if (nick != null) cell.Blocks.Add(new Paragraph { Inlines = {new Run(nick) } });
      cell = new TableCell { Style = (Style)FindResource("TextCell") };
      if (nick == null) cell.FontWeight = FontWeights.Bold;
      cell.Blocks.Add(new Paragraph { Inlines = { new Run(text) } });
      row.Cells.Add(cell);
      rowGroup.Rows.Add(row); 
     
      outputBox.ScrollToEnd();

      if (((TabItem)tabs.SelectedItem).DataContext != model)
        model.IsDirty = true;
    }

    private void OutputBoxLoaded(object sender, RoutedEventArgs e)
    {
      var outputBox = (RichTextBox)sender;
      var model = (MessageSource)outputBox.DataContext;
      model.OutputBox = outputBox;
    }

    #region IRC-related

    private void Connect(object sender, EventArgs e)
    {
      ircClient = new IrcClient();
      string nickname = Properties.Settings.Default.NickName;
      var registration = new IrcUserRegistrationInfo
      {
        NickName = nickname,
        UserName = nickname,
        RealName = nickname
      };

      ircClient.Connected += Connected;
      ircClient.ConnectFailed += ConnectFailed;
      ircClient.Connect("irc.ircstorm.net", 6667, false, registration);      
    }

    private void Connected(object sender, EventArgs e)
    {
      Dispatcher.BeginInvoke(new Action(() =>
        {
          connectLabel.Text = "Connected.\nRegistering your identity...";
        }));

      ircClient.Error += IrcError;
      ircClient.ErrorMessageReceived += DisplayErrorMessage;
      ircClient.ProtocolError += IrcProtocolError;
      ircClient.Registered += Registered;
#if DEBUG
      ircClient.RawMessageReceived += (s, ea) => System.Diagnostics.Debug.WriteLine(ea.RawContent);
#endif
      // Bug in ircdotnet: the channel name is not case insensitive and messages addressed to #OCTGN are lost!
      ircClient.RawMessageReceived += (s, ea) =>
        {
          if (octgnChannel == null) return;
          var target = ea.Message.Parameters.FirstOrDefault();
          if (string.Equals(target, octgnChannel.Name, StringComparison.InvariantCultureIgnoreCase))
            ea.Message.Parameters[0] = octgnChannel.Name;          
        };
    }

    private void Registered(object sender, EventArgs e)
    { 
      ircClient.LocalUser.JoinedChannel += JoinedChannel;      
      ircClient.LocalUser.MessageReceived += DisplayPrivateMessage;
      ircClient.LocalUser.NoticeReceived += DisplayMessage;
      ircClient.Channels.Join("#octgn");
    }

    private void ConnectFailed(object sender, IrcErrorEventArgs e)
    {
      DispatchedDisplayLine(null, "Unable to connect to IRC server:\n" + e.Error.Message);
    }

    private void JoinedChannel(object sender, IrcChannelEventArgs e)
    {
      octgnChannel = e.Channel;
      octgnChannel.MessageReceived += DisplayMessage;
      octgnChannel.NoticeReceived += DisplayMessage;

      users = new UserCollection(this, octgnChannel.Users);
      Dispatcher.BeginInvoke(new Action(() => 
        {
          DisplayLine(null, "You have joined #octgn", octgnChannel);
          connectLabel.Visibility = Visibility.Collapsed;
          usersList.ItemsSource = users.SortedView;
        }));
    }

    private void DisplayMessage(object sender, IrcMessageEventArgs e)
    {
      DispatchedDisplayLine(e.Source.Name, e.Text, octgnChannel);
    }

    private void DisplayPrivateMessage(object sender, IrcMessageEventArgs e)
    {            
      DispatchedDisplayLine(e.Source.Name, e.Text, e.Source);
    }

    private void IrcError(object sender, IrcErrorEventArgs e)
    {
      DispatchedDisplayLine(null, "Unexpected IRC error: \n" + e.Error.Message);
    }

    private void IrcProtocolError(object sender, IrcProtocolErrorEventArgs e)
    {
      DispatchedDisplayLine(null, string.Format("IRC error: \n({0}) {1}", e.Code, e.Message));
    }

    private void DisplayErrorMessage(object sender, IrcErrorMessageEventArgs e)
    {
      DispatchedDisplayLine(null, e.Message);
    }

    private void Disconnect(object sender, EventArgs e)
    {
      if (ircClient.IsConnected)
      {
        ircClient.Quit();
        ircClient.Disconnect();
      }
    }

    #endregion

    #region Tab Management

    private void CloseTabCommand(object sender, ExecutedRoutedEventArgs e)
    {
      var src = (FrameworkElement)e.OriginalSource;
      tabSource.Remove((MessageSource)src.DataContext);
      tabs.Items.Remove(tabs.Items.Cast<TabItem>().First(x => x.DataContext == src.DataContext));
    }

    private void CanCloseTab(object sender, CanExecuteRoutedEventArgs e)
    {
      var src = (FrameworkElement)e.OriginalSource;
      var ircTarget = ((MessageSource)src.DataContext).IrcTarget;
      e.ContinueRouting = false;
      e.CanExecute = ircTarget is IrcUser;
    }

    private void SelectedTabChanged(object sender, EventArgs e)
    {
      var tab = (TabItem)tabs.SelectedItem;
      if (tab == null) return;
      var model = tab.DataContext as MessageSource;
      if (model == null) return;
      model.IsDirty = false;
    }

    #endregion

    #region Users commands

    private void StartPrivateChat(User user)
    {
      var ircTarget = user.IrcUser;

      MessageSource model = tabSource.FirstOrDefault(x => x.IrcTarget == ircTarget);
      if (model == null)
        model = OpenTab(ircTarget);
      else
        tabs.SelectedItem = model;
    }

    private void UserDoubleClicked(object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount == 2)
        PrivateChatClicked(sender, e);      
    }

    private void PrivateChatClicked(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      var element = (FrameworkElement)sender;
      var user = (User)element.DataContext;
      StartPrivateChat(user);
    }

    #endregion
  }
}
