using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Octgn.Online.GameService.Api.Controllers
{
    using System.Reflection;

    using Octgn.Online.GameService.Hubs;
    using Octgn.Online.Library.Models;

    using log4net;

    public class GameController : ApiController
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [HttpGet,HttpPost]
        public IEnumerable<HostedGame> List()
        {
            return new HostedGame[0];
        }

        [HttpPost]
        public Guid? HostGame(HostedGameRequest request)
        {
            try
            {
                var v = Version.Parse(request.GameVersion);
                var id = SasManagerHub.NextConnectionId;
                var hg = request.ToHostedGameSasRequest();
                SasManagerHub.Out.Client(id).StartGame(hg).Wait();
                return hg.Id;
            }
            catch (Exception e)
            {
                Log.Fatal("HostGame error",e);
                return null;
            }
        }
    }
}