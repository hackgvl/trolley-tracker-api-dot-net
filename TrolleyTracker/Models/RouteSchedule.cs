namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RouteSchedule
    {
        public RouteSchedule()
        {

        }

        public RouteSchedule(RouteScheduleOverride routeScheduleOverride)
        {
            if (routeScheduleOverride.NewRouteID.HasValue)
            {
                RouteID = (int)routeScheduleOverride.NewRouteID;
                Route = routeScheduleOverride.NewRoute;
            }
            else
            {
                RouteID = (int)routeScheduleOverride.OverriddenRouteID;
                Route = routeScheduleOverride.OverriddenRoute;
            }
            DayOfWeek = (int)routeScheduleOverride.OverrideDate.DayOfWeek;
            StartTime = routeScheduleOverride.StartTime;
            EndTime = routeScheduleOverride.EndTime;
        }

        public RouteSchedule(RouteSchedule otherSchedule)
        {
            this.ID = otherSchedule.ID;
            this.RouteID = otherSchedule.RouteID;
            this.DayOfWeek = otherSchedule.DayOfWeek;
            this.StartTime = otherSchedule.StartTime;
            this.EndTime = otherSchedule.EndTime;
            this.Route = otherSchedule.Route;
        }


        public int ID { get; set; }
        public int RouteID { get; set; }
        public int DayOfWeek { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
    
        public virtual Route Route { get; set; }
    }
}
