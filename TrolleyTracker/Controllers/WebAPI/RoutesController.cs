using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;

namespace TrolleyTracker.Controllers.WebAPI
{
    public class RoutesController : ApiController
    {
        // GET: api/Routes
        public List<RouteSummary> GetRoutes()
        {
            using (var db = new TrolleyTracker.Models.TrolleyTrackerContext())
            {
                var routes = db.Routes;
                var summaryRoutes = new List<RouteSummary>();
                foreach (var route in routes)
                {
                    var summaryRoute = new RouteSummary(route);
                    summaryRoutes.Add(summaryRoute);
                }
                return summaryRoutes;
            }
        }

        // GET: api/Routes/5
        [ResponseType(typeof(RouteDetail))]
        public IHttpActionResult GetRoute(int id)
        {
            using (var db = new TrolleyTracker.Models.TrolleyTrackerContext())
            {
                Route route = db.Routes.Find(id);
                if (route == null)
                {
                    return NotFound();
                }

                // Assemble route + Stops + Shape
                var routeDetail = new RouteDetail(route);

                AddRouteDetail(db, routeDetail, route);

                return Ok(routeDetail);
            }
        }

        private void AddRouteDetail(TrolleyTrackerContext db, RouteDetail routeDetail, Route route)
        {

            var stops = (from stop in db.Stops
                        from routeStop in db.RouteStops
                        orderby routeStop.StopSequence
                        where (routeStop.StopID == stop.ID) && (routeStop.RouteID == route.ID)
                        select stop).ToList();

            foreach (var stop in stops)
            {
                // Construct with route info so route shape segment index is included
                var stopSummary = new StopSummary(stop, route);

                // Use arrival times if available
                var stopWithArrivalTime = StopArrivalTime.GetStopSummaryWithArrivalTimes(stop.ID);
                if (stopWithArrivalTime != null)
                {
                    stopSummary.NextTrolleyArrivalTime = stopWithArrivalTime.NextTrolleyArrivalTime;
                }
                routeDetail.Stops.Add(stopSummary);
            }

            var shapes = from shape in db.Shapes
                        orderby shape.Sequence
                        where (shape.RouteID == route.ID)
                        select shape;

            foreach (var shape in shapes)
            {
                var coordinate = new Location();
                coordinate.Lat = shape.Lat;
                coordinate.Lon = shape.Lon;
                routeDetail.RoutePath.Add(coordinate);
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

        //private bool RouteExists(int id)
        //{
        //    return db.Routes.Count(e => e.ID == id) > 0;
        //}
    }
}