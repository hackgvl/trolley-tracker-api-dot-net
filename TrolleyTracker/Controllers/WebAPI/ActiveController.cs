using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers.WebAPI
{
    public class ActiveController : ApiController
    {
        private TrolleyTrackerContext db = new TrolleyTrackerContext();


        // Mapped as - GET: api/Routes/Active
        public List<RouteSummary> Get()
        {

            // Azure server instances run with DateTime.Now set to UTC
            var currentDateTime = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);

            var activeRoutes = new List<RouteSummary>();
            var weekday = (int)currentDateTime.DayOfWeek;

            // Note: ToList() to avoid "There is already an open DataReader associated with this Command which must be closed first." exception,
            // even though MultipleActiveResultSets is already true in the connection string.
            var todaysRouteSchedules = (from route in db.Routes
                                from routeSchedule in db.RouteSchedules
                                orderby routeSchedule.StartTime
                                where (routeSchedule.RouteID == route.ID) && (routeSchedule.DayOfWeek == weekday)
                                select routeSchedule).ToList<RouteSchedule>();

            // Return active routes 5 minutes early so that progress from garage to starting point
            // is shown, also if trolley is a few minutes early.

            var startTimeRef = currentDateTime.Add(new TimeSpan(0, 5, 0)).TimeOfDay;
            var endTimeRef = currentDateTime.TimeOfDay;
            foreach (var routeSchedule in todaysRouteSchedules)
            {
                if ((startTimeRef > routeSchedule.StartTime.TimeOfDay) && (endTimeRef < routeSchedule.EndTime.TimeOfDay))
                {
                    activeRoutes.Add(new RouteSummary(routeSchedule.Route));
                }
            }
            return activeRoutes;
        }


    }
}
