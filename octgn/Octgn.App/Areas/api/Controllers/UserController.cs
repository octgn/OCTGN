using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Octgn.Common.Sockets;
using Octgn.Data;
using Octgn.Data.Models;
using WCSoft.db4oProviders;

namespace Octgn.App.Areas.api.Controllers
{
    public class UserController : Controller
    {
        public ActionResult AllUsers()
        {
			using (var c = Database.GetClient())
			{
				var rawusers = c.Query<User>();
				var users = rawusers.Select(r => new SafeUser(r)).ToList();
				//var users = rawusers.Select(u => new SafeUser(u)).ToList();

				return Json(users, JsonRequestBehavior.AllowGet);
			}
        }
    	[AcceptVerbs("get","post")]
		public ActionResult Where(string username = null, Guid ?id = null)
		{
			using(var c = Database.GetClient())
			{
				IEnumerable<User> rawusers = c.Query<User>();
				if(username != null)
				{
					rawusers = rawusers.Where(x => x.Username.Contains(username));
				}
				if(id != null)
				{
					rawusers = rawusers.Where(x => x.PKID.Equals(id));
				}
				var users = rawusers.Select(r => new SafeUser(r)).ToList();
				return Json(users, JsonRequestBehavior.AllowGet);

			}
		}
		[Authorize]
		public ActionResult AddFriend(string email)
		{
			Friend theFriend = null;
			using (var client = Database.GetClient())
			{
				var muser = client.Query<User>(x => x.Email.ToLower() == email).FirstOrDefault();
				if (muser != null)
				{
					theFriend = new Friend
					{
						Email = muser.Email,
						Username = muser.Username
					};
				}
			}
			if (theFriend != null) return JsonReturn.Success("Friend found in DB");
			var mess = new SocketMessage("addfriend");
			mess.AddData("email", email);
			PeerHandler.MessagePeers(mess);
			return JsonReturn.Success("Waiting to hear back from peers");
		}

    }
}
