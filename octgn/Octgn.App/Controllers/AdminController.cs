using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Octgn.App.Controllers
{
    public class AdminController : Controller
    {
		[Authorize]
        public ActionResult Index()
        {
			if(Roles.IsUserInRole("administrator"))
			{
				return View("Index");
			}
            return View("NotAuthorized");
        }

    }
}
