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
            ChannelFactory<IBaseInterface> pipeFactory =
        new ChannelFactory<IBaseInterface>(
          new NetNamedPipeBinding(),
          new EndpointAddress(
            "net.pipe://localhost/PipeBase"));

            IBaseInterface pipeProxy =
              pipeFactory.CreateChannel();

            ViewData["Message"] = pipeProxy.GetMessage();

			//ViewData["Message"] = Octgn.Program.tester;

			return View();
		}

		public ActionResult About()
		{
			return View();
		}
	}
}
