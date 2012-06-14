using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WCSoft.db4oProviders;

namespace Octgn.Data.Models
{
	public class SafeUser
	{
		public Guid ID { get; set; }

		public string DisplayName { get; set; }

		public string Username { get; set; }

		public SafeUser(User u)
		{
			ID = u.PKID;
			DisplayName = u.Username;
			Username = u.Username;
		}

	}
}
