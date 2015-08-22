namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Stop
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Stop()
        {
            this.RouteStops = new HashSet<RouteStop>();
        }
    
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteStop> RouteStops { get; set; }
    }
}
