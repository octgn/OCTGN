using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.ServiceModel;
using Octgn;

namespace Octgn.App.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{

            ViewData["Message"] = WcfSingleton.GetInstance().GetChannel().GetMessage();
            WcfSingleton.GetInstance().GetChannel().CreateRandomMessage();
			//ViewData["Message"] = Octgn.Program.tester;

			return View();
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
