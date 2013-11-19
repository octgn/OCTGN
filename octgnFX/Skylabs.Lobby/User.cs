// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewUser.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Defines the UserStatus type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Skylabs.Lobby
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Octgn.Site.Api.Models;

    using agsXMPP;
    using agsXMPP.protocol.client;

    /// <summary>
    /// A user model for lobby users.
    /// </summary>
    public class User : IEquatable<User>, IEqualityComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="user">
        /// The user JID.
        /// </param>
        public User(Jid user)
        {
            this.JidUser = Jid.UnescapeNode(user.Bare.Clone() as string);
            this.Status = UserStatus.Unknown;
            this.CustomStatus = string.Empty;
            this.Email = string.Empty;
            if (string.IsNullOrWhiteSpace(this.UserName)) return;
        }

        public User(User user)
        {
            this.JidUser = Jid.UnescapeNode(user.JidUser.Bare.Clone() as string);
            this.Status = user.Status;
            this.CustomStatus = user.CustomStatus.Clone() as string;
            this.Email = user.Email.Clone() as string;
        }

        /// <summary>
        /// Gets or sets the raw JID user.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public Jid JidUser { get; private set; }

        /// <summary>
        /// Gets or sets the users User Name.
        /// </summary>
        public string UserName
        {
            get
            {
                return Jid.UnescapeNode(this.JidUser.User);
            }
            set
            {
                this.JidUser.User = value;
            }
        }

        /// <summary>
        /// Gets the full user name. This is UserName@Server
        /// </summary>
        public string FullUserName
        {
            get
            {
                return Jid.UnescapeNode(this.JidUser.Bare.ToLowerInvariant());
            }
        }

        /// <summary>
        /// Gets the XMPP server.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public string Server
        {
            get
            {
                return this.JidUser.Server;
            }
        }

        /// <summary>
        /// Gets the users status.
        /// </summary>
        public UserStatus Status { get; private set; }

        /// <summary>
        /// Gets or sets the custom status.
        /// </summary>
        public string CustomStatus { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or Sets if the user is subbed
        /// </summary>
        public bool IsSubbed {
            get
            {
                var au = UserManager.Get().ApiUser(this);
                if (au == null) return false;
                return au.IsSubscribed;
            }
        }

        public ApiUser ApiUser
        {
            get
            {
                var au = UserManager.Get().ApiUser(this);
                return au;
            }
        }

        /// <summary>
        /// Convert a <see cref="Presence"/> packet into a <see cref="UserStatus"/>
        /// </summary>
        /// <param name="p">
        /// The presence packet.
        /// </param>
        /// <returns>
        /// The <see cref="UserStatus"/>.
        /// </returns>
        public static UserStatus PresenceToStatus(Presence p)
        {
            if (p.From == null)
            {
                return UserStatus.Unknown;
            }

            Trace.WriteLine("[p2s]From: " + p.From);
            Trace.WriteLine("[p2s]Type: " + p.Type.ToString());
            Trace.WriteLine("[p2s]Show: " + p.Show.ToString());
            var status = UserStatus.Unknown;
            if (p.Type == PresenceType.unavailable)
            {
                status = UserStatus.Offline;
            }
            else
            {
                switch (p.Show)
                {
                    case ShowType.NONE:
                        break;
                    case ShowType.away:
                        status = UserStatus.Away;
                        break;
                    case ShowType.chat:
                        status = UserStatus.Online;
                        break;
                    case ShowType.dnd:
                        status = UserStatus.DoNotDisturb;
                        break;
                    case ShowType.xa:
                        status = UserStatus.DoNotDisturb;
                        break;
                }
            }

            Trace.WriteLine("[p2s]Result: " + status.ToString());
            return status;
        }

        /// <summary>
        /// Determines if this user is == to another user by comparing FullUserName
        /// </summary>
        /// <param name="a">
        /// The first user
        /// </param>
        /// <param name="b">
        /// The second user
        /// </param>
        /// <returns>
        /// True on equal, else False.
        /// </returns>
        public static bool operator ==(User a, User b)
        {
            string rid1 = null;
            string rid2 = null;

            if ((null == a as object && null != b as object) || (null != a as object && null == b as object)) 
                return false;

            // null must be on the left side of a, or we get a stack overflow
            if (null != a as object && a.JidUser != null && a.JidUser.Bare != null)
            {
                rid1 = a.JidUser.Bare;
            }

            // null must be on the left side of b, or we get a stack overflow.
            if (null != b as object && b.JidUser != null && b.JidUser.Bare != null)
            {
                rid2 = b.JidUser.Bare;
            }

            if (rid1 == null && rid2 == null)
            {
                return true;
            }

            return rid1 == rid2;
        }

        /// <summary>
        /// Determines if this user is != to another user by comparing FullUserName
        /// </summary>
        /// <param name="a">
        /// The first user
        /// </param>
        /// <param name="b">
        /// The second user
        /// </param>
        /// <returns>
        /// True on equal, else False.
        /// </returns>
        public static bool operator !=(User a, User b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Set the users status from a presence packet.
        /// </summary>
        /// <param name="p">
        /// The presence packet.
        /// </param>
        public void SetStatus(Presence p)
        {
            Trace.WriteLine("[SetStatus]For: " + p.From);
            var s = PresenceToStatus(p);
            if (s != UserStatus.Unknown)
            {
                this.Status = s;
            }
            else if (this.Status == UserStatus.Unknown)
            {
                this.Status = UserStatus.Online;
            }

            Trace.WriteLine("[SetStatus]Status: " + this.Status.ToString());
        }

        /// <summary>
        /// Set the user status.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        public void SetStatus(UserStatus status)
        {
            this.Status = status;
        }

        /// <summary>
        /// Determines if this User is equal to another user by comparing FullUserName
        /// </summary>
        /// <param name="other">
        /// The other user.
        /// </param>
        /// <returns>
        /// <see cref="bool"/>. True on equal, otherwise False.
        /// </returns>
        public bool Equals(User other)
        {
            return other.FullUserName.ToLowerInvariant() == this.FullUserName.ToLowerInvariant();
        }

        /// <summary>
        /// Converts this user to a string. Equal to calling this.UserName.
        /// </summary>
        /// <returns>
        /// The UserName
        /// </returns>
        public override string ToString()
        {
            return this.UserName;
        }

        /// <summary>
        /// Is this object equal to another object?
        /// </summary>
        /// <param name="x">
        /// The first object.
        /// </param>
        /// <param name="y">
        /// The second object.
        /// </param>
        /// <returns>
        /// True if equal, else false.
        /// </returns>
        public new bool Equals(object x, object y)
        {
            return x == y;
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
            return this.JidUser.GetHashCode();
        }

        /// <summary>
        /// Is this user equal to another object? Compares GetHashCode().
        /// </summary>
        /// <param name="obj">
        /// The other object.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>. True on equal, else false.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == this.GetHashCode();
        }
    }
}
