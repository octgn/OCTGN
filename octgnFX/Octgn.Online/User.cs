/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.Site.Api.Models;
using System;
using System.Collections;

namespace Octgn.Online
{
    public class User : IEquatable<User>, IEqualityComparer
    {
        public User(string userId, bool areUsingUserIdInsteadOfUsernameForTheOtherProperty)
        {
            if (!areUsingUserIdInsteadOfUsernameForTheOtherProperty) throw new InvalidOperationException("Must use userId instead of username");

            this.UserId = userId;
            this.UserName = $"USER#{userId}";
            this.Status = UserStatus.Unknown;
            this.CustomStatus = string.Empty;
            this.Email = string.Empty;
            if (string.IsNullOrWhiteSpace(this.UserName)) return;
        }

        public User(User user)
        {
            this.UserId = user.UserId.Clone() as string;
            this.UserName = user.UserName.Clone() as string;
            this.Status = user.Status;
            this.CustomStatus = user.CustomStatus.Clone() as string;
            this.Email = user.Email.Clone() as string;
        }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public UserStatus Status { get; set; }
        public string CustomStatus { get; set; }
        public string Email { get; set; }
        public bool IsSubbed {
            get {
                var au = ApiUserCache.Instance.ApiUser(this);
                if (au == null) return false;
                return au.IsSubscribed;
            }
        }
        public ApiUser ApiUser {
            get {
                var au = ApiUserCache.Instance.ApiUser(this);
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
