using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
	public class SafeUser : IUser
	{
		public Guid ID { get; set; }

		public string DisplayName { get; set; }

		public string Username { get; set; }

		public UserStatus Status { get; set; }

		public SafeUser(IUser u)
		{
			DisplayName = u.DisplayName;
			Username = u.Username;
			Status = u.Status;
		}

	}
}
