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
            var currentDateTime = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);

            var fixedRouteSchedules = (from route in db.Routes
                                             from routeSchedule in db.RouteSchedules
                                             orderby routeSchedule.StartTime
                                             where (routeSchedule.RouteID == route.ID) 
                                             select routeSchedule).ToList<RouteSchedule>();

            var today = currentDateTime.Date;
            var routeScheduleOverrideList = (from rso in db.RouteScheduleOverrides
                                             orderby rso.OverrideDate, rso.StartTime, rso.NewRoute.ShortName
                                             select rso).ToList<RouteScheduleOverride>();

            var routeSchedules = BuildScheduleView.BuildEffectiveRouteSchedule(currentDateTime, 7, fixedRouteSchedules, routeScheduleOverrideList);

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
