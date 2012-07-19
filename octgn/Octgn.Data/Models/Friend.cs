using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCSoft.db4oProviders;

namespace Octgn.Data.Models
{
	public class Friend
	{
		public string Username { get; set; }
		public string Email { get; set; }

		public static Friend GetFromDatabase(string email)
		{
			using(var client = Database.GetClient())
			{
				var muser = client.Query<User>(x => x.Email.ToLower() == email).FirstOrDefault();
				if (muser == null) return null;
				var f = new Friend
				{
					Email = muser.Email ,
					Username = muser.Username
				};
				return f;
			}
			return null;
		}
	}
}
