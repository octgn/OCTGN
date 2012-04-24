using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Octgn.Launcher;
using Skylabs.Lobby;
using agsXMPP;
using Uri = System.Uri;

namespace Octgn.Windows
{
    /// <summary>
    ///   Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow
    {
        private Boolean _justScrolledToBottom;
        private bool _realClose;
        public NewChatRoom Room;
        public long Id { get { return Room.RID; } }
        public bool IsLobbyChat { get; private set; }

        private Brush _bBackColor = Brushes.White;
        private Brush _bMeColor = Brushes.Blue;
        private Brush _bOtherColor = Brushes.Black;
        private Brush _bTopicColor = Brushes.Green;
        private Brush _bTopicBackColor = Brushes.White;
        private Brush _bErrorColor = Brushes.Red;

        public ChatWindow(NewChatRoom room)
        {
            InitializeComponent();
            Room = room;
            var cm = new ContextMenu();
            var mi = new MenuItem {Header = "Add to friends list"};
            mi.Click += MiClick;
            cm.Items.Add(mi);
            listBox1.ContextMenu = cm;
            richTextBox1.Document.LineHeight = 2;
            Room.OnMessageRecieved += RoomOnOnMessageRecieved;
            Room.OnUserListChange += RoomOnOnUserListChange;
            IsLobbyChat = (room.GroupUser != null && room.GroupUser.User.User == "lobby");
            if (!room.IsGroupChat || IsLobbyChat) miLeaveChat.IsEnabled = false;
            ResetUserList();
        }

        private void RoomOnOnUserListChange(object sender,List<NewUser> users)
        {
            ResetUserList();

        }

        private void RoomOnOnMessageRecieved(object sender , NewUser from , string message, DateTime rTime, Client.LobbyMessageType mType)
        {
            Dispatcher.Invoke(new Action(()=>
                {
                    switch(mType)
                    {
                        case Client.LobbyMessageType.Standard:
                        {
                            Brush b = _bOtherColor;

                            if (from.Equals(Program.LobbyClient.Me)) b = _bMeColor;
                            Run r = GetUserRun(from.User.User,
                                               "[" + from.User.User + "] : ", rTime);
                            r.Foreground = b;
                            AddChatText(r, message);
                            if (this.Visibility != Visibility.Visible
                               && Program.LobbyClient.Me.Status != UserStatus.DoNotDisturb && !IsLobbyChat) Show();
                            break;
                        }
                        case Client.LobbyMessageType.Topic:
                        {
                            Run r = new Run("");
                            r.Foreground = _bTopicColor;
                            r.Background = _bTopicBackColor;
                            AddChatText(r, message, _bTopicColor, _bTopicBackColor);
                            break;
                        }
                        case Client.LobbyMessageType.Error:
                        {
                            Run r = new Run("");
                            r.Foreground = _bErrorColor;
                            AddChatText(r, message, _bErrorColor);
                            break;
                        }
                    }

                }));
        }


        private void AddChatText(Inline headerRun, string chat, Brush fB = null, Brush bB =null)
        {
            if (fB == null) fB = _bOtherColor;
            if (bB == null) bB = _bBackColor;
            bool rtbatbottom = false;

            // bool firstAutoScroll = true; // never used 

            //check to see if the richtextbox is scrolled to the bottom.
            //----------------------------------------------------------------------------------
            double dVer = richTextBox1.VerticalOffset;

            //get the vertical size of the scrollable content area
            double dViewport = richTextBox1.ViewportHeight;

            //get the vertical size of the visible content area
            double dExtent = richTextBox1.ExtentHeight;

            if (Math.Abs(dVer - 0) < double.Epsilon && dViewport < dExtent)
            {
                rtbatbottom = true;
            }

            if (Math.Abs(dVer - 0) > double.Epsilon)
            {
                if (Math.Abs(dVer + dViewport - dExtent) < double.Epsilon)
                {
                    rtbatbottom = true;
                    _justScrolledToBottom = false;
                }
                else
                {
                    if (!_justScrolledToBottom)
                    {
                        var pa = new Paragraph();
                        var ru = new Run("------------------------------") {Foreground = Brushes.Red};
                        pa.Inlines.Add(new Bold(ru));
                        richTextBox1.Document.Blocks.Add(pa);
                        _justScrolledToBottom = true;
                    }
                }
            }
            //----------------------------------------------------------------------------

            var p = new Paragraph();
            p.Inlines.Add(headerRun);
            if (chat.Contains("\n"))
            {
                String[] lines = chat.Split(new[] {'\n'});
                foreach (
                    Inline inn in
                        lines.Select(line => line.Split(new[] {' '})).SelectMany(
                            words => words.Select(word => StringToRun(word, fB))))
                {
                    if (inn != null)
                        p.Inlines.Add(inn);
                    p.Inlines.Add(new Run(" "));
                }
            }
            else
            {
                String[] words = chat.Split(new[] {' '});
                foreach (Inline inn in words.Select(word => StringToRun(word, fB)))
                {
                    if (inn != null)
                        p.Inlines.Add(inn);
                    p.Inlines.Add(new Run(" "));
                }
            }
            p.Background = bB;
            richTextBox1.Document.Blocks.Add(p);
            if (richTextBox1.Document.Blocks.Count > 1000)
            {
                try
                {
                    richTextBox1.Document.Blocks.Remove(richTextBox1.Document.Blocks.FirstBlock);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
            if (rtbatbottom)
                richTextBox1.ScrollToEnd();
        }

        public Inline StringToRun(String s, Brush b)
        {
            Inline ret = null;
            const string strUrlRegex =
                "(?i)\\b((?:[a-z][\\w-]+:(?:/{1,3}|[a-z0-9%])|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))";
            var reg = new Regex(strUrlRegex);
            s = s.Trim();
            //b = Brushes.Black;
            Inline r = new Run(s);
            if (reg.IsMatch(s))
            {
                b = Brushes.LightBlue;
                var h = new Hyperlink(r);
                h.RequestNavigate += HRequestNavigate;
                try
                {
                    h.NavigateUri = new Uri(s);
                }
                catch (UriFormatException)
                {
                    s = "http://" + s;
                    try
                    {
                        h.NavigateUri = new Uri(s);
                    }
                    catch (Exception)
                    {
                        r.Foreground = b;
                        //var ul = new Underline(r);
                    }
                }
                ret = h;
            }
            else
            {
                if (s.Equals(Program.LobbyClient.Username))
                {
                    b = Brushes.Blue;
                    ret = new Bold(r);
                }
                else
                {
                    Boolean fUser = false;
                    if (listBox1.Items.Cast<NewUser>().Any(u => u.User.User == s))
                    {
                        b = Brushes.LightGreen;
                        ret = new Bold(r) {ToolTip = "Click to whisper"};
                        r.Cursor = Cursors.Hand;
                        r.Background = Brushes.White;
                        r.MouseEnter +=
                            delegate { r.Background = new RadialGradientBrush(Colors.DarkGray, Colors.WhiteSmoke); };
                        r.MouseLeave += delegate { r.Background = Brushes.White; };
                        fUser = true;
                    }
                    if (!fUser)
                    {
                        ret = new Run(s);
                    }
                }
            }
            ret.Foreground = b;
            return ret;
        }

        private static void HRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var hl = (Hyperlink) sender;
            string navigateUri = hl.NavigateUri.ToString();
            try
            {
                Process.Start(new ProcessStartInfo(navigateUri));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                if (Debugger.IsAttached) Debugger.Break();
            }
            e.Handled = true;
        }

        private static Run GetUserRun(String user, string fulltext, DateTime rTime)
        {
            var r = new Run(fulltext)
                        {
                            ToolTip =
                                rTime.ToLongTimeString() + " " + rTime.ToLongDateString() +
                                "\nClick to whisper " +
                                user,
                            Cursor = Cursors.Hand,
                            Background = Brushes.White
                        };
            r.MouseEnter += delegate { r.Background = new RadialGradientBrush(Colors.DarkGray, Colors.WhiteSmoke); };
            r.MouseLeave += delegate { r.Background = Brushes.White; };
            r.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler
                (delegate{
                    if (user == Program.LobbyClient.Me.User.User) return;
                    var room = Program.LobbyClient.Chatting.GetRoom(new NewUser(new Jid(user + "@skylabsonline.com")));
                    var cw = Program.ChatWindows.SingleOrDefault(x => x.Room.RID == room.RID);
                    if(cw != null)
                        cw.Show();

                }));
            return r;
        }

        private void ResetUserList()
        {
            Dispatcher.Invoke(new Action(() =>
                                             {

                                                this.Title = (Room.IsGroupChat)
                                                                 ? Room.GroupUser.User.User
                                                                 : "Chat with: "
                                                                   + Room.Users.SingleOrDefault(x => x.User.Bare != Program.LobbyClient.Me.User.Bare).User.User;
                                                                 listBox1.Items.Clear();
                                                 foreach (var u in Room.Users)
                                                 {
                                                     listBox1.Items.Add(u);
                                                 }
                                             }));
        }

        private void WindowDrop(object sender, DragEventArgs e)
        {
            var s = e.Data.GetData(typeof (String)) as String;
            if (s == null) return;
            Room.AddUser(new NewUser(new Jid(s)));
            if (Room.IsGroupChat && Room.GroupUser.User.User != "lobby")
            {
                miLeaveChat.IsEnabled = true;
                var cl = Program.MainWindow.frame1.Content as ContactList;
                if(cl != null)
                    cl.RefreshList();
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

            this.Title = (Room.IsGroupChat)
                             ? Room.GroupUser.User.User
                             : "Chat with: "
                               + Room.Users.SingleOrDefault(x => x.User.Bare != Program.LobbyClient.Me.User.Bare).User.User;
            ResetUserList();
        }

        private void WindowUnloaded(object sender, RoutedEventArgs e)
        {
            Program.ChatWindows.RemoveAll(r => r.Id == Id);
            var cl = Program.MainWindow.frame1.Content as ContactList;
            Room.OnMessageRecieved -= RoomOnOnMessageRecieved;
            Room.OnUserListChange -= RoomOnOnUserListChange;
            Room.LeaveRoom();
            if (cl != null)
                cl.RefreshList();
        }

        private void TextBox1KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (textBox1.Text.Trim().Length <= 0) return;
            Room.SendMessage(textBox1.Text);
            if(!Room.IsGroupChat && textBox1.Text[0] != '/')
                RoomOnOnMessageRecieved(this,Program.LobbyClient.Me,textBox1.Text,DateTime.Now,Client.LobbyMessageType.Standard);
            //Program.LobbyClient.Chatting.SendChatMessage(Id, textBox1.Text);
            textBox1.Text = "";
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_realClose) return;
            e.Cancel = true;
            Hide();
        }

        public void CloseChatWindow()
        {
            Dispatcher.Invoke(new Action(() =>
                                         {
                                            _realClose = true;
                                            Close();                                             
                                         }));

        }

        private void MiLeaveChatClick(object sender, RoutedEventArgs e)
        {
            _realClose = true;
            Close();
        }

        private void ListBox1MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void MiClick(object sender, RoutedEventArgs e)
        {
            var u = listBox1.SelectedItem as NewUser;
            if (u != null)
                Program.LobbyClient.SendFriendRequest(u.User.Bare);
        }
    }
}