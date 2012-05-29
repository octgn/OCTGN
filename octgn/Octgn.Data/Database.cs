using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using Db4objects.Db4o;
using User = Octgn.Data.Models.User;

namespace Octgn.Data
{
	public class Database : IDisposable
	{
		private IObjectContainer database { get; set; }
		public Database()
		{
			database = Db4oEmbedded.OpenFile(Db4oEmbedded.NewConfiguration(), "master.db");
		}

		public MembershipCreateStatus CreateUser(string username, string password, string email)
		{
			var r = database.Query<User>().Where(u => u.Username == username || u.Email == email);
			if(r.Any())
			{
				return r.AsParallel().Any(u => u.Email == email) ? MembershipCreateStatus.DuplicateEmail : MembershipCreateStatus.DuplicateUserName;
			}
			database.Store(new User
			{
				Email = email ,
				PasswordHash = password ,
				Username = username
			});
			return MembershipCreateStatus.Success;
		}

		public void Dispose()
		{
			database.Close();
		}
	}
}
