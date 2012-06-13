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
		public string Username { get; set; }
		public string DisplayName { get; set; }
		public UserStatus Status { get; set; }
		public List<UserGroup> Groups { get; set; }

		public string Email { get; set; }
		public string PasswordHash { get; set; }
		public DateTime LastRequest { get; set; }

		public User()
		{
			
		}

		public static void CreateAdmin()
		{
			using (var client = Database.GetClient())
			{
				client.Store(new User
				{
					Email = "na",
					PasswordHash = ValueConverters.CreateShaHash("password"),
					Username = "admin",
					DisplayName = "admin",
					Status = UserStatus.Online,
					LastRequest = DateTime.Now,
					Groups = new List<UserGroup> { UserGroup.Admin,UserGroup.Moderator,UserGroup.User }
				});
			}
		}

		public MembershipCreateStatus CreateUser(string username, string password, string email)
		{
			username = username.ToLower();
			email = email.ToLower();
			using (var client = Database.GetClient())
			{
				var r = client.Query<User>().Where(x => x.Username == username || x.Email == email);
				if(r.Any())
				{
					return r.AsParallel().Any(u => u.Email == email)
					       	? MembershipCreateStatus.DuplicateEmail
					       	: MembershipCreateStatus.DuplicateUserName;
				}
				client.Store(new User
				{
					Email = email ,
					PasswordHash = password ,
					Username = username,
					DisplayName = username,
					Status=UserStatus.Online,
					LastRequest = DateTime.Now,
					Groups = new List<UserGroup>{UserGroup.User}
				});
				return MembershipCreateStatus.Success;
			}
		}
		public bool ValidateUser(string username, string password)
		{
			username = username.ToLower();
			using (var client = Database.GetClient())
			{
				var r = client.Query<User>().Where(u => u.Username == username);
				var us = r.FirstOrDefault();
				if(us == null) return false;
				if(us.PasswordHash == password)
				{
					us.LastRequest = DateTime.Now;
					us.Status = UserStatus.Online;
					client.Store(us);
					return true;
				}
				return false;
			}
		}
	}
}
