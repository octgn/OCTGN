using Nancy;

namespace Octgn.Client.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(Server server)
        {
            Get["/"] = x =>
            {
                ViewBag.SignalRPort = server.SignalRPort;

                return View["Index"];
            };
        }
    }
}