using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TrolleyTracker.Controllers
{
    public class DBVisualizerController : Controller
    {
        // GET: DBVisualizer
        public ActionResult Index()
        {
            // Use PartialView so that page is shown without any standard layout
            return PartialView();
        }
    }
}