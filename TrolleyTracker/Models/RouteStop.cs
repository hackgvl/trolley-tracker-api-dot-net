namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RouteStop
    {
        public int ID { get; set; }
        public int RouteID { get; set; }
        public int StopID { get; set; }
        public int StopSequence { get; set; }
    
        public virtual Route Route { get; set; }
        public virtual Stop Stop { get; set; }
    }
}
