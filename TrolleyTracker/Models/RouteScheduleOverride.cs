namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class RouteScheduleOverride
    {
        public enum OverrideRule
        {
            NoService,
            Added,
            Replace
        }

        [Key]
        public int ID { get; set; }
        [ForeignKey("NewRoute"), Column(Order = 0)]
        // This can be null for service cancellation
        public int? NewRouteID { get; set; }
        [ForeignKey("OverriddenRoute"), Column(Order = 1)]
        // This can be null if it is to apply to all routes
        public int? OverriddenRouteID { get; set; }
        [DataType(DataType.DateTime)]
        public System.DateTime OverrideDate { get; set; }
        public OverrideRule OverrideType { get; set; }
        [DataType(DataType.Time)]
        public System.DateTime StartTime { get; set; }
        [DataType(DataType.Time)]
        public System.DateTime EndTime { get; set; }
        public virtual Route NewRoute { get; set; }
        public virtual Route OverriddenRoute { get; set; }
                             
    }
}
