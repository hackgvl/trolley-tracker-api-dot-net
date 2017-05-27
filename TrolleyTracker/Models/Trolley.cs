namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Trolley
    {
        public int ID { get; set; }
        public string TrolleyName { get; set; }
        public int Number { get; set; }
        public Nullable<double> CurrentLat { get; set; }
        public Nullable<double> CurrentLon { get; set; }
        public Nullable<DateTime> LastBeaconTime { get; set; }
        [Required]
        [RegularExpression(@"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")]
        [StringLength(9, ErrorMessage = "The Trolley Icon Color value cannot exceed 9 characters. ")]
        public string IconColorRGB { get; set; }
    } 
}
