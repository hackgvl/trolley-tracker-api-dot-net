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
            this.DayOfWeek = daysOfWeek[routeSchedule.DayOfWeek];
            this.StartTime = routeSchedule.StartTime.ToShortTimeString();
            this.EndTime = routeSchedule.EndTime.ToShortTimeString();
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
    }
}