using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Db4objects.Db4o.Config.Attributes;
using Octgn.Common;

namespace Octgn.Data.Models
{
	public class User : IUser
	{
		public Guid ID { get; set; }
		public string Username { get; set; }
		public string DisplayName { get; set; }
		public UserStatus Status { get; set; }

		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public bool IsApproved { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime LastLoginDate { get; set; }
		public DateTime LastPasswordChangedDate { get; set; }
		public DateTime LastActivityDate { get; set; }
		public DateTime LastLockoutDate { get; set; }
		public DateTime LockoutExpiresDate { get; set; }
		public bool IsLockedOut { get; set; }
		public int FailedPasswordAttemptCount { get; set; }

		public User()
		{
			
		}

		public static User CreateUser(string username, string displayname,string email, string passHash)
		{
			return new User
				{
					ID = Guid.NewGuid(),
					Username = username,
					DisplayName = displayname,
					Status = UserStatus.Online,
					Email = email,
					PasswordHash = passHash,
					IsApproved = true,
					CreationDate = DateTime.Now,
					LastPasswordChangedDate = DateTime.Now,
					LastActivityDate = DateTime.Now,
					LastLockoutDate = DateTime.Now,
					LockoutExpiresDate = DateTime.Now.AddDays(-1),
					LastLoginDate = DateTime.Now,
					IsLockedOut = true,
					FailedPasswordAttemptCount = 0
				};
		}

		public static User CreateAdmin()
		{
			return CreateUser("admin" , "admin" , "na" , ValueConverters.CreateShaHash("password"));
		}

		public MembershipUser ToMembershipUser()
		{
			var mUser = new MembershipUser("OctgnMembershipProvider" , Username , ID , Email , "" , "" , IsApproved , IsLockedOut ,
			                               CreationDate , LastLoginDate , LastActivityDate , LastPasswordChangedDate ,
			                               LastLockoutDate);
			return mUser;
		}

		public static bool ValidateUser(string username, string password)
		{
			username = username.ToLower();
			using (var client = Database.GetClient())
			{
				var r = client.Query<User>().Where(u => u.Username == username);
				var us = r.FirstOrDefault();
				if(us == null) return false;
				if(us.PasswordHash == password)
				{
					//us.LastRequest = DateTime.Now;
					us.Status = UserStatus.Online;
					client.Store(us);
					return true;
				}
				return false;
			}
		}
	}
}
