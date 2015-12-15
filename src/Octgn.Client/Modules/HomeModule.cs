using Nancy;

namespace Octgn.Client.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(UIBackend uiBackend)
        {
            Get["/"] = x =>
            {
                ViewBag.SignalRPort = uiBackend.SignalRPort;

                return View["Index"];
            };
        }
    }
}