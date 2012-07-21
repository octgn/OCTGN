using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Octgn.Data;
using Octgn.Data.Models;
using Octgn.Peer;

namespace Octgn.App.Areas.api.Controllers
{
    public class PeerController : Controller
    {
		[AcceptVerbs("get","post")]
        public ActionResult PeerCount()
        {
			return Json(new{Count=PeerHandler.Swarm.Peers.Count}, JsonRequestBehavior.AllowGet);
        }

    }
}
