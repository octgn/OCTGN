// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NewUser.cs" company="OCTGN">
//   GNU Stuff
// </copyright>
// <summary>
//   Defines the UserStatus type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Octgn.Library.Communication;
using Octgn.Site.Api.Models;
using System;
using System.Collections;

namespace Octgn.Library
{
    public class User : IEquatable<User>, IEqualityComparer
    {
        public User(string username)
        {
            this.UserName = username;
            this.Status = UserStatus.Unknown;
            this.CustomStatus = string.Empty;
            this.Email = string.Empty;
            if (string.IsNullOrWhiteSpace(this.UserName)) return;
        }

        public User(User user)
        {
            this.UserName = user.UserName;
            this.Status = user.Status;
            this.CustomStatus = user.CustomStatus.Clone() as string;
            this.Email = user.Email.Clone() as string;
        }
        public string UserName { get; set; }
        public UserStatus Status { get; set; }
        public string CustomStatus { get; set; }
        public string Email { get; set; }
        public bool IsSubbed {
            get {
                var au = UserManager.Get().ApiUser(this);
                if (au == null) return false;
                return au.IsSubscribed;
            }
        }
        public ApiUser ApiUser {
            get {
                var au = UserManager.Get().ApiUser(this);
                return au;
            }
        }

        public static bool operator ==(User a, User b)
        {
            if (null == a as object || null == b as object) return false;
            return a.UserName.Equals(b.UserName, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator !=(User a, User b) => !(a == b);

        public bool Equals(User other) => this == other;

        public override string ToString() => this.UserName;

        public override int GetHashCode() => this.UserName.GetHashCode();

        public new bool Equals(object x, object y) => x == y;

        public override bool Equals(object obj)
        {
            if (obj is User) return (obj as User).Equals(this);
            return false;
        }

        public int GetHashCode(object obj) => obj?.GetHashCode() ?? 0;
    }
}
