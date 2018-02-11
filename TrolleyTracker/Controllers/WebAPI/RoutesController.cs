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

                routeDetail.AddRouteDetail(db, route);

                return Ok(routeDetail);
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