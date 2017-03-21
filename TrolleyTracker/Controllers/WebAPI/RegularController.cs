using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Models;
using Newtonsoft.Json;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json.Serialization;

namespace TrolleyTracker.Controllers.WebAPI
{
    /// <summary>
    /// List of stops served by one or more fixed routes
    /// Mostly used to provide source of Public Map Layers reference data
    /// </summary>
    public class RegularController : ApiController
    {

        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: Stops/Regular
        // Mapped as - GET: api/v1/Stops/Regular
        public List<GeoJSONSummary> Get()
        {

            var stops = (from stop in db.Stops
                         from routeSchedule in db.RouteSchedules
                         from routeStop in db.RouteStops
                         where (routeSchedule.Route.ID == routeStop.RouteID &&
                             routeStop.StopID == stop.ID)
                         //select stop);
                         select stop).Select(stop => stop).ToList().Distinct(new StopComparer()).OrderBy(stop => stop.Name);

            var geoJSONStopsList = new List<GeoJSONSummary>();
            foreach (var stop in stops)
            {
                geoJSONStopsList.Add(new GeoJSONSummary(stop));
            }

            //var serializedData = JsonConvert.SerializeObject(geoJSONStopsList, Formatting.Indented);  // Starting with GEOJSON objects yields same result

            return geoJSONStopsList;
        }
    }

    public class StopComparer : IEqualityComparer<Stop>
    {
        #region IEqualityComparer<Village> Members
        bool IEqualityComparer<Stop>.Equals(Stop x, Stop y)
        {
            // Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y))
                return true;

            // Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.ID == y.ID;
        }

        int IEqualityComparer<Stop>.GetHashCode(Stop obj)
        {
            return obj.GetHashCode();
        }
        #endregion
    }


    public class GeoJSONSummary
    {
        public string Name { get; set; }

        public GeographicPosition Location { get; set; }

        public GeoJSONSummary(Stop stop)
        {
            this.Name = stop.Name;
            this.Location = new GeographicPosition(stop.Lat, stop.Lon);
        }
    }

}