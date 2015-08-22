namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RouteSchedule
    {
        public int ID { get; set; }
        public int RouteID { get; set; }
        public int DayOfWeek { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
    
        public virtual Route Route { get; set; }
    }
}
