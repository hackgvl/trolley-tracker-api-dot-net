using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using TrolleyTracker.Models;

namespace TrolleyTracker.ViewModels
{
    [DataContract(Name = "RouteDetail")]
    public class RouteDetail
    {
        public RouteDetail(Route route)
        {
            this.ID = route.ID;
            this.ShortName = route.ShortName;
            this.LongName = route.LongName;
            this.Description = route.Description;
            this.FlagStopsOnly = route.FlagStopsOnly;

            this.RoutePath = new List<Location>();
            this.Stops = new List<StopSummary>();    
        }


        [DataMember(Name = "ID")]
        public int ID { get; set; }

        [DataMember(Name = "ShortName")]
        public string ShortName { get; set; }
        [DataMember(Name = "LongName")]
        public string LongName { get; set; }
        [DataMember(Name = "Description")]
        public string Description { get; set; }
        [DataMember(Name = "FlagStopsOnly")]
        public bool FlagStopsOnly { get; set; }

        [DataMember(Name = "Stops")]
        public List<StopSummary> Stops  { get; set; }

        [DataMember(Name = "RouteShape")]
        public List<Location> RoutePath { get; set; }
    }
}


