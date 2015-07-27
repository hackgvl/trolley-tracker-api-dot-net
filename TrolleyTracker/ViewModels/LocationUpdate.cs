using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TrolleyTracker.ViewModels
{
    public class LocationUpdate
    {
        [Required]
        public double Lat { get; set; }

        [Required]
        public double Lon { get; set; }
    }
}