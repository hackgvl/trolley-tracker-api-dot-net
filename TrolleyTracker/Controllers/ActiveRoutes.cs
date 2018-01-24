using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class ActiveRoutes
    {

        public static List<RouteSummary> GetActiveRoutes()
        {
            // Azure server instances run with DateTime.Now set to UTC
            var currentDateTime = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);

            var activeRoutes = new List<RouteSummary>();
            var weekday = (int)currentDateTime.DayOfWeek;


            List<RouteSchedule> todaysFixedRouteSchedules = null;
            List<RouteScheduleOverride> routeScheduleOverrideList = null;
            using (var db = new TrolleyTrackerContext())
            {
                // Note: ToList() to avoid "There is already an open DataReader associated with this Command which must be closed first." exception,
                // even though MultipleActiveResultSets is already true in the connection string.
                todaysFixedRouteSchedules = (from rs in db.RouteSchedules.Include(r => r.Route)
                                                 orderby rs.StartTime
                                                 where rs.DayOfWeek == weekday
                                                 select rs).ToList();

                var today = currentDateTime.Date;
                routeScheduleOverrideList = (from rso in db.RouteScheduleOverrides.Include(rso => rso.NewRoute).Include(rso => rso.OverriddenRoute)
                                          orderby rso.OverrideDate, rso.StartTime, rso.NewRoute.ShortName
                                             where rso.OverrideDate == today
                                             select rso).ToList();
            }

            var scheduleToDate = new Dictionary<RouteSchedule, DateTime>();
            var todaysRouteSchedules = BuildScheduleView.BuildEffectiveRouteSchedule(currentDateTime, 1, todaysFixedRouteSchedules, scheduleToDate, routeScheduleOverrideList);

            // Get today's effective routes
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