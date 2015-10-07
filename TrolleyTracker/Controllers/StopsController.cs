using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;
using System.Drawing;

namespace TrolleyTracker.Controllers
{
    public class StopsController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: Stops
        public ActionResult Index()
        {
            //return PartialView(db.Stops.ToList());
            var stopsView = new List<StopSummary>();
            foreach(var stop in db.Stops)
            {
                stopsView.Add(new StopSummary(stop));
            }
            var jsonResult = Json(new { Stops = stopsView }, JsonRequestBehavior.AllowGet);

            string json = new JavaScriptSerializer().Serialize(jsonResult.Data);

            ViewData["StopsJSON"] = json;
            return PartialView();
        }

        // GET: Stops/List
        public ActionResult List()
        {
            return View(db.Stops.ToList());
        }

        // GET: Stops/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stop stop = db.Stops.Find(id);
            if (stop == null)
            {
                return HttpNotFound();
            }
            return View(stop);
        }


        // GET: Stops/Picture/5
        public ActionResult Picture(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stop stop = db.Stops.Find(id);
            if (stop == null)
            {
                return HttpNotFound();
            }
            return PartialView(stop);
        }

        // GET: Stops/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Create()
        {
            return View();
        }

        // GET: Stops/CreateAtPosition
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult CreateAtPosition(double lat, double lon)
        {
            ViewBag.Lat = lat;
            ViewBag.Lon = lon;
            return View();
        }

        // POST: Stops/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,StopSequence,Name,Description,Lat,Lon")] Stop stop)
        {
            if (ModelState.IsValid)
            {
                var file = Request.Files["Picture"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    stop.Picture = fileBytes;
                }

                db.Stops.Add(stop);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stop);
        }

        //private byte[] CheckForImageResize(HttpPostedFileBase file)
        //{
        //    //string fileName = file.FileName;
        //    //string fileContentType = file.ContentType;
        //    //byte[] fileBytes = new byte[file.ContentLength];
        //    //file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

        //    var uploadedImage = Image.FromStream(file.InputStream, true, true);
        //    if (uploadedImage.Width > 700 || uploadedImage.Height > 700)
        //    {
        // ** CPU Intensive routine
        //        uploadedImage = ResizeImage(uploadedImage);
        //    }

        //}

        // POST: Stops/CreateAtPosition
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAtPosition([Bind(Include = "ID,StopSequence,Name,Description,Lat,Lon")] Stop stop)
        {
            if (ModelState.IsValid)
            {
                var file = Request.Files["Picture"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    stop.Picture = fileBytes;
                }
                db.Stops.Add(stop);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stop);
        }

        // GET: Stops/Edit/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stop stop = db.Stops.Find(id);
            if (stop == null)
            {
                return HttpNotFound();
            }
            return View(stop);
        }

        // POST: Stops/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,StopSequence,Name,Description,Lat,Lon")] Stop stop)
        {
            if (ModelState.IsValid)
            {
                var newStop = new Stop();
                newStop.ID = stop.ID;
                db.Stops.Attach(newStop);  // Attach used instead of EntityState.Modified so that only changed fields are saved
                newStop.Lat = stop.Lat;
                newStop.Lon = stop.Lon;
                newStop.Name = stop.Name;
                newStop.Description = stop.Description;

                var file = Request.Files["Picture"];
                if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                    newStop.Picture = fileBytes;
                }

                //db.Entry(stop).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stop);
        }

        // GET: Stops/Delete/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stop stop = db.Stops.Find(id);
            if (stop == null)
            {
                return HttpNotFound();
            }
            return View(stop);
        }

        // POST: Stops/Delete/5
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stop stop = db.Stops.Find(id);
            db.Stops.Remove(stop);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // POST: Stops/UpdatePosition/5
        [HttpPost, ActionName("UpdatePosition")]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePosition(int id, double Lat, double Lon)
        {
            Stop stop = db.Stops.Find(id);
            stop.Lat = Lat;
            stop.Lon = Lon;
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
