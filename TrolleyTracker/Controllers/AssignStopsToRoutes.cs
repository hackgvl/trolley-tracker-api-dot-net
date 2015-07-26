using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class AssignStopsToRoutes
    {

        const double MinStopProximity = 20.0;  // Meters

        /// <summary>
        /// Recalculate which routes the stops belong to, and the order.
        /// Call after any change to route or stops
        /// </summary>
        /// <param name="routeID">Route being recalculated</param>
        /// <param name="shapePoints"></param>
        /// <param name="stopPoints"></param>
        public void UpdateRouteStops(TrolleyTrackerEntities db, int routeID, List<Coordinate> shapePoints, List<Stop> stops)
        {

            RemovePreviousRouteStops(db, routeID);

            var route = (from Route in db.Routes
                         where Route.ID == routeID
                         select Route).FirstOrDefault<Route>();

            // A stop is considered to belong to a route if it's within MinStopProximity meters of the route path
            var routeStopList = new List<Stop>();

            if (route.FlagStopsOnly)
            {
                // No stops to generate
                return; 
            }

            for (int i=1; i< shapePoints.Count; i++)
            {
                for (int s=0; s< stops.Count; s++)
                {
                    var stop = stops[s];
                    var stopPosition = new Coordinate(0, stop.Lat, stop.Lon, null);
                    Coordinate closest = null;
                    var distance = FindDistanceToSegment(stopPosition, shapePoints[i], shapePoints[i - 1], out closest);
                    if (distance < MinStopProximity)
                    {
                        if (!routeStopList.Contains(stop))
                        {
                            routeStopList.Add(stop);
                        }
                        //break;   //  Bug? How to handle case of ordering multiple stops in a long straight segment
                    }
                }
            }


            for (int i=0; i < routeStopList.Count; i++)
            {
                var newRouteStop = db.RouteStops.Create();
                newRouteStop.RouteID = routeID;
                newRouteStop.StopID = routeStopList[i].ID;
                newRouteStop.StopSequence = i;
                db.RouteStops.Add(newRouteStop);
            }
            db.SaveChanges();

        }


        public void UpdateStopsForRoute(TrolleyTrackerEntities db, int routeID)
        {

            var shapes = (from Shape in db.Shapes
                         where Shape.RouteID == routeID
                         orderby Shape.Sequence
                              select Shape).ToList<Shape>();

            var shapePoints = new List<Coordinate>();
            foreach (var shape in shapes)
            {
                var coord = new Coordinate(0, shape.Lat, shape.Lon, null);
                shapePoints.Add(coord);
            }

            var stops = (from Stop s in db.Stops
                        select s).ToList<Stop>();


            UpdateRouteStops(db, routeID, shapePoints, stops);

        }

        public void UpdateStopsForAllRoutes()
        {
            using (var db = new TrolleyTracker.Models.TrolleyTrackerEntities())
            {
                var routes = from Route in db.Routes
                             select Route;
                foreach (var route in routes)
                {
                    UpdateStopsForRoute(db, route.ID);
                }
            }

        }



        private void RemovePreviousRouteStops(TrolleyTrackerEntities db, int routeID)
        {
            var routeStops = from RouteStop rs in db.RouteStops
                              where rs.RouteID == routeID
                              select rs;

            foreach (var rs in routeStops)
            {
                db.RouteStops.Remove(rs);
            }
            db.SaveChanges();

        }




        // Calculate the distance between
        // Node 'stopPosition' and the segment p1 --> p2, where p1 -> p2 is a subset of a way.
        private double FindDistanceToSegment(Coordinate stopPosition, Coordinate p1, Coordinate p2, out Coordinate closest)
        {
            double dx = p2.Lon - p1.Lon;
            double dy = p2.Lat - p1.Lat;
            if ((dx == 0) && (dy == 0))
            {
                // It's a point not a line segment.
                closest = p1;
                dx = stopPosition.Lon - p1.Lon;
                dy = stopPosition.Lat - p1.Lat;

                //return Math.Sqrt(dx * dx + dy * dy);
                return stopPosition.GreatCircleDistance(closest);
            }

            // Calculate the t that minimizes the distance.
            double t = ((stopPosition.Lon - p1.Lon) * dx + (stopPosition.Lat - p1.Lat) * dy) / (dx * dx + dy * dy);

            Dictionary<string, string> tagList = null;
            // See if this represents one of the segment's
            // end points or a point in the middle.
            if (t < 0)
            {
                closest = new Coordinate(-1, p1.Lat, p1.Lon, tagList);
                dx = stopPosition.Lon - p1.Lon;
                dy = stopPosition.Lat - p1.Lat;
            }
            else if (t > 1)
            {
                closest = new Coordinate(-1, p2.Lat, p2.Lon, tagList);
                dx = stopPosition.Lon - p2.Lon;
                dy = stopPosition.Lat - p2.Lat;
            }
            else
            {
                closest = new Coordinate(-1, p1.Lat + t * dy, p1.Lon + t * dx, tagList);
                dx = stopPosition.Lon - closest.Lon;
                dy = stopPosition.Lat - closest.Lat;
            }

            // For linear geometry return Math.Sqrt(dx * dx + dy * dy);

            // For earth geography below
            return stopPosition.GreatCircleDistance(closest);
        }

    }
}