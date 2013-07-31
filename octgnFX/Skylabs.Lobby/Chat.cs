// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Chat.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Defines the Chat type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Skylabs.Lobby
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using agsXMPP;
    using agsXMPP.protocol.client;
    using agsXMPP.protocol.extensions.chatstates;
    using agsXMPP.protocol.x.muc;

    /// <summary>
    /// Handles chatting for the Lobby Client
    /// </summary>
    public class Chat
    {
        #region Fields

        /// <summary>
        /// Lobby Client
        /// </summary>
        private Client client;

        /// <summary>
        /// XMPP Connection
        /// </summary>
        private XmppClientConnection xmpp;

        /// <summary>
        /// Last assigned Room ID
        /// </summary>
        private long lastRid;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Chat"/> class.
        /// </summary>
        /// <param name="c">
        /// The lobby client.
        /// </param>
        /// <param name="xmpp">
        /// The XMPP connection.
        /// </param>
        public Chat(Client c, XmppClientConnection xmpp)
        {
            this.client = c;
            this.Rooms = new ThreadSafeList<ChatRoom>();
            this.xmpp = xmpp;
            this.xmpp.OnMessage += this.XmppOnMessage;
            this.xmpp.OnPresence += this.XmppOnPresence;
        }

        #endregion

        public void Reconnect(Client c, XmppClientConnection xmpp)
        {
            this.client = c;
            if (this.xmpp != null)
            {
                this.xmpp.OnMessage -= this.XmppOnMessage;
                this.xmpp.OnPresence -= this.XmppOnPresence;
            }
            this.xmpp = xmpp;
            this.xmpp.OnMessage += this.XmppOnMessage;
            this.xmpp.OnPresence += this.XmppOnPresence;
        }

        #region Delegates

        /// <summary>
        /// Delegate for room creation events
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="room">Created Room</param>
        public delegate void DCreateChatRoom(object sender, ChatRoom room);

        #endregion

        #region Public Events

        /// <summary>
        /// Fires when a room is created.
        /// </summary>
        public event DCreateChatRoom OnCreateRoom;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the next Room ID for assignment.
        /// </summary>
        public long NextRid
        {
            get
            {
                this.lastRid = this.lastRid + 1 == long.MaxValue ? 0 : this.lastRid + 1;
                return this.lastRid;
            }
        }

        /// <summary>
        /// Gets or sets the chat rooms.
        /// </summary>
        private ThreadSafeList<ChatRoom> Rooms { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Get an existing user, or if it doesn't exist, create it.
        /// </summary>
        /// <param name="otherUser">
        /// The other user(or group user if a group chat)
        /// </param>
        /// <param name="group">
        /// Is it a group chat?
        /// </param>
        /// <returns>
        /// The <see cref="ChatRoom"/>.
        /// </returns>
        public ChatRoom GetRoom(User otherUser, bool group = false)
        {
            lock (this.Rooms)
            {
                if (group)
                {
                    ChatRoom ret = this.Rooms.FirstOrDefault(x => x.IsGroupChat && x.GroupUser.Equals(otherUser));
                    if (ret == null)
                    {
                        ret = new ChatRoom(this.NextRid, this.client, otherUser);
                        this.Rooms.Add(ret);
                    }

                    if (ret != null) this.FireOnCreateRoom(this, ret);
                    return ret;
                }
                else
                {
                    ChatRoom ret = this.Rooms.FirstOrDefault(x => x.Users.Contains(otherUser) && !x.IsGroupChat);
                    if (ret == null)
                    {
                        ret = new ChatRoom(this.NextRid, this.client, otherUser);
                        this.Rooms.Add(ret);
                    }
                    if (ret != null) this.FireOnCreateRoom(this, ret);
                    return ret;
                }
            }
        }

        public void LeaveRoom(ChatRoom room)
        {
            if (room.IsGroupChat && room.GroupUser.JidUser == "lobby") return;
            lock (Rooms)
            {
                if (room.IsGroupChat)
                {
                    this.client.MucManager.LeaveRoom(room.GroupUser.UserName, this.client.Me.UserName);
                    this.client.RosterManager.RemoveRosterItem(room.GroupUser.FullUserName);
                }
                this.Rooms.Remove(room);
                room.Dispose();
            }
        }

        public bool HasGroupRoom(User groupUser)
        {
            lock (Rooms)
            {
                return Rooms.Any(x => x.GroupUser == groupUser);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Fires when the XMPP Client receives a message.
        /// </summary>
        /// <param name="sender">
        /// The Sender
        /// </param>
        /// <param name="msg">
        /// The Message
        /// </param>
        private void XmppOnMessage(object sender, Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.normal:
                    if (msg.From.Server == "conference." + this.client.Config.ChatHost)
                    {
                        string rname = msg.From.User;
                        var m = new MucManager(this.xmpp);
                        m.JoinRoom(msg.From, msg.From.User);
                        ChatRoom theRoom = this.GetRoom(new User(msg.From), true);
                        this.xmpp.RosterManager.AddRosterItem(msg.From, msg.From.User);
                    }

                    break;
                case MessageType.error:
                    {
                        if (msg.From.User.ToLowerInvariant().Contains("gameserv")) return;
                        ChatRoom nc = this.GetRoom(new User(msg.From.Bare), true);
                        nc.OnMessage(this, msg);
                        break;
                    }

                case MessageType.chat:
                    {
                        switch (msg.Chatstate)
                        {
                            case Chatstate.None:

                                // TODO Group chat whispers in the form of {roomname}@conference.server.octgn.info/{username} need to be handled here.
                                ChatRoom nc = this.GetRoom(new User(msg.From.Bare));
                                nc.OnMessage(sender, msg);
                                break;
                        }

                        break;
                    }

                case MessageType.groupchat:
                    {
                        ChatRoom nc = this.GetRoom(new User(msg.From.Bare), true);
                        nc.OnMessage(this, msg);
                        break;
                    }

                case MessageType.headline:
                    break;
            }
        }

        /// <summary>
        /// XMPP on presence.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="pres">
        /// The presence
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private void XmppOnPresence(object sender, Presence pres)
        {
            switch (pres.Type)
            {
                case PresenceType.available:
                    if (pres.From.Server == "conference." + this.client.Config.ChatHost)
                    {
                        var addUser = new User(pres.MucUser.Item.Jid);
                        var rm = this.GetRoom(new User(pres.From), true);
                        switch (pres.MucUser.Item.Affiliation)
                        {
                            case Affiliation.none:
                                break;
                            case Affiliation.owner:
                                rm.OwnerList.Add(addUser);
                                break;
                            case Affiliation.admin:
                                rm.AdminList.Add(addUser);
                                break;
                            case Affiliation.member:
                                break;
                            case Affiliation.outcast:
                                break;
                        }

                        switch (pres.MucUser.Item.Role)
                        {
                            case Role.none:
                                break;
                            case Role.moderator:
                                rm.ModeratorList.Add(addUser);
                                break;
                            case Role.participant:
                                break;
                            case Role.visitor:
                                break;
                        }

                        rm.AddUser(new User(pres.MucUser.Item.Jid), false);
                    }

                    break;
                case PresenceType.unavailable:
                    {
                        if (pres.From.Server == "conference." + this.client.Config.ChatHost)
                        {
                            if (pres.MucUser.Item.Jid == null)
                            {
                                break;
                            }

                            if (pres.MucUser.Item.Jid.Bare == this.client.Me.FullUserName)
                            {
                                break;
                            }

                            ChatRoom rm = this.GetRoom(new User(pres.From), true);
                            rm.UserLeft(new User(pres.MucUser.Item.Jid));
                        }

                        break;
                    }
            }
        }

        /// <summary>
        /// Fire OnCreateRoom event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="room">
        /// The room.
        /// </param>
        private void FireOnCreateRoom(object sender, ChatRoom room)
        {
            if (this.OnCreateRoom != null)
            {
                this.OnCreateRoom(sender, room);
            }
        }

        #endregion
    }
}