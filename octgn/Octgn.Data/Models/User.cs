using System;
using System.Linq;
using System.Web.Security;

namespace Octgn.Data.Models
{
	public class User : IDisposable
	{
		public string Username { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }

		public Database Db { get; set; }

		public User():this(new Database())
		{
			
		}
		public User(Database db) { Db = db; }

		public MembershipCreateStatus CreateUser(string username, string password, string email)
		{
			username = username.ToLower();
			email = email.ToLower();
			var r = Db.DbConnection.Query<User>().Where(x => x.Username == username || x.Email == email);
			if(r.Any())
			{
				return r.AsParallel().Any(u => u.Email == email)
					    ? MembershipCreateStatus.DuplicateEmail
					    : MembershipCreateStatus.DuplicateUserName;
			}
			Db.DbConnection.Store(new User
			{
				Email = email ,
				PasswordHash = password ,
				Username = username
			});
			return MembershipCreateStatus.Success;
		}
		public bool ValidateUser(string username, string password)
		{
			username = username.ToLower();
			var r = Db.DbConnection.Query<User>().Where(u => u.Username == username);
			var us = r.FirstOrDefault();
			if(us == null) return false;
			if(us.PasswordHash == password) return true;
			return false;
		}

		public void Dispose()
		{
			Db.Dispose();
		}
	}
}
