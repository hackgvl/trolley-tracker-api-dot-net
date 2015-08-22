namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Route
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Route()
        {
            this.RouteStops = new HashSet<RouteStop>();
            this.Shapes = new HashSet<Shape>();
            this.RouteSchedules = new HashSet<RouteSchedule>();
        }
    
        public int ID { get; set; }
        public string ShortName { get; set; }
        public string LongName { get; set; }
        public string Description { get; set; }
        public bool FlagStopsOnly { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteStop> RouteStops { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Shape> Shapes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteSchedule> RouteSchedules { get; set; }
    }
}
