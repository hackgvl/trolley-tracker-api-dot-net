using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Controllers;

namespace TrolleyTracker.Models
{
    public class StopArrivalTime
    {
        private static readonly object _lock = new object();

        private static Dictionary<int, Coordinate> lastReportedLocation;

        private const double MinMoveDistance = 5.0; // Meters
        private const double StopRadiusCheck = 30.0; // Meters

        private static Dictionary<int, StopSummary> stopSummaries;  // Key is Stop.ID
        private static DateTime lastStoplistUpdateTime;
        private const double TimeBetweenStopUpdates = 3600.0; // Seconds


        private static Dictionary<int, int> lastStopIDByTrolley; // Key is trolley Number
        private static TrolleyTrackerContext db = new TrolleyTrackerContext();

        public static void Initialize()
        {
            lastReportedLocation = new Dictionary<int, Coordinate>();
            lastStopIDByTrolley = new Dictionary<int, int>();
            UpdateStopList();
        }

        public static List<StopSummary> StopSummaryListWithArrivalTimes
        {
            get
            {

                lock (_lock)
                {
                    var stopSummaryList = new List<StopSummary>();
                    foreach(var stopSummary in stopSummaries.Values)
                    {
                        stopSummaryList.Add(stopSummary);
                    }
                    return stopSummaryList;
                }
            }
        }

        public static StopSummary GetStopSummaryWithArrivalTimes(int stopId)
        {

            lock (_lock)
            {
                if (!stopSummaries.ContainsKey(stopId))
                {
                    return null;
                }
                return stopSummaries[stopId];
            }

        }

        /// <summary>
        /// Get local stop list structure.   Stops don't change frequently, so this needs to be updated only
        /// once per hour or so
        /// </summary>
        private static void UpdateStopList()
        {
            if ((DateTime.Now - lastStoplistUpdateTime).TotalSeconds < TimeBetweenStopUpdates)
            {
                return;
            }

            var oldStopSummaryList = stopSummaries;
            var stops = db.Stops.ToList<Stop>();
            stopSummaries = new Dictionary<int, StopSummary>();

            foreach(var stop in stops)
            {
                stopSummaries.Add(stop.ID, new StopSummary(stop));
            }

            // Transfer any stop times from old list to new list
            if (oldStopSummaryList != null)
            {
                foreach (var summaryStop in stopSummaries)
                {
                    if (oldStopSummaryList.ContainsKey(summaryStop.Key))
                    {
                        summaryStop.Value.LastTrolleyArrivalTime = oldStopSummaryList[summaryStop.Key].LastTrolleyArrivalTime;
                    }
                }
            }
            lastStoplistUpdateTime = DateTime.Now;
        }

        /// <summary>
        /// Check and record time seen at each stop
        /// </summary>
        /// <param name="trolley"></param>
        public static void UpdateTrolleyStopArrivalTime(Trolley trolley)
        {
            if ((trolley.CurrentLat == null) || (trolley.CurrentLon == null))
            {
                return;
            }

            lock (_lock)
            {
                if (!lastReportedLocation.ContainsKey(trolley.Number))
                {
                        lastReportedLocation.Add(trolley.Number, new Coordinate((double)trolley.CurrentLat, (double)trolley.CurrentLon));

                }
                else
                {
                    UpdateStopTime(trolley);
                }


            }

        }

        private static void UpdateStopTime(Trolley trolley)
        {
            UpdateStopList();

            // See if trolley has moved far enough to justify a full stop distance check
            var currentLocation = new Coordinate((double)trolley.CurrentLat, (double)trolley.CurrentLon);
            if (currentLocation.GreatCircleDistance(lastReportedLocation[trolley.Number]) > MinMoveDistance)
            {
                CheckForStopInRange(trolley, currentLocation);
                lastReportedLocation[trolley.Number] = currentLocation; 
            }

        }

        /// <summary>
        /// Check for stop proximity - try to minimize number of polled stops
        ///   - First preference is the previous stop
        ///    Then loop through all stops
        /// </summary>
        /// <param name="trolley"></param>
        /// <param name="currentLocation"></param>
        private static void CheckForStopInRange(Trolley trolley, Coordinate currentLocation)
        {
            //var lastStopID = -1;
            if (lastStopIDByTrolley.ContainsKey(trolley.Number) )
            {
                var stopID = lastStopIDByTrolley[trolley.Number];
                var stopSummary = stopSummaries[stopID];
                if (currentLocation.GreatCircleDistance(new Coordinate(stopSummary.Lat, stopSummary.Lon)) < StopRadiusCheck) {
                    // Still within stop zone
                    return;
                }
            }

            foreach(var stopID in stopSummaries.Keys)
            {
                var stopSummary = stopSummaries[stopID];
                if (currentLocation.GreatCircleDistance(new Coordinate(stopSummary.Lat, stopSummary.Lon)) < StopRadiusCheck)
                {
                    // Found new stop zone
                    if (stopSummary.LastTrolleyArrivalTime.ContainsKey(trolley.Number))
                    {
                        stopSummary.LastTrolleyArrivalTime[trolley.Number] = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);
                    }
                    else
                    {
                        stopSummary.LastTrolleyArrivalTime.Add(trolley.Number, UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow));
                    }
                    lastStopIDByTrolley.Add(trolley.Number, stopSummary.ID);
                    return;
                }

            }

            // Not currently in any stop zone
            if (lastStopIDByTrolley.ContainsKey(trolley.Number))
            {
                lastStopIDByTrolley.Remove(trolley.Number);
            }

        }
    }
}