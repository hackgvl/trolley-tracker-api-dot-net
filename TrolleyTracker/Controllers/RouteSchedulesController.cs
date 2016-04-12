using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;
using MvcSchedule.Objects;

namespace TrolleyTracker.Controllers
{
    public class RouteSchedulesController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: RouteSchedules
        public ActionResult Index()
        {
            //var routeSchedules = db.RouteSchedules.Include(r => r.Route);

            ViewBag.DaysOfWeek = BuildScheduleView.daysOfWeek;

            ViewBag.ServerTime = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow).ToString("MM-dd-yyyy HH:mm:ss");

            ViewBag.CssFile = Url.Content("~/Content/RouteScheduleSummary.css");
            return View(BuildScheduleView.ConfigureScheduleView(db, false));




            //return View(routeSchedules.ToList());
        }

        // GET: RouteSchedules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteSchedule routeSchedule = db.RouteSchedules.Find(id);

            ViewBag.StrWeekday = BuildScheduleView.daysOfWeek[routeSchedule.DayOfWeek];

            if (routeSchedule == null)
            {
                return HttpNotFound();
            }
            return View(routeSchedule);
        }

        // GET: RouteSchedules/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Create()
        {
            var routeList = db.Routes.OrderBy(r => r.ShortName).ToList();
            ViewBag.RouteID = new SelectList(routeList, "ID", "ShortName");

            ViewBag.DayOfWeek = GetWeekDaySelectorFor(4);

            return View();
        }

        // POST: RouteSchedules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RouteID,DayOfWeek,StartTime,EndTime")] RouteSchedule routeSchedule)
        {

            if (ModelState.IsValid)
            {
                routeSchedule.StartTime = ExtractTimeValue(routeSchedule.StartTime);
                routeSchedule.EndTime = ExtractTimeValue(routeSchedule.EndTime);
                db.RouteSchedules.Add(routeSchedule);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName", routeSchedule.RouteID);



            return View(routeSchedule);
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

        // GET: RouteSchedules/Edit/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteSchedule routeSchedule = db.RouteSchedules.Find(id);
            if (routeSchedule == null)
            {
                return HttpNotFound();
            }
            var routeList = db.Routes.OrderBy(r => r.ShortName).ToList();
            ViewBag.RouteID = new SelectList(routeList, "ID", "ShortName", routeSchedule.RouteID);
            ViewBag.DayOfWeek = GetWeekDaySelectorFor(routeSchedule.DayOfWeek);
            return View(routeSchedule);
        }

        // POST: RouteSchedules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RouteID,DayOfWeek,StartTime,EndTime")] RouteSchedule routeSchedule)
        {
            if (ModelState.IsValid)
            {
                routeSchedule.StartTime = ExtractTimeValue(routeSchedule.StartTime);
                routeSchedule.EndTime = ExtractTimeValue(routeSchedule.EndTime);
                db.Entry(routeSchedule).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName", routeSchedule.RouteID);
            return View(routeSchedule);
        }

        // GET: RouteSchedules/Delete/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RouteSchedule routeSchedule = db.RouteSchedules.Find(id);
            if (routeSchedule == null)
            {
                return HttpNotFound();
            }
            ViewBag.StrWeekday = BuildScheduleView.daysOfWeek[routeSchedule.DayOfWeek];

            return View(routeSchedule);
        }

        // POST: RouteSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RouteSchedule routeSchedule = db.RouteSchedules.Find(id);
            db.RouteSchedules.Remove(routeSchedule);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        private SelectList GetWeekDaySelectorFor(int dayOfWeek)
        {
            return new SelectList(new[]
                                  {
                                        new{DayOfWeek="0",Name="Sunday"},
                                        new{DayOfWeek="1",Name="Monday"},
                                        new{DayOfWeek="2",Name="Tuesday"},
                                        new{DayOfWeek="3",Name="Wednesday"},
                                        new{DayOfWeek="4",Name="Thursday"},
                                        new{DayOfWeek="5",Name="Friday"},
                                        new{DayOfWeek="6",Name="Saturday"},
            },
            "DayOfWeek", "Name", dayOfWeek);


        }
    }
}
