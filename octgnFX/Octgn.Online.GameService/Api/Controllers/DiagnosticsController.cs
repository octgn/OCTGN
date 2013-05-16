namespace Octgn.Online.GameService.Api.Controllers
{
    using System.Web.Http;

    public class DiagnosticsController : ApiController
    {
        [HttpGet,HttpPost]
        public bool HealthCheck()
        {
            // TODO Probably want to include other health checks for other connected services
            return true;
        }
    }
}
