namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Shape
    {
        public int ID { get; set; }
        public int RouteID { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int Sequence { get; set; }
        public double DistanceTraveled { get; set; }
    
        public virtual Route Route { get; set; }
    }
}
