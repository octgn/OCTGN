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
        private readonly Client client;

        /// <summary>
        /// XMPP Connection
        /// </summary>
        private readonly XmppClientConnection xmpp;

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
            this.Rooms = new ThreadSafeList<NewChatRoom>();
            this.xmpp = xmpp;
            this.xmpp.OnMessage += this.XmppOnMessage;
            this.xmpp.OnPresence += this.XmppOnPresence;
        }

        #endregion

        #region Delegates

        /// <summary>
        /// Delegate for room creation events
        /// </summary>
        /// <param name="sender">The Sender</param>
        /// <param name="room">Created Room</param>
        public delegate void DCreateChatRoom(object sender, NewChatRoom room);

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
        public ThreadSafeList<NewChatRoom> Rooms { get; set; }

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
        /// The <see cref="NewChatRoom"/>.
        /// </returns>
        public NewChatRoom GetRoom(NewUser otherUser, bool group = false)
        {
            if (group)
            {
                NewChatRoom ret = this.Rooms.FirstOrDefault(x => x.IsGroupChat && x.GroupUser.Equals(otherUser));
                if (ret == null)
                {
                    ret = new NewChatRoom(this.NextRid, this.client, otherUser);
                    this.Rooms.Add(ret);
                    this.FireOnCreateRoom(this, ret);
                }

                return ret;
            }
            else
            {
                NewChatRoom ret = this.Rooms.FirstOrDefault(x => x.Users.Contains(otherUser) && !x.IsGroupChat);
                if (ret == null)
                {
                    ret = new NewChatRoom(this.NextRid, this.client, otherUser);
                    this.Rooms.Add(ret);
                    this.FireOnCreateRoom(this, ret);
                }

                return ret;
            }
        }

        /// <summary>
        /// Removes a room from the room list only. Doesn't do anything else.
        /// </summary>
        /// <param name="room">
        /// Room to remove
        /// </param>
        public void RemoveRoom(NewChatRoom room)
        {
            // TODO This piece should be replaced with an event that lives inside the chat room object.
            this.Rooms.Remove(room);
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
                    if (msg.From.Server == "conference." + this.client.Host)
                    {
                        string rname = msg.From.User;
                        var m = new MucManager(this.xmpp);
                        m.JoinRoom(msg.From, msg.From.User);
                        NewChatRoom theRoom = this.GetRoom(new NewUser(msg.From), true);
                        this.xmpp.RosterManager.AddRosterItem(msg.From, msg.From.User);
                        this.FireOnCreateRoom(this, theRoom);
                    }

                    break;
                case MessageType.error:
                    {
                        NewChatRoom nc = this.GetRoom(new NewUser(msg.From.Bare), true);
                        nc.OnMessage(this, msg);
                        break;
                    }

                case MessageType.chat:
                    {
                        switch (msg.Chatstate)
                        {
                            case Chatstate.None:

                                // TODO Group chat whispers in the form of {roomname}@conference.server.octgn.info/{username} need to be handled here.
                                NewChatRoom nc = this.GetRoom(new NewUser(msg.From.Bare));
                                nc.OnMessage(sender, msg);
                                break;
                        }

                        break;
                    }

                case MessageType.groupchat:
                    {
                        NewChatRoom nc = this.GetRoom(new NewUser(msg.From.Bare), true);
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
                    if (pres.From.Server == "conference." + this.client.Host)
                    {
                        var addUser = new NewUser(pres.MucUser.Item.Jid);
                        var rm = this.GetRoom(new NewUser(pres.From), true);
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

                        rm.AddUser(new NewUser(pres.MucUser.Item.Jid), false);
                    }

                    break;
                case PresenceType.unavailable:
                    {
                        if (pres.From.Server == "conference." + this.client.Host)
                        {
                            if (pres.MucUser.Item.Jid == null)
                            {
                                break;
                            }

                            if (pres.MucUser.Item.Jid.Bare == this.client.Me.FullUserName)
                            {
                                break;
                            }

                            NewChatRoom rm = this.GetRoom(new NewUser(pres.From), true);
                            rm.UserLeft(new NewUser(pres.MucUser.Item.Jid));
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
        private void FireOnCreateRoom(object sender, NewChatRoom room)
        {
            if (this.OnCreateRoom != null)
            {
                this.OnCreateRoom(sender, room);
            }
        }

        #endregion
    }
}