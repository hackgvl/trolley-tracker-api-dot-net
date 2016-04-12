using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using TrolleyTracker.Models;


namespace TrolleyTracker.ViewModels
{
    [DataContract(Name = "RouteScheduleSummary")]
    public class RouteScheduleSummary
    {
        private List<string> daysOfWeek = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };


        public RouteScheduleSummary(RouteSchedule routeSchedule)
        {
            this.ID = routeSchedule.ID;
            this.RouteID = routeSchedule.RouteID;
            this.DayOfWeek = daysOfWeek[routeSchedule.DayOfWeek % 7];
            this.StartTime = routeSchedule.StartTime.ToShortTimeString();
            this.EndTime = routeSchedule.EndTime.ToShortTimeString();
            this.RouteLongName = routeSchedule.Route.LongName;
            this.RouteShortName = routeSchedule.Route.ShortName;
        }

        public RouteScheduleSummary(RouteScheduleOverride routeScheduleOverride)
        {
            this.ID = routeScheduleOverride.ID;
            if (routeScheduleOverride.NewRouteID.HasValue)
            {
                this.RouteID = (int)routeScheduleOverride.NewRouteID;
                this.RouteLongName = routeScheduleOverride.NewRoute.LongName;
                this.RouteShortName = routeScheduleOverride.NewRoute.ShortName;
            }
            else
            {
                this.RouteID = (int)routeScheduleOverride.OverriddenRouteID;
                this.RouteLongName = routeScheduleOverride.OverriddenRoute.LongName;
                this.RouteShortName = routeScheduleOverride.OverriddenRoute.ShortName;
            }
            this.DayOfWeek = daysOfWeek[(int)routeScheduleOverride.OverrideDate.DayOfWeek];
            this.StartTime = routeScheduleOverride.StartTime.ToShortTimeString();
            this.EndTime = routeScheduleOverride.EndTime.ToShortTimeString();
        }

        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public int RouteID { get; set; }
        [DataMember]
        public string DayOfWeek { get; set; }
        [DataMember]
        public string StartTime { get; set; }
        [DataMember]
        public string EndTime { get; set; }
        [DataMember]
        public string RouteLongName { get; set; }
        public string RouteShortName { get; set; }
    }
}