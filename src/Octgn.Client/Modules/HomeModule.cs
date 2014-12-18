using Nancy;

namespace Octgn.Client.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = x => View["Index"];
        }
    }
}