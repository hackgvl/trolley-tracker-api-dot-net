using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class RoutesController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: Routes
        public ActionResult Index()
        {
            return View(db.Routes.ToList());
        }

        // GET: Routes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        // GET: Routes/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Routes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,ShortName,LongName,Description,FlagStopsOnly")] Route route)
        {
            if (ModelState.IsValid)
            {
                db.Routes.Add(route);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(route);
        }

        // GET: Routes/RouteShape/5
        public ActionResult RouteShape(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Include raw street data
            var baseDir = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin", string.Empty) + @"\StreetData";
            var filePath = baseDir + "\\AllStreetPaths.osm";

            string xml = "";
            using (var streetFile = new StreamReader(filePath))
            {
                xml = streetFile.ReadToEnd();
                streetFile.Close();
            }
            // Replace single quotes with double quotes so that javascript can define variable in single quotes
            xml = xml.Replace('\'', '"');
            xml = xml.Replace("\r\n", "");

            ViewData["StreetDataXML"] = xml;

            var routeShape = from shape in db.Shapes
                          orderby shape.Sequence
                          where (shape.RouteID == id)
                          select shape;

            var shapeList = new List<Coordinate>();
            foreach(var point in routeShape)
            {
                var coord = new Coordinate(-1, point.Lat, point.Lon, null);
                shapeList.Add(coord);
            }
            string shapeJSON = new JavaScriptSerializer().Serialize(shapeList);

            ViewData["RouteShapeJSON"] = shapeJSON;

            return PartialView();
        }

        // GET: Routes/Edit/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        // POST: Routes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,ShortName,LongName,Description,FlagStopsOnly")] Route route)
        {
            if (ModelState.IsValid)
            {
                db.Entry(route).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(route);
        }

        // GET: Routes/Delete/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Route route = db.Routes.Find(id);
            if (route == null)
            {
                return HttpNotFound();
            }
            return View(route);
        }

        // POST: Routes/Delete/5
        [HttpPost, ActionName("Delete")]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Route route = db.Routes.Find(id);
            db.Routes.Remove(route);
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
