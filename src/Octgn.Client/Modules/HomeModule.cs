using Nancy;

namespace Octgn.Client.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(UIBackend uiBackend)
        {
            Get["/"] = x =>
            {
                return View["Index"];
            };
        }
    }
}