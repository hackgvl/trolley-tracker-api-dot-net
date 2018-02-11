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
            this.RouteColorRGB = route.RouteColorRGB;

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
        [DataMember(Name = "RouteColorRGB")]
        public string RouteColorRGB { get; set; }


        public void AddRouteDetail(TrolleyTrackerContext db, Route route)
        {

            var stops = (from stop in db.Stops
                         from routeStop in db.RouteStops
                         orderby routeStop.StopSequence
                         where (routeStop.StopID == stop.ID) && (routeStop.RouteID == route.ID)
                         select stop).ToList();

            foreach (var stop in stops)
            {
                // Construct with route info so route shape segment index is included
                var stopSummary = new StopSummary(stop, route);

                // Use arrival times if available
                var stopWithArrivalTime = StopArrivalTime.GetStopSummaryWithArrivalTimes(stop.ID);
                if (stopWithArrivalTime != null)
                {
                    stopSummary.NextTrolleyArrivalTime = stopWithArrivalTime.NextTrolleyArrivalTime;
                }
                this.Stops.Add(stopSummary);
            }

            var shapes = from shape in db.Shapes
                         orderby shape.Sequence
                         where (shape.RouteID == route.ID)
                         select shape;

            foreach (var shape in shapes)
            {
                var coordinate = new Location();
                coordinate.Lat = shape.Lat;
                coordinate.Lon = shape.Lon;
                this.RoutePath.Add(coordinate);
            }
        }

    }
}


