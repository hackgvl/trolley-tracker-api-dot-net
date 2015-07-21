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
    public class TrolleysController : Controller
    {
        private TrolleyTrackerEntities db = new TrolleyTrackerEntities();

        //// GET: Trolleys
        //public ActionResult Index()
        //{
        //    return View(db.Trolleys.ToList());
        //}
        // GET: TrolleyDBs
        public ActionResult Index()
        {
            var trolleys = from t in db.Trolleys
                           orderby t.TrolleyName, t.Number
                           select t;
            return View(trolleys.ToList());
        }


        // GET: Trolleys/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trolley trolley = db.Trolleys.Find(id);
            if (trolley == null)
            {
                return HttpNotFound();
            }
            return View(trolley);
        }

        // GET: Trolleys/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Trolleys/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,TrolleyName,Number,CurrentLat,CurrentLon")] Trolley trolley)
        {
            if (ModelState.IsValid)
            {
                db.Trolleys.Add(trolley);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(trolley);
        }

        // GET: Trolleys/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trolley trolley = db.Trolleys.Find(id);
            if (trolley == null)
            {
                return HttpNotFound();
            }
            return View(trolley);
        }

        // POST: Trolleys/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,TrolleyName,Number,CurrentLat,CurrentLon")] Trolley trolley)
        {
            if (ModelState.IsValid)
            {
                db.Entry(trolley).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(trolley);
        }

        // GET: Trolleys/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Trolley trolley = db.Trolleys.Find(id);
            if (trolley == null)
            {
                return HttpNotFound();
            }
            return View(trolley);
        }

        // POST: Trolleys/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Trolley trolley = db.Trolleys.Find(id);
            db.Trolleys.Remove(trolley);
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
    }
}
