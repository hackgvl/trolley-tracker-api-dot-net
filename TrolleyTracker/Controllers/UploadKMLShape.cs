using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class UploadKMLShapeController : Controller
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // GET: UploadKMLShape
        public ActionResult Index()
        {
            return View();
        }

        // GET: UploadKMLShape/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        private List<SelectListItem> GetRouteSelectList(int? selectedID)
        {
            var routes = from r in db.Routes
                         orderby r.ShortName
                         select r;
            var items = new SelectList(routes, "ID", "ShortName").ToList();

            // Set selected item if specified
            if (selectedID.HasValue)
            {
                var strSelValue = selectedID.ToString();
                foreach(var item in items)
                {
                    if (item.Value == strSelValue) item.Selected = true;
                }
            }

            items.Insert(0, (new SelectListItem { Text = "-- Select Route  --", Value = "" }));
            return items;
        }


        // GET: UploadKMLShape/Create/5
        public ActionResult Create(int? id)
        {
            ViewBag.RouteID = GetRouteSelectList(id);
            return View();
        }


        // POST: UploadKMLShape/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            var resultText = "Unknown error";
            var errorText = "";
            try
            {
                var strRouteID = collection.Get("RouteID");
                int routeID = Convert.ToInt32(strRouteID);
                var route = db.Routes.Find(routeID);

                resultText = "";

                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];

                    var routeData = new ParseKML(file.InputStream);

                    string shapeJSON = new JavaScriptSerializer().Serialize(routeData.RouteShape);

                    ViewData["RouteShapeJSON"] = shapeJSON;

                    List<Stop> newStops = null;
                    List<Stop> oldStops = null;
                    FindNewStops(routeData.RouteStops, ref newStops, ref oldStops);

                    string stopsJSON = new JavaScriptSerializer().Serialize(newStops);
                    ViewData["NewRouteStops"] = stopsJSON;

                    stopsJSON = new JavaScriptSerializer().Serialize(oldStops);
                    ViewData["OldRouteStops"] = stopsJSON;

                    ViewData["RouteName"] = route.ShortName;
                    ViewData["RouteID"] = routeID;

                    return PartialView("RouteShapeConfirm");
                }

                // No file included
                throw new KMLParseException("Missing KML file attachment");

            }
            catch (KMLParseException kmlEx) {
                // This message should be shown to the end user
                errorText = "Oops, problem processing KML file: " + kmlEx.ParseErrorMessage;
                logger.Error(kmlEx, "Exception parsing route shape");
            }
            catch (Exception ex)
            {
                // Unexpected error in parse or preview
                errorText = "Oops - Shape save exception.  This has been logged";
                logger.Error(ex, "Exception processing route shape");
            }

            ViewBag.RouteID =  GetRouteSelectList(null);
            ViewBag.ErrorMessage = errorText;
            ViewBag.Message = resultText;

            return View();

        }

        private void FindNewStops(List<Stop> routeStops, ref List<Stop> newStops, ref List<Stop> oldStops)
        {
            double Epsilon = 20.0; // Max distance in meters for matching stop

            newStops = new List<Stop>();
            oldStops = new List<Stop>();

            var dbStopsList = (from s in db.Stops
                            select s).ToList();

            foreach (var testStop in routeStops)
            {
                var testCoordinate = new Coordinate(testStop.Lat, testStop.Lon);
                bool match = false;
                foreach(var dbStop in dbStopsList)
                {
                    var dbCoordinate = new Coordinate(dbStop.Lat, dbStop.Lon);
                    if (dbCoordinate.GreatCircleDistance(testCoordinate) < Epsilon)
                    {
                        // Stop already exists nearby
                        match = true;
                        break;
                    }
                }
                if (match)
                    oldStops.Add(testStop);
                else
                    newStops.Add(testStop);
            }

        }


        // POST: UploadKMLShape/CreateConfirm
        [CustomAuthorize(Roles = "RouteManagers")]
        [HttpPost]
        public ActionResult CreateConfirm(FormCollection collection)
        {
            var resultText = "Unknown error";
            var errorText = "";
            try
            {
                var strRouteID = collection.Get("RouteID");
                int routeID = Convert.ToInt32(strRouteID);
                var route = db.Routes.Find(routeID);

                resultText = "";

                var jsonShapes = collection.Get("RouteShapeJSON");
                if (jsonShapes != null && strRouteID != null)
                {

                    using (var db = new TrolleyTracker.Models.TrolleyTrackerContext())
                    {
                        RemoveOldShape(routeID, db);

                        SaveNewRouteShape(db, route, jsonShapes);

                        var jsonStops = collection.Get("NewRouteStops");
                        var newStopCount = SaveNewRouteStops(db, jsonStops);

                        var assignStops = new AssignStopsToRoutes();
                        assignStops.UpdateStopsForRoute(db, routeID);

                        logger.Info($"Modified route shape of '{route.ShortName}' - '{route.LongName}, added {newStopCount} stops.'");
                        resultText = $"Route path for {route.ShortName} Saved, added {newStopCount} stops.";
                    }
                }

            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        logger.Info("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
                errorText = "Oops - Shape save exception.  This has been logged";
                logger.Error(dbEx, "Exception processing route shape");
            }
            catch (Exception ex)
            {
                // Unexpected error in parse or preview
                errorText = "Oops - Shape save exception.  This has been logged";
                logger.Error(ex, "Exception processing route shape");
            }

            ViewBag.RouteID = GetRouteSelectList(null);
            ViewBag.ErrorMessage = errorText;
            ViewBag.Message = resultText;

            return View("Create");

        }


        private int SaveNewRouteStops(TrolleyTrackerContext db, string jsonStops)
        {

            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

            dynamic stopData = jss.Deserialize(jsonStops, typeof(object)) as dynamic;

            int stopCount = stopData.Length;
            for (int i = 0; i < stopCount; i++)
            {
                var importStop = stopData[i];

                var newStop = new Stop();
                newStop.Lat = Convert.ToDouble(importStop.Lat);
                newStop.Lon = Convert.ToDouble(importStop.Lon);
                newStop.Name = importStop.Name;
                newStop.Description = importStop.Name;
                newStop.RouteStops = new List<RouteStop>();
                db.Stops.Add(newStop);
            }

            db.SaveChanges();

            return stopCount;
        }

        /// <summary>
        /// Parse JSON serialized route coordinate array
        /// </summary>
        /// <param name="db"></param>
        /// <param name="route"></param>
        /// <param name="jsonShapes"></param>
        private void SaveNewRouteShape(TrolleyTrackerContext db, Route route, string jsonShapes)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

            dynamic shapeData = jss.Deserialize(jsonShapes, typeof(object)) as dynamic;

            Coordinate lastCoordinate = null;
            var totalDistance = 0.0;
            int sequence = 0;
            int nodeCount = shapeData.Length;
            for (int i = 0; i < nodeCount; i++)
            {
                var node = shapeData[i];
                var lon = Convert.ToDouble(node.Lon);
                var lat = Convert.ToDouble(node.Lat);

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
                dbShape.RouteID = route.ID;
                dbShape.Sequence = sequence;
                dbShape.DistanceTraveled = totalDistance;
                sequence++;
                db.Shapes.Add(dbShape);
            }

            db.SaveChanges();


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


    }
}
