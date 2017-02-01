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
    public class LogsController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: Logs
        /// <summary>
        /// Get specified page of log entries, newest first
        /// </summary>
        /// <param name="pageIndex">0 based page number</param>
        /// <returns></returns>
        public ActionResult Index(int pageIndex=0)
        {

            if (pageIndex < 0)
            {
                pageIndex = 0;
            }
            int pageSize = 40;


            int totalRecords = db.Logs.Count();
            int totalPageCount = (totalRecords / pageSize) + ((totalRecords % pageSize) > 0 ? 1 : 0);
            var query = db.Logs.OrderByDescending(l => l.Logged).Skip((pageIndex * pageSize)).Take(pageSize).ToList();

            ReformatQueryItems(query);

            ViewBag.dbCount = totalRecords;
            ViewBag.pageSize = pageSize;
            ViewBag.totalPageCount = totalPageCount;
            return View(query);

            //return View(db.Logs.ToList());
        }

        // Separate long run-on strings by adding a space between separators to 
        // allow table to wrap those fields
        private void ReformatQueryItems(List<Log> query)
        {
            foreach(var log in query)
            {
                log.Username = log.Username.Replace(":", ": ");
                log.Callsite = log.Callsite.Replace(".", ". ");
            }
        }


        // GET: Logs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Log log = db.Logs.Find(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // GET: Logs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Logs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Logged,Level,Message,Username,RemoteAddress,Callsite,Exception")] Log log)
        {
            if (ModelState.IsValid)
            {
                db.Logs.Add(log);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(log);
        }

        // GET: Logs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Log log = db.Logs.Find(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // POST: Logs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Logged,Level,Message,Username,RemoteAddress,Callsite,Exception")] Log log)
        {
            if (ModelState.IsValid)
            {
                db.Entry(log).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(log);
        }

        // GET: Logs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Log log = db.Logs.Find(id);
            if (log == null)
            {
                return HttpNotFound();
            }
            return View(log);
        }

        // POST: Logs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Log log = db.Logs.Find(id);
            db.Logs.Remove(log);
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
