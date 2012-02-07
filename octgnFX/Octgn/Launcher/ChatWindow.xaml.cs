using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private bool _realClose;
        private Boolean justScrolledToBottom;

        public ChatWindow(long id)
        {
            InitializeComponent();
            Users = new List<User>();
            ID = id;
            if (ID == 0)
                miLeaveChat.IsEnabled = false;
            Program.lobbyClient.OnUserStatusChanged += LobbyClientOnOnUserStatusChanged;
            var cm = new ContextMenu();
            var mi = new MenuItem();
            mi.Header = "Add to friends list";
            mi.Click += mi_Click;
            cm.Items.Add(mi);
            listBox1.ContextMenu = cm;

            richTextBox1.Document.LineHeight = 2;
        }

        public long ID { get; private set; }
        public List<User> Users { get; private set; }

        private void LobbyClientOnOnUserStatusChanged(UserStatus eve, User user)
        {
            Program.lobbyClient.Chatting.UserStatusChange(ID, user, eve);
            ResetUserList();
        }

        public void ChatEvent(ChatRoom cr, Chatting.ChatEvent e, User user, object data)
        {
            Chatting_eChatEvent(cr, e, user, data);
        }

        private void Chatting_eChatEvent(ChatRoom cr, Chatting.ChatEvent e, User user, object data)
        {
            if (cr.ID == ID)
            {
                Dispatcher.Invoke(new Action(() =>
                                                 {
                                                     switch (e)
                                                     {
                                                         case Chatting.ChatEvent.ChatMessage:
                                                             {
                                                                 Brush b = Brushes.Black;
                                                                 if (user.Uid == Program.lobbyClient.Me.Uid)
                                                                     b = Brushes.Blue;

                                                                 Run r = getUserRun(user.DisplayName,
                                                                                    "[" + user.DisplayName + "] : ");
                                                                 r.Foreground = b;
                                                                 var mess = data as string;
                                                                 AddChatText(r, mess);
                                                                 break;
                                                             }
                                                         case Chatting.ChatEvent.MeJoinedChat:
                                                             {
                                                                 ResetUserList();
                                                                 break;
                                                             }
                                                         case Chatting.ChatEvent.UserJoinedChat:
                                                             {
                                                                 string reg =
                                                                     Registry.ReadValue("Options_HideLoginNotifications");
                                                                 if (reg == "false" || reg == null)
                                                                 {
                                                                     var r = new Run("#" + user.DisplayName + ": ");
                                                                     Brush b = Brushes.DarkGray;
                                                                     r.ToolTip = DateTime.Now.ToLongTimeString() + " " +
                                                                                 DateTime.Now.ToLongDateString();
                                                                     r.Foreground = b;
                                                                     AddChatText(r, "Joined the chat.", b);
                                                                     ResetUserList();
                                                                 }
                                                                 break;
                                                             }
                                                         case Chatting.ChatEvent.UserLeftChat:
                                                             {
                                                                 string reg =
                                                                     Registry.ReadValue("Options_HideLoginNotifications");
                                                                 if (reg == "false" || reg == null)
                                                                 {
                                                                     var r = new Run("#" + user.DisplayName + ": ");
                                                                     Brush b = Brushes.LightGray;
                                                                     r.ToolTip = DateTime.Now.ToLongTimeString() + " " +
                                                                                 DateTime.Now.ToLongDateString();
                                                                     r.Foreground = b;
                                                                     AddChatText(r, "Left the chat.", b);
                                                                     ResetUserList();
                                                                 }
                                                                 break;
                                                             }
                                                     }
                                                 }));
            }
        }

        private void AddChatText(Run headerRun, string chat, Brush b = null)
        {
            if (b == null) b = Brushes.Black;
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
                    justScrolledToBottom = false;
                }
                else
                {
                    if (!justScrolledToBottom)
                    {
                        var pa = new Paragraph();
                        var ru = new Run("------------------------------");
                        ru.Foreground = Brushes.Red;
                        pa.Inlines.Add(new Bold(ru));
                        richTextBox1.Document.Blocks.Add(pa);
                        justScrolledToBottom = true;
                    }
                }
            }
            //----------------------------------------------------------------------------

            var p = new Paragraph();
            p.Inlines.Add(headerRun);
            if (chat.Contains("\n"))
            {
                String[] lines = chat.Split(new char[] {'\n'});
                foreach (String line in lines)
                {
                    String[] words = line.Split(new char[] {' '});
                    foreach (String word in words)
                    {
                        Inline inn = StringToRun(word, b);

                        if (inn != null)
                            p.Inlines.Add(inn);
                        p.Inlines.Add(new Run(" "));
                    }
                    //p.Inlines.Add(new Run("\n"));
                }
            }
            else
            {
                String[] words = chat.Split(new char[] {' '});
                foreach (String word in words)
                {
                    Inline inn = StringToRun(word, b);

                    if (inn != null)
                        p.Inlines.Add(inn);
                    p.Inlines.Add(new Run(" "));
                }
            }
            richTextBox1.Document.Blocks.Add(p);
            if (richTextBox1.Document.Blocks.Count > 200)
            {
                try
                {
                    richTextBox1.Document.Blocks.Remove(richTextBox1.Document.Blocks.FirstBlock);
                }
                catch (Exception)
                {
                }
            }
            if (rtbatbottom)
                richTextBox1.ScrollToEnd();
        }

        public Inline StringToRun(String s, Brush b)
        {
            Inline ret = null;
            String strUrlRegex =
                "(?i)\\b((?:[a-z][\\w-]+:(?:/{1,3}|[a-z0-9%])|www\\d{0,3}[.]|[a-z0-9.\\-]+[.][a-z]{2,4}/)(?:[^\\s()<>]+|\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\))+(?:\\(([^\\s()<>]+|(\\([^\\s()<>]+\\)))*\\)|[^\\s`!()\\[\\]{};:'\".,<>?«»“”‘’]))";
            var reg = new Regex(strUrlRegex);
            s = s.Trim();
            //b = Brushes.Black;
            Inline r = new Run(s);
            if (reg.IsMatch(s))
            {
                b = Brushes.LightBlue;
                var h = new Hyperlink(r);
                h.RequestNavigate += h_RequestNavigate;
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
                        var ul = new Underline(r);
                    }
                }
                ret = h;
            }
            else
            {
                if (s.Equals(Program.lobbyClient.Me.DisplayName))
                {
                    b = Brushes.Blue;
                    ret = new Bold(r);
                }
                else
                {
                    Boolean fUser = false;
                    foreach (User u in listBox1.Items)
                    {
                        if (u.DisplayName == s)
                        {
                            b = Brushes.LightGreen;
                            ret = new Bold(r);
                            ret.ToolTip = "Click to whisper";
                            r.Cursor = Cursors.Hand;
                            r.Background = Brushes.White;
                            r.MouseEnter +=
                                delegate { r.Background = new RadialGradientBrush(Colors.DarkGray, Colors.WhiteSmoke); };
                            r.MouseLeave += delegate { r.Background = Brushes.White; };
                            fUser = true;
                            break;
                        }
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

        private void h_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var hl = (Hyperlink) sender;
            string navigateUri = hl.NavigateUri.ToString();
            try
            {
                Process.Start(new ProcessStartInfo(navigateUri));
            }
            catch
            {
            }
            e.Handled = true;
        }

        private Run getUserRun(String user, string fulltext)
        {
            var r = new Run(fulltext);
            r.ToolTip = DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "\nClick to whisper " +
                        user;
            r.Cursor = Cursors.Hand;
            r.Background = Brushes.White;
            r.MouseEnter += delegate { r.Background = new RadialGradientBrush(Colors.DarkGray, Colors.WhiteSmoke); };
            r.MouseLeave += delegate { r.Background = Brushes.White; };
            return r;
        }

        private void ResetUserList()
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 ChatRoom cr = Program.lobbyClient.Chatting.GetChatRoomFromRID(ID);
                                                 if (cr != null)
                                                 {
                                                     listBox1.Items.Clear();
                                                     Users = new List<User>();

                                                     foreach (User u in cr.GetUserList())
                                                     {
                                                         listBox1.Items.Add(u);
                                                         Users.Add(u);
                                                     }
                                                 }
                                             }));
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            var s = e.Data.GetData(typeof (String)) as String;
            if (s != null)
            {
                int uid;
                if (Int32.TryParse(s, out uid))
                {
                    //BUG Should be pulling from FriendList
                    User u = Program.lobbyClient.GetFriendFromUID(uid);
                    if (u != null && (u.Status != UserStatus.Offline || u.Status != UserStatus.Unknown))
                    {
                        Program.lobbyClient.Chatting.AddUserToChat(u, ID);
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.Chatting.eChatEvent += Chatting_eChatEvent;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.Chatting.LeaveChatRoom(ID);
            Program.lobbyClient.Chatting.eChatEvent -= Chatting_eChatEvent;
            Program.ChatWindows.RemoveAll(r => r.ID == ID);
            Program.lobbyClient.OnUserStatusChanged -= LobbyClientOnOnUserStatusChanged;
            var cl = Program.ClientWindow.frame1.Content as ContactList;
            if (cl != null)
                cl.RefreshList();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox1.Text.Trim().Length > 0)
                {
                    Program.lobbyClient.Chatting.SendChatMessage(ID, textBox1.Text);
                    textBox1.Text = "";
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!_realClose)
            {
                e.Cancel = true;
                Hide();
            }
        }

        public void CloseChatWindow()
        {
            _realClose = true;
            Close();
        }

        private void miLeaveChat_Click(object sender, RoutedEventArgs e)
        {
            _realClose = true;
            Close();
        }

        private void listBox1_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void mi_Click(object sender, RoutedEventArgs e)
        {
            var u = listBox1.SelectedItem as User;
            if (u != null)
                Program.lobbyClient.AddFriend(u.Email);
        }
    }
}