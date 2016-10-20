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
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: api/Routes
        public List<RouteSummary> GetRoutes()
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

        // GET: api/Routes/5
        [ResponseType(typeof(RouteDetail))]
        public IHttpActionResult GetRoute(int id)
        {
            Route route = db.Routes.Find(id);
            if (route == null)
            {
                return NotFound();
            }

            // Assemble route + Stops + Shape
            var routeDetail = new RouteDetail(route);

            AddRouteDetail(routeDetail, route);

            return Ok(routeDetail);
        }

        private void AddRouteDetail(RouteDetail routeDetail, Route route)
        {

            var stops = from stop in db.Stops
                        from routeStop in db.RouteStops
                        orderby routeStop.StopSequence
                        where (routeStop.StopID == stop.ID) && (routeStop.RouteID == route.ID)
                        select stop;

            foreach (var stop in stops)
            {
                // Use arrival times if available
                var stopWithArrivalTime = StopArrivalTime.GetStopSummaryWithArrivalTimes(stop.ID);
                if (stopWithArrivalTime != null)
                {
                    routeDetail.Stops.Add(stopWithArrivalTime);
                }
                else
                {
                    routeDetail.Stops.Add(new StopSummary(stop));
                }
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
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RouteExists(int id)
        {
            return db.Routes.Count(e => e.ID == id) > 0;
        }
    }
}