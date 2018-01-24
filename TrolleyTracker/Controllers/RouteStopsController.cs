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
    public class RouteStopsController : Controller
    {

        // GET: RouteStops
        public ActionResult Index()
        {
            using (var db = new TrolleyTrackerContext())
            {
                var routeStops = db.RouteStops.Include(r => r.Route).Include(r => r.Stop);
                return View(routeStops.ToList());
            }
        }

        // GET: RouteStops/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var db = new TrolleyTrackerContext())
            {
                RouteStop routeStop = db.RouteStops.Find(id);
                if (routeStop == null)
                {
                    return HttpNotFound();
                }
                return View(routeStop);
            }
        }

        // GET: RouteStops/Create
        public ActionResult Create()
        {
            using (var db = new TrolleyTrackerContext())
            {
                ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName");
                ViewBag.StopID = new SelectList(db.Stops, "ID", "Name");
                return View();
            }
        }

        // POST: RouteStops/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,RouteID,StopID,StopSequence")] RouteStop routeStop)
        {
            using (var db = new TrolleyTrackerContext())
            {
                if (ModelState.IsValid)
                {
                    db.RouteStops.Add(routeStop);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName", routeStop.RouteID);
                ViewBag.StopID = new SelectList(db.Stops, "ID", "Name", routeStop.StopID);
                return View(routeStop);
            }
        }


        // GET: RouteStops/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var db = new TrolleyTrackerContext())
            {
                RouteStop routeStop = db.RouteStops.Find(id);
                if (routeStop == null)
                {
                    return HttpNotFound();
                }
                ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName", routeStop.RouteID);
                ViewBag.StopID = new SelectList(db.Stops, "ID", "Name", routeStop.StopID);
                return View(routeStop);
            }
        }

        // POST: RouteStops/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,RouteID,StopID,StopSequence")] RouteStop routeStop)
        {
            using (var db = new TrolleyTrackerContext())
            {
                if (ModelState.IsValid)
                {
                    db.Entry(routeStop).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName", routeStop.RouteID);
                ViewBag.StopID = new SelectList(db.Stops, "ID", "Name", routeStop.StopID);
                return View(routeStop);
            }
        }

        // GET: RouteStops/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var db = new TrolleyTrackerContext())
            {
                RouteStop routeStop = db.RouteStops.Find(id);
                if (routeStop == null)
                {
                    return HttpNotFound();
                }
                return View(routeStop);
            }
        }

        // POST: RouteStops/Delete/5
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var db = new TrolleyTrackerContext())
            {
                RouteStop routeStop = db.RouteStops.Find(id);
                db.RouteStops.Remove(routeStop);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
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
