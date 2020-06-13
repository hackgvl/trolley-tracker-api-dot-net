using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class RescanRouteStopsController : Controller
    {


        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        private static List<int> routeList = new List<int>();
        private static int routeIndex = 0;
        private static bool running = false;



        // GET: RescanRouteStops
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost, ActionName("Start")]
        [CustomAuthorize(Roles = "RouteManagers")]
        // POST: RescanRouteStops/Start
        public async Task<ActionResult> Start()
        {
            await StartRescanThread();

            ViewBag.Step = "Running";
            ViewBag.Total = routeList.Count;
            ViewBag.Current = routeIndex;

            return RedirectToAction("Status", "RescanRouteStops");
        }


        // GET: RescanRouteStops/Status
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Status(int? num)
        {

            if (running)
            {
                ViewBag.Step = "Running";
                ViewBag.Total = routeList.Count;
                ViewBag.Current = routeIndex;
            }
            else
            {
                ViewBag.Step = "Finished";
            }

            return View();
        }



        private async Task StartRescanThread()
        {
            routeList = await (from routes in db.Routes
                                   select routes.ID).ToListAsync();
            routeIndex = 0;
            running = true;

            var threadStart = new ThreadStart(RescanThread);
            var rescanThread = new Thread(threadStart);
            rescanThread.Start();
        }



        private static void RescanThread()
        {
            TrolleyTrackerContext db = new TrolleyTrackerContext();
            var assignStopsToRoutes = new AssignStopsToRoutes();
            for (routeIndex=0; routeIndex < routeList.Count; routeIndex++)
            {
                assignStopsToRoutes.UpdateStopsForRoute(db, routeList[routeIndex]);

            }
            running = false;
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
