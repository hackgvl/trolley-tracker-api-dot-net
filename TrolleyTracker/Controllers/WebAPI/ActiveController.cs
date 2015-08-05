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
        private TrolleyTrackerEntities db = new TrolleyTrackerEntities();


        // Mapped as - GET: api/Routes/Active
        public List<RouteSummary> Get()
        {


            var activeRoutes = new List<RouteSummary>();
            var weekday = (int)DateTime.Now.DayOfWeek;

            // Note: ToList() to avoid "There is already an open DataReader associated with this Command which must be closed first." exception,
            // even though MultipleActiveResultSets is already true in the connection string.
            var todaysRouteSchedules = (from route in db.Routes
                                from routeSchedule in db.RouteSchedules
                                orderby routeSchedule.StartTime
                                where (routeSchedule.RouteID == route.ID) && (routeSchedule.DayOfWeek == weekday)
                                select routeSchedule).ToList<RouteSchedule>();  

            // Return active routes 15 minutes early so that progress from garage to starting point
            // is shown, also if trolley is a few minutes early.
            var startTimeRef = DateTime.Now.Add(new TimeSpan(0, 15, 0)).TimeOfDay;
            foreach (var routeSchedule in todaysRouteSchedules)
            {
                if ( (startTimeRef > routeSchedule.StartTime.TimeOfDay) && (DateTime.Now.TimeOfDay < routeSchedule.EndTime.TimeOfDay) )
                {
                    activeRoutes.Add(new RouteSummary(routeSchedule.Route));
                }
            }
            return activeRoutes;
        }


    }
}
