using System.Web.Mvc;

namespace Octgn.App.Controllers
{
	[HandleError]
	public class HomeController : Controller
	{
		public ActionResult Index()
		{

            //ViewData["Message"] = WcfSingleton.GetInstance().GetChannel().GetMessage();
            //WcfSingleton.GetInstance().GetChannel().CreateRandomMessage();
			//ViewData["Message"] = Octgn.Program.tester;

			return View();
		}

		public ActionResult About()
		{
			return View();
		}
		
	}
}
