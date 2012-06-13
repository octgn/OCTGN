using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Data.Models
{
	public enum UserStatus{Offline,Online,Idle,Busy}
	public enum UserGroup{User, Moderator, Admin}
	public interface IUser
	{
		string DisplayName { get; set; }
		string Username { get; set; }
		UserStatus Status { get; set; }
		List<UserGroup> Groups { get; set; }
	}
}
