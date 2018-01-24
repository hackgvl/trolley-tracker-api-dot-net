using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class RouteScheduleOverridesController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: RouteScheduleOverrides
        public ActionResult Index()
        {
            List<RouteScheduleOverride> routeScheduleOverrides = null;
            using (var db = new TrolleyTrackerContext())
            {
                routeScheduleOverrides = (from rso in db.RouteScheduleOverrides.Include(rso => rso.NewRoute).Include(rso => rso.OverriddenRoute)
                                              orderby rso.OverrideDate, rso.StartTime, rso.NewRoute.ShortName
                                              select rso).ToList();
            }

            ViewBag.CssFile = Url.Content("~/Content/RouteScheduleSummary.css");
            var routeScheduleView = BuildScheduleView.ConfigureScheduleView(true);
            routeScheduleView.RouteScheduleOverrides = routeScheduleOverrides;

            return View(routeScheduleView);
        }

        // GET: RouteScheduleOverrides/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteScheduleOverride routeScheduleOverride = null;
            using (var db = new TrolleyTrackerContext())
            {
                routeScheduleOverride = (from rso in db.RouteScheduleOverrides.Include(rso => rso.NewRoute).Include(rso => rso.OverriddenRoute)
                                                               where rso.ID == id
                                                               select rso).FirstOrDefault();
            }

            if (routeScheduleOverride == null)
            {
                return HttpNotFound();
            }
            return View(routeScheduleOverride);
        }

        /// <summary>
        /// Create sorted route list, with first item representing null entry choice as ID=-1
        /// </summary>
        /// <param name="nullLabel"></param>
        /// <returns></returns>
        private List<Route> RouteSelectList(string nullLabel)
        {
            using (var db = new TrolleyTrackerContext())
            {
                var routeList = db.Routes.OrderBy(r => r.ShortName).ToList();

                var nullRoute = new Route();
                nullRoute.ID = -1;
                nullRoute.ShortName = nullLabel;
                routeList.Insert(0, nullRoute);
                return routeList;
            }

        }

        // GET: RouteScheduleOverrides/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Create()
        {
            List<Route> routeList = null;
            using (var db = new TrolleyTrackerContext())
            {
                routeList = db.Routes.OrderBy(r => r.ShortName).ToList();
            }

            ViewBag.NewRouteID = new SelectList(RouteSelectList(""), "ID", "ShortName");
            ViewBag.OverriddenRouteID = new SelectList(RouteSelectList("** All **"), "ID", "ShortName");
            var routeScheduleOverride = new RouteScheduleOverride();
            routeScheduleOverride.OverrideType = RouteScheduleOverride.OverrideRule.NoService;
            routeScheduleOverride.OverrideDate = DateTime.Now.AddDays(1);
            routeScheduleOverride.StartTime = new DateTime(1970, 1, 1, 18, 00, 00);
            routeScheduleOverride.EndTime = new DateTime(1970, 1, 1, 22, 00, 00);
            return View(routeScheduleOverride);
        }


        /// <summary>
        /// Effectively strip off year by forcing everything to a fixed year
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        private DateTime ExtractTimeValue(DateTime startTime)
        {
            return new DateTime(1970, 1, 1, startTime.Hour, startTime.Minute, startTime.Second);
        }



        // POST: RouteScheduleOverrides/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,NewRouteID,OverrideDate,OverrideType,OverriddenRouteID,StartTime,EndTime")] RouteScheduleOverride routeScheduleOverride)
        {
            bool ok = true;
            if (routeScheduleOverride.OverrideType != RouteScheduleOverride.OverrideRule.NoService && 
                routeScheduleOverride.NewRouteID == -1)
            {
                ModelState.AddModelError("NewRouteID", "Added or Replacement route is required");
                ok = false;
            }

            if (routeScheduleOverride.StartTime > routeScheduleOverride.EndTime)
            {
                ModelState.AddModelError("StartTime", "Start time must be before end time");
                ok = false;
            }


            if (ok && ModelState.IsValid)
            {
                routeScheduleOverride.StartTime = ExtractTimeValue(routeScheduleOverride.StartTime);
                routeScheduleOverride.EndTime = ExtractTimeValue(routeScheduleOverride.EndTime);

                if (routeScheduleOverride.NewRouteID == -1)
                {
                    // Was cancellation case
                    routeScheduleOverride.NewRouteID = null;
                }
                if (routeScheduleOverride.OverriddenRouteID == -1)
                {
                    // No specific target route
                    routeScheduleOverride.OverriddenRouteID = null;
                }

                using (var db = new TrolleyTrackerContext())
                {
                    db.RouteScheduleOverrides.Add(routeScheduleOverride);
                    db.SaveChanges();
                    PurgeOldOverrides(db);
                }

                logger.Info($"Created special schedule ID #{routeScheduleOverride.ID} type '{routeScheduleOverride.OverrideType.ToString()}' at {routeScheduleOverride.OverrideDate} '{routeScheduleOverride.StartTime.TimeOfDay} - {routeScheduleOverride.EndTime.TimeOfDay}");

                return RedirectToAction("Index");
            }

            ViewBag.NewRouteID = new SelectList(RouteSelectList(""), "ID", "ShortName", routeScheduleOverride.NewRouteID);
            ViewBag.OverriddenRouteID = new SelectList(RouteSelectList("** All **"), "ID", "ShortName", routeScheduleOverride.OverriddenRouteID);
            return View(routeScheduleOverride);
        }

        private void PurgeOldOverrides(TrolleyTrackerContext db)
        {
            var purgeDate = DateTime.Now.AddDays(-7);
            var oldRouteScheduleOverrides = (from rso in db.RouteScheduleOverrides
                                            where rso.OverrideDate < purgeDate
                                            select rso).ToList();

            oldRouteScheduleOverrides.ForEach(rso => db.RouteScheduleOverrides.Remove(rso));
            db.SaveChanges();

            int nPurged = oldRouteScheduleOverrides.Count<RouteScheduleOverride>();
            if (nPurged > 0)
            {
                logger.Info($"Purged {nPurged} special schedules");
            }

        }

        // GET: RouteScheduleOverrides/Edit/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteScheduleOverride routeScheduleOverride = null;
            using (var db = new TrolleyTrackerContext())
            {
                routeScheduleOverride = db.RouteScheduleOverrides.Find(id);
            }
            if (routeScheduleOverride == null)
            {
                return HttpNotFound();
            }

            ViewBag.NewRouteID = new SelectList(RouteSelectList(""), "ID", "ShortName", routeScheduleOverride.NewRouteID ?? -1);
            ViewBag.OverriddenRouteID = new SelectList(RouteSelectList("** All **"), "ID", "ShortName", routeScheduleOverride.OverriddenRouteID ?? -1);

            return View(routeScheduleOverride);
        }

        // POST: RouteScheduleOverrides/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,NewRouteID,OverrideDate,OverrideType,OverriddenRouteID,StartTime,EndTime")] RouteScheduleOverride routeScheduleOverride)
        {
            bool ok = true;

            if (routeScheduleOverride.OverrideType != RouteScheduleOverride.OverrideRule.NoService &&
                routeScheduleOverride.NewRouteID == -1)
            {
                ModelState.AddModelError("NewRouteID", "Added or Replacement route is required");
                ok = false;
            }

            if (routeScheduleOverride.StartTime > routeScheduleOverride.EndTime)
            {
                ModelState.AddModelError("StartTime", "Start time must be before end time");
                ok = false;
            }

            if (ok && ModelState.IsValid)
            {
                if (routeScheduleOverride.NewRouteID == -1)
                {
                    // Was cancellation case
                    routeScheduleOverride.NewRouteID = null;
                }
                if (routeScheduleOverride.OverriddenRouteID == -1)
                {
                    // No specific target route
                    routeScheduleOverride.OverriddenRouteID = null;
                }
                routeScheduleOverride.StartTime = ExtractTimeValue(routeScheduleOverride.StartTime);
                routeScheduleOverride.EndTime = ExtractTimeValue(routeScheduleOverride.EndTime);

                using (var db = new TrolleyTrackerContext())
                {
                    db.Entry(routeScheduleOverride).State = EntityState.Modified;
                    db.SaveChanges();
                }

                logger.Info($"Edited special schedule type '{routeScheduleOverride.OverrideType.ToString()}' at  {routeScheduleOverride.OverrideDate} '{routeScheduleOverride.StartTime.TimeOfDay} - {routeScheduleOverride.EndTime.TimeOfDay}");

                return RedirectToAction("Index");
            }
            ViewBag.NewRouteID = new SelectList(RouteSelectList(""), "ID", "ShortName", routeScheduleOverride.NewRouteID);
            ViewBag.OverriddenRouteID = new SelectList(RouteSelectList("** All **"), "ID", "ShortName", routeScheduleOverride.OverriddenRouteID);
            return View(routeScheduleOverride);
        }

        // GET: RouteScheduleOverrides/Delete/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteScheduleOverride routeScheduleOverride = null;
            using (var db = new TrolleyTrackerContext())
            {
                routeScheduleOverride = (from rso in db.RouteScheduleOverrides.Include(rso => rso.NewRoute).Include(rso => rso.OverriddenRoute)
                                         where rso.ID == id
                                         select rso).FirstOrDefault();
            }
            if (routeScheduleOverride == null)
            {
                return HttpNotFound();
            }
            return View(routeScheduleOverride);
        }

        // POST: RouteScheduleOverrides/Delete/5
        [CustomAuthorize(Roles = "RouteManagers")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new TrolleyTrackerContext())
            {
                RouteScheduleOverride routeScheduleOverride = db.RouteScheduleOverrides.Find(id);
                logger.Info($"Deleted special schedule type '{routeScheduleOverride.OverrideType.ToString()}' at  {routeScheduleOverride.OverrideDate} '{routeScheduleOverride.StartTime.TimeOfDay} - {routeScheduleOverride.EndTime.TimeOfDay}");
                db.RouteScheduleOverrides.Remove(routeScheduleOverride);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing)
            //{
            //    db.Dispose();
            //}
            base.Dispose(disposing);
        }
    }
}
