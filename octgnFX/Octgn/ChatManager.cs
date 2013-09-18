namespace Octgn
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using Octgn.Controls;
    using Octgn.Core;
    using Octgn.Extentions;
    using Octgn.Utils;
    using Octgn.Windows;

    using Skylabs.Lobby;

    using log4net;

    public class ChatManager
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static ChatManager SingletonContext { get; set; }

        private static readonly object ChatManagerSingletonLocker = new object();

        public static ChatManager Get()
        {
            lock (ChatManagerSingletonLocker) return SingletonContext ?? (SingletonContext = new ChatManager());
        }

        internal ChatManager()
        {
            Init();
        }

        #endregion Singleton

        internal ChatBar ChatBar;
        internal object Locker = new object();
        
        internal void Init()
        {
            Program.LobbyClient.Chatting.OnCreateRoom += ChattingOnCreateRoom;
        }

        public void Start(ChatBar chatBar)
        {
            Log.Info("ChatManager Start");
            ChatBar = chatBar;
        }

        public void MoveToChatBar(ChatControl chat)
        {
            lock(Locker)
                ChatBar.AddChat(chat);
        }

        public void MoveToWindow(ChatControl chat)
        {
            lock (Locker)
            {
                var win = new ChatWindow(chat);
                WindowManager.ChatWindows.Add(win);
                win.Show();
            }
        }

        private void ChattingOnCreateRoom(object sender, ChatRoom room)
        {
            if (room.GroupUser != null && room.GroupUser.UserName.ToLowerInvariant() == "lobby") return;
            lock (Locker)
            {
                // Check if chat room is in a window
                var rw = WindowManager.ChatWindows.FirstOrDefault(x => x.Room.Rid == room.Rid);
                if (rw != null) // There is a chat window for it
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if (!rw.IsActive)
                            {
                                rw.FlashWindow();
                                Sounds.PlayMessageSound();
                            }
                            if (!rw.IsVisible)
                            {
                                rw.Visibility = Visibility.Visible;
                            }
                        }));
                    return;
                }

                // Check if chat bar has chat room
                var cbi = ChatBar.Items.OfType<ChatBarItem>().FirstOrDefault(x => x.Room.Rid == room.Rid);
                if (cbi != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            if((ChatBar.SelectedItem is ChatBarItem && ChatBar.SelectedItem == cbi) == false)
                                cbi.SetAlert();
                        }));
                    return;
                }

                // If no chat is already active
                if (Prefs.UseWindowsForChat)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { 
                    var win = new ChatWindow(room);
                    WindowManager.ChatWindows.Add(win);
                    win.Show();
                    }));
                }
                else
                {
                    ChatBar.AddChat(room);
                }
                Sounds.PlayMessageSound();
            }
        }
    }
}