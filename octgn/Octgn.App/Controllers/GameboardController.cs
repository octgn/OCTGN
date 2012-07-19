using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace Octgn.App.Controllers
{
    public class GameboardController : Controller
    {
        //
        // GET: /Gameboard/

        public ActionResult Index()
        {
            
            return View();
        }

    }
}
