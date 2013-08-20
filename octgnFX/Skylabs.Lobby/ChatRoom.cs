// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewChatRoom.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   The new chat room.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Skylabs.Lobby
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using agsXMPP;
    using agsXMPP.protocol.client;
    using agsXMPP.protocol.extensions.chatstates;
    using agsXMPP.protocol.x;

    /// <summary>
    /// The new chat room.
    /// </summary>
    public class ChatRoom : IDisposable, IEquatable<ChatRoom>, IEqualityComparer
    {
        #region Fields

        /// <summary>
        /// The client.
        /// </summary>
        private Client client;

        private DateTime? lastRecieveTime;

        private int messageCount = 0;

        private readonly Timer stateTimer;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRoom"/> class.
        /// </summary>
        /// <param name="rid">
        /// The rid.
        /// </param>
        /// <param name="c">
        /// The c.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        internal ChatRoom(long rid, Client c, User user)
        {
            stateTimer = new Timer(StateUpdate,null,1000,1000);
            lastRecieveTime = DateTime.Now;
            this.State = ChatRoomState.Disconnected;
            this.Rid = rid;
            this.Users = new List<User>();
            this.AdminList = new List<User>();
            this.OwnerList = new List<User>();
            this.ModeratorList = new List<User>();
            this.VoiceList = new List<User>();
            this.client = c;
            if (user.Server == "conference." + this.client.Config.ChatHost)
            {
                this.IsGroupChat = true;
                this.GroupUser = new User(new Jid(user.FullUserName));
                this.client.MucManager.JoinRoom(this.GroupUser.JidUser, this.client.Me.UserName);
                this.client.MucManager.RequestModeratorList(this.GroupUser.JidUser);
                this.client.MucManager.RequestAdminList(this.GroupUser.JidUser);
                this.client.MucManager.RequestOwnerList(this.GroupUser.JidUser);
                this.client.MucManager.RequestVoiceList(this.GroupUser.JidUser);
            }
            else
            {
                this.AddUser(user);
            }

            this.State = ChatRoomState.Connecting;
            this.AddUser(this.client.Me);
            this.client.OnLoginComplete += ClientOnOnLoginComplete;
        }

        #endregion

        internal void StateUpdate(object o)
        {
            switch (this.State)
            {
                case ChatRoomState.GettingUsers:
                    if (!this.IsGroupChat && this.Users.Count > 0)
                    {
                        State = ChatRoomState.Connected;
                    }
                    break;
                case ChatRoomState.GettingHistory:
                    if (new TimeSpan(DateTime.Now.Ticks - lastRecieveTime.Value.Ticks).TotalSeconds >= 2)
                    {
                        State = ChatRoomState.Connected;
                    }
                    break;
            }
        }

        #region Delegates

        /// <summary>
        /// The d message received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="msgId">Id of the message.</param>
        /// <param name="from">
        /// The from.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="rTime">
        /// The r time.
        /// </param>
        /// <param name="mType">
        /// The m type.
        /// </param>
        public delegate void DMessageReceived(
            object sender,
            string msgId,
            User from,
            string message,
            DateTime rTime,
            LobbyMessageType mType = LobbyMessageType.Standard);

        /// <summary>
        /// The d user list change.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        public delegate void DUserListChange(object sender, List<User> users);

        public delegate void DRoomStateChange(object sender, ChatRoomState oldState, ChatRoomState newState);

        public delegate void DRoomTypeChange(object sender, ChatRoomType roomType);

        #endregion

        #region Public Events

        /// <summary>
        /// The on message received.
        /// </summary>
        public event DMessageReceived OnMessageReceived;

        /// <summary>
        /// The on user list change.
        /// </summary>
        public event DUserListChange OnUserListChange;

        public event DRoomStateChange OnStateChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the admin list.
        /// </summary>
        public List<User> AdminList { get; set; }

        /// <summary>
        /// Gets the group user.
        /// </summary>
        public User GroupUser { get; private set; }

        /// <summary>
        /// Gets a value indicating whether is group chat.
        /// </summary>
        public bool IsGroupChat { get; private set; }

        /// <summary>
        /// Gets or sets the moderator list.
        /// </summary>
        public List<User> ModeratorList { get; set; }

        /// <summary>
        /// Gets or sets the owner list.
        /// </summary>
        public List<User> OwnerList { get; set; }

        /// <summary>
        /// Gets the rid.
        /// </summary>
        public long Rid { get; private set; }

        /// <summary>
        /// Gets or sets the users.
        /// </summary>
        public List<User> Users { get; set; }

        /// <summary>
        /// Gets or sets the voice list.
        /// </summary>
        public List<User> VoiceList { get; set; }

        /// <summary>
        /// Chat room state
        /// </summary>
        public ChatRoomState State
        {
            get
            {
                return state;
            }
            private set
            {
                if (state == value) return;
                var os = state;
                state = value;
                this.FireOnStateChanged(os,state);
            }
        }

        private ChatRoomState state;

        #endregion

        #region Operators

        /// <summary>
        /// The ==.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// True if rooms are equal, False if they are not.
        /// </returns>
        public static bool operator ==(ChatRoom a, ChatRoom b)
        {
            long rid1 = -1;
            long rid2 = -1;
            if ((object.Equals(a, null) && !object.Equals(b, null))
                || (object.Equals(b, null) && !object.Equals(a, null)))
            {
                return false;
            }

            if (!object.Equals(a, null))
            {
                rid1 = a.Rid;
            }

            if (!object.Equals(b, null))
            {
                rid2 = b.Rid;
            }

            if (rid1 == -1 && rid2 == -1)
            {
                return true;
            }

            return rid1 == rid2;
        }

        /// <summary>
        /// The !=.
        /// </summary>
        /// <param name="a">
        /// The a.
        /// </param>
        /// <param name="b">
        /// The b.
        /// </param>
        /// <returns>
        /// True if rooms are not equal, False otherwise.
        /// </returns>
        public static bool operator !=(ChatRoom a, ChatRoom b)
        {
            return !(a == b);
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Equals(ChatRoom other)
        {
            return other.Rid == this.Rid;
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="o">
        /// The o.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool Equals(object o)
        {
            return this.GetHashCode() == o.GetHashCode();
        }

        /// <summary>
        /// The equals.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public new bool Equals(object x, object y)
        {
            return x.GetHashCode() == y.GetHashCode();
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <param name="obj">
        /// The object.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }

        /// <summary>
        /// The get hash code.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return (int)this.Rid;
        }

        #endregion operators

        #region Public Methods

        public void Reconnect()
        {
            messageCount = 0;
            lastRecieveTime = DateTime.Now;
            State = ChatRoomState.Disconnected;
            var users = this.Users.ToList();
            this.Users = new List<User>();
            this.AdminList = new List<User>();
            this.OwnerList = new List<User>();
            this.ModeratorList = new List<User>();
            this.VoiceList = new List<User>();

            if (this.IsGroupChat)
            {
                this.client.MucManager.JoinRoom(this.GroupUser.JidUser, this.client.Me.UserName);
                this.client.MucManager.RequestModeratorList(this.GroupUser.JidUser);
                this.client.MucManager.RequestAdminList(this.GroupUser.JidUser);
                this.client.MucManager.RequestOwnerList(this.GroupUser.JidUser);
                this.client.MucManager.RequestVoiceList(this.GroupUser.JidUser);
                State = ChatRoomState.Connecting;
                this.AddUser(this.client.Me);
            }
            else
            {
                State = ChatRoomState.Connecting;
                foreach (var u in users)
                    this.AddUser(u);
            }
        }

        /// <summary>
        /// The add user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="inviteUser">
        /// The invite user.
        /// </param>
        public void AddUser(User user, bool inviteUser = true)
        {
            if (this.State == ChatRoomState.Connecting)
            {
                this.State = ChatRoomState.GettingUsers;
            }
            if (!this.Users.Contains(user))
            {
                this.Users.Add(user);
            }

            if (this.Users.Count > 2 || this.IsGroupChat)
            {
                if (!this.IsGroupChat)
                {
                    this.IsGroupChat = true;
                    string rname = Randomness.RandomRoomName();
                    this.GroupUser = new User(rname + "@conference." + this.client.Config.ChatHost);

                    this.client.MucManager.JoinRoom(this.GroupUser.JidUser, this.client.Me.UserName);
                    this.client.MucManager.AcceptDefaultConfiguration(this.GroupUser.JidUser);
                    //this.client.MucManager.JoinRoom(this.GroupUser.JidUser, this.client.Me.UserName);
                    this.client.RosterManager.AddRosterItem(this.GroupUser.JidUser, this.GroupUser.UserName);
                }

                if (inviteUser)
                {
                    foreach (var u in this.Users)
                    {
                        if (u != this.client.Me)
                        {
                            this.client.MucManager.Invite(u.JidUser, this.GroupUser.JidUser);
                        }
                    }
                }
            }
            this.FireUpdateList();
        }

        private void ClientOnOnLoginComplete(object sender, LoginResults results)
        {
            this.Reconnect();
        }

        /// <summary>
        /// The on message.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        public void OnMessage(object sender, Message msg)
        {
            bool gotMessage = false;
            var remoteTime = DateTime.Now;
            if (msg.XDelay != null)
            {
                remoteTime = msg.XDelay.Stamp.ToLocalTime();
            }

            switch (msg.Type)
            {
                case MessageType.normal:
                    break;
                case MessageType.error:
                    if (msg.Error != null && !string.IsNullOrWhiteSpace(msg.Error.ErrorText))
                    {
                        this.OnMessageReceived.Invoke(
                            this,msg.Id, new User(msg.From), msg.Error.ErrorText, DateTime.Now, LobbyMessageType.Error);
                    }

                    break;
                case MessageType.chat:
                    switch (msg.Chatstate)
                    {
                        case Chatstate.None:
                            if (!this.IsGroupChat && !string.IsNullOrWhiteSpace(msg.Body)
                                && this.OnMessageReceived != null && this.Users.Contains(new User(msg.From.Bare)))
                            {
                                gotMessage = true;
                                this.OnMessageReceived.Invoke(this, msg.Id,new User(msg.From.Bare), msg.Body, remoteTime);
                            }

                            break;
                        case Chatstate.active:

                            break;
                        case Chatstate.inactive:
                            break;
                        case Chatstate.composing:
                            break;
                        case Chatstate.gone:
                            break;
                        case Chatstate.paused:
                            break;
                    }

                    break;
                case MessageType.groupchat:
                    if (this.IsGroupChat && msg.Chatstate == Chatstate.None)
                    {
                        if (msg.From.Bare == this.GroupUser.FullUserName)
                        {
                            if (!string.IsNullOrWhiteSpace(msg.Subject))
                            {
                                if (this.OnMessageReceived != null)
                                {
                                    gotMessage = true;
                                    this.OnMessageReceived.Invoke(
                                        this,
                                        msg.Id,
                                        new User(new Jid(msg.From.Resource + "@" + this.client.Config.ChatHost)),
                                        msg.Subject,
                                        remoteTime,
                                        LobbyMessageType.Topic);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(msg.Body))
                            {
                                if (this.OnMessageReceived != null)
                                {
                                    gotMessage = true;
                                    this.OnMessageReceived.Invoke(
                                        this,
                                        msg.Id,
                                        new User(new Jid(msg.From.Resource + "@" + this.client.Config.ChatHost)),
                                        msg.Body,
                                        remoteTime);
                                }
                            }
                        }
                    }

                    break;
                case MessageType.headline:
                    break;
            }
            if (gotMessage)
            {
                if (this.State == ChatRoomState.GettingUsers)
                {
                    this.State = ChatRoomState.GettingHistory;
                }
                else if (this.State == ChatRoomState.GettingHistory)
                {
                    if (messageCount >= 5 )
                    {
                        this.State = ChatRoomState.Connected;
                    }
                }
                lastRecieveTime = DateTime.Now;
                messageCount++;
            }
        }

        public void Whisper(User user)
        {
            if (this.IsGroupChat)
            {
                if (user.UserName.Equals(this.GroupUser.UserName))
                {
                    this.client.Chatting.GetRoom(new User(new Jid(user.JidUser.Resource, this.client.Config.ChatHost, "")));
                }
            }
            this.client.Chatting.GetRoom(user);
        }

        private static Dictionary<string, string> slashCommands;
        public static Dictionary<string, string> SlashCommands
        {
            get
            {
                if (slashCommands == null)
                {
                    slashCommands = new Dictionary<string, string>();
                    slashCommands.Add("?","Get Help");
                    slashCommands.Add("topic", "Set the room topic(if you have the cred)" + Environment.NewLine + "Usage: /topic This is a really great room");
                    slashCommands.Add("msg", "Starts a chat with a specific user" + Environment.NewLine + "Usage: /msg username");
                    slashCommands.Add("friend", "Add a friend, or multiple friends(separated by commas)" + Environment.NewLine + "Usage: /friend username[,username2,username3,...]");
                    slashCommands.Add("removefriend", "Remove a friend, or multiple friends(separated by commas)" + Environment.NewLine + "Usage: /removefriend username[,username2,username3,...]");
                    slashCommands.Add("invite", "Invite user, or multiple users, to a chat room(separated by commas)" + Environment.NewLine + "Usage: /invite username[,username2,username3,...]");
                }
                return slashCommands;
            }
        }

        /// <summary>
        /// The send message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void SendMessage(string message)
        {
            var to = this.IsGroupChat
                             ? this.GroupUser
                             : this.Users.FirstOrDefault(x => x.FullUserName != this.client.Me.FullUserName);
            if (to == null || string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            if (message[0] == '/')
            {
                var userCommand = message.Substring(1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var args = new string[0];
                if (userCommand.Length > 1)
                {
                    args = String.Join(" ", userCommand.Skip(1)).Split(new[] { ',' });
                    for (var i = 0; i < args.Length; i++) args[i] = args[i].Trim();
                }
                //var args = userCommand.Skip(1).ToArray();
                var command = userCommand.Length > 0 ? userCommand[0] : string.Empty;

                #region SlashCommands
                switch (command)
                {
                    case "?":
                        {
                            var helpMessage = new StringBuilder();
                            helpMessage.AppendLine("--Commands--");
                            helpMessage.AppendLine("/?                : This help");
                            helpMessage.AppendLine(
                                "/topic            : Set the topic of the room if you have the proper credentials");
                            helpMessage.AppendLine("/msg {username} : Starts a chat with a specific user");
                            helpMessage.AppendLine("/friend {username}: Add a friend, or multiple friends(separated by commas)");
                            helpMessage.AppendLine("/removefriend {username}: Remove a friend, or multiple friends(separated by commas)");
                            helpMessage.AppendLine("/invite {username}: Invite user, or multiple users, to a chat room(separated by commas)");
                            Message mess = null;
                            if (IsGroupChat)
                            {
                                foreach (var l in helpMessage.ToString()
                                                   .Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                                {
                                    mess = new Message(this.client.Me.JidUser, MessageType.groupchat, l);
                                    mess.From = this.GroupUser.JidUser;
                                    mess.Chatstate = Chatstate.None;
                                    this.OnMessage(this, mess);
                                }
                            }
                            else
                            {
                                foreach (var l in helpMessage.ToString()
                                                   .Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                                {
                                    mess = new Message(this.client.Me.JidUser, MessageType.chat, l);
                                    mess.From = "SYSTEM" + "@" + this.client.Config.ChatHost;
                                    mess.Chatstate = Chatstate.None;
                                    this.OnMessage(this, mess);
                                }
                            }
                            break;
                        }
                    case "topic":
                        {
                            this.SetTopic(String.Join(" ", args));
                            break;
                        }

                    case "msg":
                        {
                            ChatRoom r =
                                this.client.Chatting.GetRoom(
                                    new User(new Jid(args[0] ?? "", this.client.Config.ChatHost, "")));
                            break;
                        }
                    case "friend":
                        {
                            foreach (var a in args)
                                this.client.SendFriendRequest(a);
                            break;
                        }
                    case "removefriend":
                        {
                            foreach (var a in args)
                                this.client.RemoveFriend(new User(new Jid(a, this.client.Config.ChatHost, "")));
                            break;
                        }
                    case "invite":
                        {
                            foreach (var a in args)
                                this.AddUser(new User(new Jid(a, this.client.Config.ChatHost, "")));
                            break;
                        }
                    default:
                        {
                            var helpMessage = new StringBuilder();
                            helpMessage.AppendLine("--Commands--");
                            helpMessage.AppendLine("/?                : This help");
                            helpMessage.AppendLine(
                                "/topic            : Set the topic of the room if you have the proper credentials");
                            helpMessage.AppendLine("/msg {username}   : Starts a chat with a specific user");
                            helpMessage.AppendLine("/friend {username}: Add a friend, or multiple friends");
                            helpMessage.AppendLine("/removefriend {username}: Remove a friend, or multiple friends");
                            helpMessage.AppendLine("/invite {username}: Invite user, or multiple users, to a chat room");
                            Message mess = null;
                            if (IsGroupChat)
                            {
                                foreach (var l in helpMessage.ToString()
                                                   .Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                                {
                                    mess = new Message(this.client.Me.JidUser, MessageType.groupchat, l);
                                    mess.From = this.GroupUser.JidUser;
                                    mess.Chatstate = Chatstate.None;
                                    this.OnMessage(this, mess);
                                }
                            }
                            else
                            {
                                foreach (var l in helpMessage.ToString()
                                                   .Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                                {
                                    mess = new Message(this.client.Me.JidUser, MessageType.chat, l);
                                    mess.From = "SYSTEM" + "@" + this.client.Config.ChatHost;
                                    mess.Chatstate = Chatstate.None;
                                    this.OnMessage(this, mess);
                                }
                            }
                            break;
                        }
                }
                #endregion SlashCommands
            }
            else
            {
                var j = new Jid(to.JidUser);
                var m = new Message(j, this.IsGroupChat ? MessageType.groupchat : MessageType.chat, message);
                m.GenerateId();
                m.XEvent = new Event { Delivered = true, Displayed = true };
                this.client.Send(m);
                if (!this.IsGroupChat)
                {
                    m.From = this.client.Me.JidUser;
                    this.OnMessage(this, m);
                }
            }
        }

        /// <summary>
        /// The set topic.
        /// </summary>
        /// <param name="topic">
        /// The topic.
        /// </param>
        public void SetTopic(string topic)
        {
            if (!this.IsGroupChat || this.GroupUser == null)
            {
                return;
            }

            var m = new Message(this.GroupUser.FullUserName, MessageType.groupchat, string.Empty, topic);
            m.GenerateId();
            m.XEvent = new Event { Delivered = true, Displayed = true };
            this.client.Send(m);
        }

        /// <summary>
        /// The user left.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public void UserLeft(User user)
        {
            this.Users.Remove(user);
            this.FireUpdateList();
        }

        /// <summary>
        /// The fire update list.
        /// </summary>
        private void FireUpdateList()
        {
            if (this.OnUserListChange != null)
            {
                this.OnUserListChange.Invoke(this, this.Users);
            }
        }

        protected virtual void FireOnStateChanged(ChatRoomState oldstate, ChatRoomState newstate)
        {
            var handler = this.OnStateChanged;
            if (handler != null)
            {
                handler(this, oldstate, newstate);
            }
        }

        #endregion

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.AdminList.Clear();
            this.ModeratorList.Clear();
            this.OwnerList.Clear();
            this.Users.Clear();
            this.VoiceList.Clear();
            this.GroupUser = null;
            if (OnMessageReceived != null)
            {
                foreach (var d in OnMessageReceived.GetInvocationList())
                {
                    OnMessageReceived -= (DMessageReceived)d;
                }
            }
            if (OnUserListChange != null)
            {
                foreach (var d in OnUserListChange.GetInvocationList())
                {
                    OnUserListChange -= (DUserListChange)d;
                }
            }
            if (OnStateChanged != null)
            {
                foreach (var d in OnStateChanged.GetInvocationList())
                {
                    OnStateChanged -= (DRoomStateChange)d;
                }
            }
            stateTimer.Dispose();
        }
    }

    public enum ChatRoomState
    {
        Disconnected,
        Connecting,
        GettingUsers,
        GettingHistory,
        Connected
    }

    public enum ChatRoomType
    {
        Whisper,
        GroupChat
    }
}