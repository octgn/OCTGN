using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Octgn.Data;
using Octgn.Data.Models;

namespace Octgn.App.Areas.api.Controllers
{
    public class UserController : Controller
    {
        public ActionResult AllUsers()
        {
			using (var c = Database.GetClient())
			{
				var rawusers = c.Query<IUser>();
				var users = rawusers.Select(u => new SafeUser(u)).ToList();

				return Json(users, JsonRequestBehavior.AllowGet);
			}
        }
    	[AcceptVerbs("get","post")]
		public ActionResult Where(string username = null,string displayname = null, UserStatus? status = null)
		{
			using(var c = Database.GetClient())
			{
				IEnumerable<IUser> users = c.Query<IUser>();
				if(username != null)
				{
					users = users.Where(x => x.Username.Contains(username));
				}
				if(displayname != null)
				{
					users = users.Where(x => x.DisplayName.Contains(displayname));
				}
				if(status != null)
				{
					users = users.Where(x => x.Status.Equals(status));
				}
				var ret = users.Select(u => new SafeUser(u)).ToList();
				return Json(ret , JsonRequestBehavior.AllowGet);

			}
		}

    }
}
