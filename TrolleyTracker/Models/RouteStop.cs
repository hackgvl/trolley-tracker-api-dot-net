namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class RouteStop
    {
        public int ID { get; set; }
        public int RouteID { get; set; }
        public int StopID { get; set; }
        public int StopSequence { get; set; }
        /// <summary>
        /// Index into the shape segment end for this stop - the
        /// stop is located within the segment from this index to (previous index % #ofShapePoints)
        /// </summary>
        public int RouteSegmentIndex { get; set; }
        /// <summary>
        /// Average travel time to next stop on this route.
        /// Zero value means never set.
        /// </summary>
        [Required]
        public int AverageTravelTimeToNextStop { get; set; }
        /// <summary>
        /// Most recent time at this stop on this route.  
        /// Null value means that it was never set.
        /// </summary>
        public DateTime? LastTimeAtStop { get; set; }

        public virtual Route Route { get; set; }
        public virtual Stop Stop { get; set; }
    }
}
