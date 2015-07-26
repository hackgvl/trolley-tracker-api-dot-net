using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using TrolleyTracker.Models;

namespace TrolleyTracker.ViewModels
{
    [DataContract(Name = "RouteSummary")]
    public class RouteSummary
    {
        public RouteSummary(Route route)
        {
            this.ID = route.ID;
            this.ShortName = route.ShortName;
            this.LongName = route.LongName;
            this.Description = route.Description;
            this.FlagStopsOnly = route.FlagStopsOnly;
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
    }
}


