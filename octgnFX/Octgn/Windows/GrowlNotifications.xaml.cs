using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Windows
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Net;
    using System.Reflection;
    using System.Windows.Input;
    using System.Windows.Threading;

    using log4net;

    using Octgn.Core;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using Octgn.Library.Exceptions;
    using Octgn.Networking;

    using Skylabs.Lobby;

    /// <summary>
    /// Interaction logic for GrowlNotifications.xaml
    /// </summary>
    public partial class GrowlNotifications
    {
        private const byte MAX_NOTIFICATIONS = 4;
        private int count;
        public Notifications Notifications = new Notifications();
        private readonly Notifications buffer = new Notifications();

        public GrowlNotifications()
        {
            InitializeComponent();
            NotificationsControl.DataContext = Notifications;
        }

        public void AddNotification(NotificationBase notification)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => AddNotification(notification)));
                return;
            }
            notification.Id = count++;
            if (Notifications.Count + 1 > MAX_NOTIFICATIONS)
                buffer.Add(notification);
            else
                Notifications.Add(notification);

            //Show window if there're notifications
            if (Notifications.Count > 0 && !IsActive)
            {
                Show();
            }
            Resize();
        }

        public void RemoveNotification(NotificationBase notification)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => RemoveNotification(notification)));
                return;
            }
            if (Notifications.Contains(notification))
                Notifications.Remove(notification);

            if (buffer.Count > 0)
            {
                Notifications.Add(buffer[0]);
                buffer.RemoveAt(0);
            }

            //Close window if there's nothing to show
            Resize();
        }

        private void Resize()
        {
            var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(WindowManager.Main).Handle);
            this.Left = screen.WorkingArea.Right - 300;
            this.Top = screen.WorkingArea.Top;
            this.Height = screen.WorkingArea.Height;
        }

        private void NotificationWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Height != 0.0)
                return;
            var element = sender as Grid;
            RemoveNotification(Notifications.First(n => n.Id == Int32.Parse(element.Tag.ToString())));
        }

        private void OnNotificationWindowMouseUp(object sender, MouseButtonEventArgs e)
        {
            var element = sender as Grid;
            var dal = element.DataContext as NotificationBase;

            new Action(dal.OnClick).BeginInvoke(null, null);
        }
    }

    public abstract class NotificationBase : INotifyPropertyChanged
    {
        private string message;
        public string Message
        {
            get { return message; }

            set
            {
                if (message == value) return;
                message = value;
                OnPropertyChanged("Message");
            }
        }

        private int id;
        public int Id
        {
            get { return id; }

            set
            {
                if (id == value) return;
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private string imageUrl;
        public string ImageUrl
        {
            get { return imageUrl; }

            set
            {
                if (imageUrl == value) return;
                imageUrl = value;
                OnPropertyChanged("ImageUrl");
            }
        }

        private string title;
        public string Title
        {
            get { return title; }

            set
            {
                if (title == value) return;
                title = value;
                OnPropertyChanged("Title");
            }
        }

        public abstract void OnClick();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class GameInviteNotification : NotificationBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        InviteToGame Invite { get; set; }

        HostedGameData HostedGame { get; set; }

        Game Game { get; set; }

        public GameInviteNotification(InviteToGame invite, HostedGameData hostedGame, Game game)
        {
            HostedGame = hostedGame;
            Invite = invite;
            Game = game;
            Title = "Game Invite";
            Message = String.Format("{0} Has invited you to the game '{1}'", invite.From.UserName, hostedGame.Name);
            ImageUrl = game.IconUrl;
        }

        public override void OnClick()
        {
            if (HostedGame.Source == HostedGameSource.Online)
            {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
                {
                    throw new UserMessageException("The game server is currently down. Please try again later.");
                }
            }
            Log.InfoFormat("Starting to join a game {0} {1}", HostedGame.GameGuid, HostedGame.Name);
            Program.IsHost = false;
            var username = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.Me == null
                || Program.LobbyClient.Me.UserName == null) ? Prefs.Nickname : Program.LobbyClient.Me.UserName;
            Program.GameEngine = new GameEngine(Game, username, Invite.Password);
            Program.CurrentOnlineGameName = HostedGame.Name;
            IPAddress hostAddress = HostedGame.IpAddress;
            if (hostAddress == null)
            {
                Log.WarnFormat("Dns Error, couldn't resolve {0}", AppConfig.GameServerPath);
                throw new UserMessageException("There was a problem with your DNS. Please try again.");
            }

            try
            {
                Log.InfoFormat("Creating client for {0}:{1}", hostAddress, HostedGame.Port);
                Program.Client = new ClientSocket(hostAddress, HostedGame.Port);
                Log.InfoFormat("Connecting client for {0}:{1}", hostAddress, HostedGame.Port);
                Program.Client.Connect();
                WindowManager.GrowlWindow.Dispatcher.Invoke(new Action(() =>
                {
                    WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
                    WindowManager.PreGameLobbyWindow.Setup(false, WindowManager.Main);
                }));
            }
            catch (Exception e)
            {
                Log.Warn("Start join game error ", e);
                throw new UserMessageException("Could not connect. Please try again.");
            }
        }
    }

    public class Notifications : ObservableCollection<NotificationBase> { }
}
