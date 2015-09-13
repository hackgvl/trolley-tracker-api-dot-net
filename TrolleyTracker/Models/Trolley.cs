namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Trolley
    {
        public int ID { get; set; }
        public string TrolleyName { get; set; }
        public int Number { get; set; }
        public Nullable<double> CurrentLat { get; set; }
        public Nullable<double> CurrentLon { get; set; }
        public Nullable<DateTime> LastBeaconTime { get; set; }
    }
}
