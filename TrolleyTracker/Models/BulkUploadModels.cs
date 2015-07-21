using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace TrolleyTracker.Models
{
    public class BulkStopsViewModel
    {
        [DataType(DataType.MultilineText)]
        public string JSONText { get; set; }
    }

    public class BulkShapesViewModel
    {

        public int RouteID { get; set; }

        [DataType(DataType.MultilineText)]
        public string JSONText { get; set; }

        public virtual Route Route { get; set; }
    }

}