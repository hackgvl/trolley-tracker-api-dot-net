using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;

namespace TrolleyTracker.Controllers.WebAPI
{
    public class RouteSchedulesController : ApiController
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();


        // GET: api/RouteSchedules
        public List<RouteScheduleSummary> Get()
        {
            var routeSchedules = from rs in db.RouteSchedules.Include(rs => rs.Route)
                                 orderby rs.DayOfWeek, rs.StartTime ascending
                                 select rs;
            var schedules = new List<RouteScheduleSummary>();
            foreach(var routeSchedule in routeSchedules)
            {
                schedules.Add(new RouteScheduleSummary(routeSchedule));
            }
            return schedules;
        }


        // GET: api/RouteSchedules/5
        [ResponseType(typeof(RouteScheduleSummary))]
        public IHttpActionResult GetRouteSchedule(int id)
        {
            RouteSchedule routeSchedule = db.RouteSchedules.Find(id);
            if (routeSchedule == null)
            {
                return NotFound();
            }

            var routeScheduleSummary = new RouteScheduleSummary(routeSchedule);

            return Ok(routeScheduleSummary);
        }



    }
}
