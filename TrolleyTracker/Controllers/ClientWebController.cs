using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Models;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace TrolleyTracker.Controllers
{
    public class ClientWebController : Controller
    {
        static object lockObject = new object();
        static string mainWebPageCache = null;
        static DateTime lastWebPageCreationTime;
        const int MaxCacheSeconds = 60;  // Time between page loads before data is re-queried

        // GET: ClientWeb
        public ActionResult Index()
        {
            string clientWebTemplate = null;
            lock (lockObject)
            {
                if (!PageAvailableFromCache(ref clientWebTemplate))
                {
                    clientWebTemplate = System.IO.File.ReadAllText(Server.MapPath("/Content/ClientWeb/index.html"));

                    using (var db = new TrolleyTrackerContext())
                    {

                        clientWebTemplate = clientWebTemplate.Replace("%routedata%", ActiveRouteDetailJSON(db));
                        clientWebTemplate = clientWebTemplate.Replace("%scheduledata%", EffectiveScheduleJSON(db, null, -1));
                        clientWebTemplate = clientWebTemplate.Replace("%fulltrolleydata%", TrolleysJSON(db));
                    }
                    UpdateMainPageCache(clientWebTemplate);
                }
            }
            ViewBag.ClientWebPage = clientWebTemplate;
            // Use PartialView so that page is shown without any standard layout
            return PartialView();
        }

        private void UpdateMainPageCache(string clientWebTemplate)
        {
            mainWebPageCache = clientWebTemplate;
            lastWebPageCreationTime = DateTime.Now;
        }

        private bool PageAvailableFromCache(ref string clientWebTemplate)
        {
            if (mainWebPageCache == null) return false;
            if ((DateTime.Now - lastWebPageCreationTime).TotalSeconds > MaxCacheSeconds) return false;
            clientWebTemplate = mainWebPageCache;
            return true;
        }



        // GET: ClientWeb/RouteView/5
        public ActionResult RouteView(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var db = new TrolleyTrackerContext())
            {
                var route = db.Routes.Find(id);
                if (route == null)
                {
                    return HttpNotFound();
                }

                var scheduleWebTemplate = System.IO.File.ReadAllText(Server.MapPath("/Content/ClientWeb/schedule.html"));

                scheduleWebTemplate = scheduleWebTemplate.Replace("%routedata%", SingleRouteDetailJSON(route, db));
                var runsOnSchedule = new List<String>();
                scheduleWebTemplate = scheduleWebTemplate.Replace("%scheduledata%", EffectiveScheduleJSON(db, runsOnSchedule, route.ID));

                string runsOnJSON = JsonConvert.SerializeObject(runsOnSchedule);
                scheduleWebTemplate = scheduleWebTemplate.Replace("%runs_on%", runsOnJSON);

                ViewBag.ClientWebPage = scheduleWebTemplate;
                // Use PartialView so that page is shown without any standard layout
                return PartialView();
            }
        }

        private String SingleRouteDetailJSON(Route route, TrolleyTrackerContext db)
        {
            var routeDetailList = new List<RouteDetail>();  // Schedule web page expects array, even for single route
            var routeDetail = new RouteDetail(route);
            routeDetail.AddRouteDetail(db, route);
            routeDetailList.Add(routeDetail);

            string routeDetailJSON = JsonConvert.SerializeObject(routeDetailList);

            return routeDetailJSON;

        }

        private String TrolleysJSON(TrolleyTrackerContext db)
        {
            var trolleysList = db.Trolleys.ToList();
            string trolleysJSON = JsonConvert.SerializeObject(trolleysList);

            return trolleysJSON;
        }

        /// <summary>
        /// Build effective schedule string for client web page, with optional single route schedule list
        /// </summary>
        /// <param name="db"></param>
        /// <param name="runsOnSchedule">String list to hold optional single route list</param>
        /// <param name="routeID">Route to create runsOnSchedule list for, -1 for don't care</param>
        /// <returns></returns>
        private string EffectiveScheduleJSON(TrolleyTrackerContext db, List<String> runsOnSchedule, int routeID)
        {
            var effectiveScheduleList = WebAPI.RouteSchedulesController.GetSchedules();

            // Massage and convert for display on web page - web page uses in reverse order
            // "<b><a href=\"schedule/18/\">Heart Of Main:</a></b><br>Sunday 5:30 PM - 8:00 PM", 

            // var runson = ["Sunday: 1:00 PM - 8:00 PM", "Saturday: 10:00 AM - 11:00 PM", "Friday: 6:00 PM - 11:00 PM", "Thursday: 6:00 PM - 11:00 PM"];

            var webScheduleList = new List<String>();
            foreach (var schedule in effectiveScheduleList)
            {
                var thisSchedule = $"<b><a href=\"/ClientWeb/RouteView/{schedule.RouteID}/\">{HttpUtility.HtmlEncode(schedule.RouteLongName)}:</a></b><br>{schedule.DayOfWeek} {schedule.StartTime} - {schedule.EndTime}";
                webScheduleList.Insert(0, thisSchedule);  // For reverse sorting

                if (schedule.RouteID == routeID)
                {
                    runsOnSchedule.Add($"{schedule.DayOfWeek} {schedule.StartTime} - {schedule.EndTime}");
                }
            }

            string effectiveScheduleJSON = JsonConvert.SerializeObject(webScheduleList);

            return effectiveScheduleJSON;
        }

        private string ActiveRouteDetailJSON(TrolleyTrackerContext db)
        {
            var activeRouteSummaries = ActiveRoutes.GetActiveRoutes();
            var routeDetailList = new List<RouteDetail>();
            foreach (var routeSummary in activeRouteSummaries)
            {
                var route = db.Routes.Find(routeSummary.ID);
                var routeDetail = new RouteDetail(route);
                routeDetail.AddRouteDetail(db, route);
                routeDetailList.Add(routeDetail);
            }

            string routeDetailJSON = JsonConvert.SerializeObject(routeDetailList);

            return routeDetailJSON;
        }

    }
}