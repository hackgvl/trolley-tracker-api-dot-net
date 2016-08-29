using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class BulkUploadShapesController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: BulkUploadShapes
        public ActionResult Index()
        {
            return View();
        }

        // GET: BulkUploadShapes/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BulkUploadShapes/Create
        public ActionResult Create()
        {
            ViewBag.RouteID = new SelectList(db.Routes, "ID", "ShortName");
            return View();
        }

        // POST: BulkUploadShapes/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                Coordinate lastCoordinate = null;

                var strRouteID = collection.Get("RouteID");


                // Parse Geo-JSON formatted route coordinates
                var jsonShapes = collection.Get("JSONText");
                if (jsonShapes != null && strRouteID != null)
                {
                    int routeID = Convert.ToInt32(strRouteID);

                    using (var db = new TrolleyTracker.Models.TrolleyTrackerContext())
                    {
                        RemoveOldShape(routeID, db);

                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

                        dynamic shapeData = jss.Deserialize(jsonShapes, typeof(object)) as dynamic;

                        var features = shapeData.features;
                        var geometryBlock = features[0];
                        var geometry = geometryBlock["geometry"];
                        var geoJSONtype = geometry["type"];
                        var coordinates = geometry["coordinates"];

                        int segmentCount = coordinates.Count;
                        if (geoJSONtype == "LineString") segmentCount = 1;
                        int sequence = 0;
                        double totalDistance = 0.0;
                        for (int seg = 0; seg < segmentCount; seg++)
                        {
                            var coordArray = coordinates[seg];
                            if (geoJSONtype == "LineString") coordArray = coordinates;
                            int nodeCount = coordArray.Count;
                            for (int i = 0; i < nodeCount; i++)
                            {
                                var node = coordArray[i];
                                var strLon = node[0];
                                var strLat = node[1];
                                var lon = Convert.ToDouble(strLon);
                                var lat = Convert.ToDouble(strLat);

                                var thisCoordinate = new Coordinate(lat, lon);
                                double distance = 0.0;
                                if (lastCoordinate != null)
                                {
                                    distance = thisCoordinate.GreatCircleDistance(lastCoordinate);
                                }
                                lastCoordinate = thisCoordinate;
                                totalDistance += distance;

                                var dbShape = new TrolleyTracker.Models.Shape();
                                dbShape.Lat = lat;
                                dbShape.Lon = lon;
                                dbShape.RouteID = routeID;
                                dbShape.Sequence = sequence;
                                dbShape.DistanceTraveled = totalDistance;
                                sequence++;
                                db.Shapes.Add(dbShape);
                            }

                        }
                        db.SaveChanges();

                        var assignStops = new AssignStopsToRoutes();
                        assignStops.UpdateStopsForRoute(db, routeID);
                    }

                }

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return View();
            }
        }

        private void RemoveOldShape(int routeID, TrolleyTrackerContext db)
        {
            // Slow way
            //var oldShapes = from shape in db.Shapes
            //                where shape.RouteID == routeID
            //                select shape;
            //foreach(var shape in oldShapes)
            //{
            //    db.Shapes.Remove(shape);
            //}
            //db.SaveChanges();

            // delete existing records
            db.Database.ExecuteSqlCommand("DELETE FROM Shapes WHERE RouteID = " + routeID);

        }


        //// GET: BulkUploadShapes/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: BulkUploadShapes/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: BulkUploadShapes/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: BulkUploadShapes/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
