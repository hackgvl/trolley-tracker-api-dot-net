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
using NLog;

namespace TrolleyTracker.Controllers
{
    public class RoutesController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: Routes
        public ActionResult Index()
        {
            var routes = from r in db.Routes
                        orderby r.ShortName
                        select r;

            return View(routes.ToList());
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

            var r = new Route();
            r.RouteColorRGB = "#008000";  // Must have starting value for Farbtastic
            return View(r);
        }

        // GET: Routes/CreateVariationFrom/5
        [CustomAuthorize(Roles = "RouteManagers")]
        public ActionResult CreateVariationFrom(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var r = new Route();
            r.RouteColorRGB = "#008000";  // Must have starting value for Farbtastic
            Route referenceRoute = db.Routes.Find(id);
            if (referenceRoute != null)
            {
                r.RouteColorRGB = referenceRoute.RouteColorRGB;
            }
            return View("Create", r);
        }

        // POST: Routes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [CustomAuthorize(Roles = "RouteManagers")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,ShortName,LongName,Description,FlagStopsOnly,RouteColorRGB")] Route route)
        {
            if (ModelState.IsValid)
            {

                var testRoute = from r in db.Routes
                                where r.ShortName == route.ShortName
                                select r;
                if (testRoute.Count<Route>() > 0)
                {
                    ViewBag.ErrorMessage = $"Unable to create - Route name {route.ShortName} already exists";
                    return View("Create", route);
                }
                               


                db.Routes.Add(route);
                db.SaveChanges();

                logger.Info($"Created route '{route.ShortName}' ({route.Description})");

                var uploadAlsoFlag = Request.Form["UploadAlso"];
                if (uploadAlsoFlag != null)
                {
                    if (uploadAlsoFlag == "yes")
                    {
                        return RedirectToAction("Create", "UploadKMLShape", new { Id = route.ID });
                    }
                    else
                    {
                        // List all routes
                        return RedirectToAction("Index");
                    }
                }

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
                var coord = new Coordinate(point.Lat, point.Lon);
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
        public ActionResult Edit([Bind(Include = "ID,ShortName,LongName,Description,FlagStopsOnly,RouteColorRGB")] Route route)
        {
            if (ModelState.IsValid)
            {
                db.Entry(route).State = EntityState.Modified;
                db.SaveChanges();
                logger.Info($"Updated route '{route.ShortName}' ({route.Description})");
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
            logger.Info($"Deleted route '{route.ShortName}' ({route.Description})");
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
