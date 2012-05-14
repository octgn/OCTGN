using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Octgn.App.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			ViewData["Message"] = Octgn.Program.tester;

			return View();
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
