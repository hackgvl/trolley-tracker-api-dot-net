using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using TrolleyTracker.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TrolleyTracker.ViewModels
{
    [DataContract(Name = "StopSummary")]
    public class StopSummary
    {
        public StopSummary(Stop stop)
        {
            Construct(stop, null);
        }

        private void Construct(Stop stop, Route route)
        {
            this.ID = stop.ID;
            this.Name = stop.Name;
            this.Description = stop.Description;
            this.Lat = stop.Lat;
            this.Lon = stop.Lon;
            if (route != null)
            {
                var routeStop = stop.RouteStops.FirstOrDefault(rs => (rs.StopID == stop.ID) && (rs.RouteID == route.ID));
                if (routeStop != null)
                {
                    this.ShapeSegmentIndex = routeStop.RouteSegmentIndex;
                }
            }
            if (stop.Picture != null)
            {
                StopImageURL = System.Web.VirtualPathUtility.ToAbsolute("~/") + String.Format("Stops/Picture/{0}", stop.ID);
            }
            NextTrolleyArrivalTime = new Dictionary<int, DateTime>();
        }

        public StopSummary(Stop stop, Route route)
        {
            Construct(stop, route);
        }


        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public double Lat { get; set; }
        [DataMember]
        public double Lon { get; set; }
        [DataMember]
        public int? ShapeSegmentIndex { get; set; }
        [DataMember]
        public string StopImageURL { get; set; }
        [DataMember]
        public Dictionary<int, DateTime> NextTrolleyArrivalTime { get; set; }
    }
}