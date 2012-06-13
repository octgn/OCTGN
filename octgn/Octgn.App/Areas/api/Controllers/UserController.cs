using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Octgn.Data;
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
				//var users = rawusers.Select(u => new SafeUser(u)).ToList();

				return Json(rawusers, JsonRequestBehavior.AllowGet);
			}
        }
    	[AcceptVerbs("get","post")]
		public ActionResult Where(string username = null)
		{
			using(var c = Database.GetClient())
			{
				IEnumerable<User> users = c.Query<User>();
				if(username != null)
				{
					users = users.Where(x => x.Username.Contains(username));
				}
				return Json(users, JsonRequestBehavior.AllowGet);

			}
		}

    }
}
