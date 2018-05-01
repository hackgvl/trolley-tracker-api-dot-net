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
    [CustomAuthorize(Roles = "Administrators")]
    public class AppSettingsController : Controller
    {
        // GET: AppSettings
        public ActionResult Index()
        {

            using (var db = new TrolleyTrackerContext())
            {
                return View(db.AppSettings.ToList());
            }
        }

        // GET: AppSettings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new TrolleyTrackerContext())
            {
                AppSettings appSettings = db.AppSettings.Find(id);
                if (appSettings == null)
                {
                    return HttpNotFound();
                }
                return View(appSettings);
            }
        }

        // GET: AppSettings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AppSettings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UseSyncromatics")] AppSettings appSettings)
        {

            if (ModelState.IsValid)
            {
                using (var db = new TrolleyTrackerContext())
                {
                    db.AppSettings.Add(appSettings);
                    db.SaveChanges();
                }
                AppSettingsInterface.UpdateSettings(appSettings);
                return RedirectToAction("Index");
            }

            return View(appSettings);
        }

        // GET: AppSettings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new TrolleyTrackerContext())
            {
                AppSettings appSettings = db.AppSettings.Find(id);

                if (appSettings == null)
                {
                    return HttpNotFound();
                }
                return View(appSettings);
            }
        }

        // POST: AppSettings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UseSyncromatics")] AppSettings appSettings)
        {
            if (ModelState.IsValid)
            {

                using (var db = new TrolleyTrackerContext())
                {
                    db.Entry(appSettings).State = EntityState.Modified;
                    db.SaveChanges();
                    AppSettingsInterface.UpdateSettings(appSettings);
                    return RedirectToAction("Index");
                }
            }
            return View(appSettings);
        }

        // GET: AppSettings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new TrolleyTrackerContext())
            {
                AppSettings appSettings = db.AppSettings.Find(id);
                if (appSettings == null)
                {
                    return HttpNotFound();
                }
                return View(appSettings);
            }
        }

        // POST: AppSettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {

            using (var db = new TrolleyTrackerContext())
            {
                AppSettings appSettings = db.AppSettings.Find(id);
                db.AppSettings.Remove(appSettings);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
