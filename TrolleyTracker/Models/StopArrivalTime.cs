using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Data.Entity;
using System.Web;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Controllers;

namespace TrolleyTracker.Models
{
    public class StopArrivalTime
    {
        private static readonly object _lock = new object();

        private const double MinMoveDistance = 4.0; // Meters
        private const double StopRadiusCheck = 25.0; // Meters
        private const int MaxTimeBetweenStops = 15 * 60; // Seconds

        private static Dictionary<int, StopSummary> stopSummaries;  // Key is Stop.ID
        private const double TimeBetweenStopUpdates = 3600.0; // Seconds

        private static DateTime lastDataUpdateTime = DateTime.Now.AddMinutes(-60);  // So first pass will update
        private static TrolleyTrackerContext db = new TrolleyTrackerContext();

        private static Dictionary<int, Trolley> runningTrolleys; // Full record of running trolleys by trolley number
        private static Dictionary<int, Route> activeRoutes;  // Active routes by route ID
        private static Dictionary<int, TrolleyTrackingInfo> trolleyTrackingInfo;  // Key is trolley ID


        public static void Initialize()
        {
            runningTrolleys = new Dictionary<int, Trolley>();
            activeRoutes = new Dictionary<int, Route>();
            trolleyTrackingInfo = new Dictionary<int, TrolleyTrackingInfo>();

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

                RefreshStaticData();
                if (runningTrolleys.Count == 0)
                {
                    trolleyTrackingInfo.Clear();
                    return; // Nothing on the schedule
                }

                if (!trolleyTrackingInfo.ContainsKey(trolley.ID))
                {
                    CreateTrackingInfo(trolley);
                    MatchTrolleyToRoute(trolley);
                }
                var trackingInfo = trolleyTrackingInfo[trolley.ID];
                trackingInfo.CurrentTrolley = trolley;

                UpdateStopTime(trackingInfo);

            }

        }

        private static void CreateTrackingInfo(Trolley trolley)
        {
            var newTrackingInfo = new TrolleyTrackingInfo(trolley);
            if (activeRoutes.Count == 1)
            {
                // For a single active route, all trolleys follow that route
                newTrackingInfo.CurrentRoute = activeRoutes.First().Value;
            }
            trolleyTrackingInfo.Add(trolley.ID, newTrackingInfo);
        }

        /// <summary>
        /// Update data periodically that doesn't change often such as stops, trolleys, etc.
        /// </summary>
        private static void RefreshStaticData()
        {
            if (TimeToUpdate())
            {
                var newRunningTrolleys = TrolleyCache.GetRunningTrolleys(false);
                SetNewRunningTrolleys(newRunningTrolleys);   // Get full trolley records, keyed by Number
                if (newRunningTrolleys.Count > 0)  // Check that schedule is active
                {
                    // Schedule is active; update remaining items
                    lastDataUpdateTime = DateTime.Now;

                    UpdateActiveRoutes();
                    UpdateStopList();

                }
            }

        }

        /// <summary>
        /// Assign trollies to route, for now based on color match.
        /// If there is only 1 active route, assign all trollies to that route, regardless of color
        /// </summary>
        private static void MatchTrolleyToRoute(Trolley trolley)
        {
            if (activeRoutes.Count == 1)
            {
                var activeRoute = activeRoutes.First().Value;
                var trackingInfo = trolleyTrackingInfo[trolley.ID];
                trackingInfo.CurrentRoute = activeRoute;
            } else
            {
                foreach (var route in activeRoutes.Values)
                {
                    if (route.RouteColorRGB == trolley.IconColorRGB)
                    {
                        var trackingInfo = trolleyTrackingInfo[trolley.ID];
                        trackingInfo.CurrentRoute = route;
                        break;
                    }
                }

            }

        }


        /// <summary>
        /// Get local stop list structure.   Stops don't change frequently, so this needs to be updated only
        /// periodically
        /// </summary>
        private static void UpdateStopList()
        {

            var oldStopSummaryList = stopSummaries;
            var stops = db.Stops.ToList<Stop>();
            stopSummaries = new Dictionary<int, StopSummary>();

            foreach (var stop in stops)
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
                        summaryStop.Value.NextTrolleyArrivalTime = oldStopSummaryList[summaryStop.Key].NextTrolleyArrivalTime;
                    }
                }
            }
        }



        private static void UpdateActiveRoutes()
        {
            var activeRoutesSummary = ActiveRoutes.GetActiveRoutes();

            // Add new routes from active route set
            foreach (var activeRouteSummary in activeRoutesSummary)
            {
                if (!activeRoutes.ContainsKey(activeRouteSummary.ID))
                {
                    LoadRoute(activeRouteSummary.ID);
                }
            }

            // Remove local memory routes not in active route set
            var localRouteIDs = activeRoutes.Keys.ToList();
            foreach(var routeID in localRouteIDs)
            {
                if (activeRoutesSummary.Find(rt => rt.ID == routeID) == null)
                {
                    activeRoutes.Remove(routeID);
                }
            }


        }

        private static void LoadRoute(int routeID)
        {
            var route = (from r in db.Routes
                          orderby r.ShortName
                          where r.ID == routeID
                          select r).ToList().FirstOrDefault();
            if (route == null) return;

            var routeStops = (from rs in db.RouteStops
                             orderby rs.StopSequence
                             where rs.RouteID == routeID
                             select rs).ToList();
            route.RouteStops = routeStops;

            var shapes = (from s in db.Shapes
                          orderby s.Sequence
                          where s.RouteID == routeID
                          select s).ToList();
            route.Shapes = shapes;
            activeRoutes.Add(route.ID, route);
        }

        private static void SetNewRunningTrolleys(List<RunningTrolley> newRunningTrolleys)
        {

            // Get full list of trolleys to convert to running list only keyed by trolleyNumber instead of ID
            var trolleyList = QueryAllTrolleys();

            runningTrolleys.Clear();
            foreach (var runningTrolley in newRunningTrolleys)
            {
                var trolley = trolleyList.Find(tr => tr.ID == runningTrolley.ID);
                if (trolley != null)
                {
                    runningTrolleys.Add(trolley.Number, trolley);
                }
            }

        }


    private static bool TimeToUpdate()
        {
            var shouldUpdate = false;

            // Update every 15 minutes or after crossing the next quarter hour
            var currentTime = DateTime.Now;
            if ( (currentTime.Minute / 15 != lastDataUpdateTime.Minute / 15) ||
                (currentTime - lastDataUpdateTime).TotalMinutes > 15.0 )
            {
                shouldUpdate = true;
                // Don't update lastDataUpdateTime unless schedule is active (see caller)
            }
            return shouldUpdate;
        }

        private static void UpdateStopTime(TrolleyTrackingInfo trackingInfo)
        {
            // See if trolley has moved far enough to justify a full stop distance check
            var currentLocation = new Coordinate((double)trackingInfo.CurrentTrolley.CurrentLat, (double)trackingInfo.CurrentTrolley.CurrentLon);
            if (currentLocation.Distance(trackingInfo.LastReportedLocation) > MinMoveDistance)
            {
                // Heading will be integer 0..359
                trackingInfo.CurrentHeading = ((int) (trackingInfo.LastReportedLocation.DirectionToward(currentLocation) + 0.5)) % 360;
                CheckForStopInRange(trackingInfo, currentLocation);
                trackingInfo.LastReportedLocation = currentLocation; 
            }

        }


        private static List<Trolley> QueryAllTrolleys()
        {
            var trolleys = from t in db.Trolleys
                           orderby t.Number, t.TrolleyName
                           select t;
            return trolleys.ToList();
        }

        /// <summary>
        /// Check for stop proximity - try to minimize number of polled stops
        ///   - First preference is the previous stop
        ///    Then loop through all stops
        /// </summary>
        /// <param name="trackingInfo"></param>
        /// <param name="currentLocation"></param>
        private static void CheckForStopInRange(TrolleyTrackingInfo trackingInfo, Coordinate currentLocation)
        {
            if (stopSummaries.ContainsKey(trackingInfo.LastStopID))
            {
                // Test if Still within stop zone
                var stopSummary = stopSummaries[trackingInfo.LastStopID];
                if (currentLocation.Distance(new Coordinate(stopSummary.Lat, stopSummary.Lon)) < StopRadiusCheck) {
                    return;
                }
                Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Left stop zone {stopSummary.Name}");
            }

            if (trackingInfo.CurrentRoute == null) return;  // No route found to assign to trolley

            foreach(var routeStop in trackingInfo.CurrentRoute.RouteStops)
            {
                var stopID = routeStop.StopID;
                var stopSummary = stopSummaries[stopID];
                if (currentLocation.Distance(new Coordinate(stopSummary.Lat, stopSummary.Lon)) < StopRadiusCheck)
                {
                    Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Entered stop zone {stopSummary.Name}");
                    // Found new stop zone; validate that it's an upcoming stop on right side and not on the left side 
                    // (which would be on the return trip)
                    if (StopIsOnRight(trackingInfo, currentLocation, stopSummary))
                    {
                        UpdateStopArrivalTime(trackingInfo.CurrentRouteStop);
                        UpdateTravelTime(trackingInfo);

                        PredictStopArrivalTimes(trackingInfo);

                        trackingInfo.LastStopID = stopSummary.ID;
                        Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Stop was on right {stopSummary.Name}");
                        return;
                    }
                    else
                    {
                        Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Stop not on right {stopSummary.Name}");
                    }
                }

            }

            // Not currently in any stop zone
            trackingInfo.LastStopID = -1;
        }

        /// <summary>
        /// Set or refresh stop arrival times for all stops on this route based on just arriving at current stop
        /// </summary>
        /// <param name="trackingInfo"></param>
        /// <param name="currentLocation"></param>
        /// <param name="stopSummary"></param>
        private static void PredictStopArrivalTimes(TrolleyTrackingInfo trackingInfo)
        {
            var trolleyNumber = trackingInfo.CurrentTrolley.Number;

            var now = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);
            var stopsCount = trackingInfo.CurrentRoute.RouteStops.Count;
            int travelSeconds = 0;  // Total travel time from current stop
            for (var i=0; i<= stopsCount; i++)
            {
                var stopIndex = (trackingInfo.CurrentRouteStopIndex + i) % stopsCount;
                var routeStop = trackingInfo.CurrentRoute.RouteStops[stopIndex];
                if (routeStop.LastTimeAtStop.HasValue)
                {
                    var timeAtStop = now.AddSeconds(travelSeconds);
                    var previousRouteStop = trackingInfo.CurrentRoute.RouteStops[(stopIndex + stopsCount -1) % stopsCount];
                    var previousSummary = stopSummaries[previousRouteStop.StopID];
                    SetArrivalTime(trolleyNumber, previousSummary, timeAtStop);
                    travelSeconds += routeStop.AverageTravelTimeToNextStop;
                }
            }

        }

        private static void SetArrivalTime(int trolleyNumber, StopSummary stopSummary, DateTime timeAtStop)
        {
            if (stopSummary.NextTrolleyArrivalTime.ContainsKey(trolleyNumber))
            {
                stopSummary.NextTrolleyArrivalTime[trolleyNumber] = timeAtStop;
            }
            else
            {
                stopSummary.NextTrolleyArrivalTime.Add(trolleyNumber, timeAtStop);
            }

        }





        /// <summary>
        /// Calculate and update travel time from previous to current stop
        /// </summary>
        /// <param name="trackingInfo"></param>
        private static void UpdateTravelTime(TrolleyTrackingInfo trackingInfo)
        {
            var routeStop = trackingInfo.CurrentRouteStop;
            if (routeStop == null) return;
            var lastRouteStop = trackingInfo.PreviousRouteStop;

            if (!lastRouteStop.LastTimeAtStop.HasValue) return;
            var elapsedSeconds = (routeStop.LastTimeAtStop - lastRouteStop.LastTimeAtStop).Value.TotalSeconds;

            if (elapsedSeconds > MaxTimeBetweenStops)
            {
                // Invalid time - likely from a long break or first stop of the day
                return;
            }

            var newTravelTime = (int)elapsedSeconds;

            if (routeStop.AverageTravelTimeToNextStop != 0)
            {
                // Average is approximted by a weighted moving average instead of true moving average
                var adjustment = (newTravelTime - routeStop.AverageTravelTimeToNextStop) / 50;

                // Avoid having a single anomaly create a large swing
                if (adjustment > (routeStop.AverageTravelTimeToNextStop / 2)) adjustment = (routeStop.AverageTravelTimeToNextStop / 2);

                newTravelTime += adjustment;
            }
            routeStop.AverageTravelTimeToNextStop = newTravelTime;

            UpdateTravelTimeInDB(routeStop);
        }


        private static void UpdateTravelTimeInDB(RouteStop routeStop)
        {
            var dbRouteStop = (from rs in db.RouteStops
                               where rs.ID == routeStop.ID
                               select rs).FirstOrDefault();
            if (dbRouteStop == null) return;
            dbRouteStop.AverageTravelTimeToNextStop = routeStop.AverageTravelTimeToNextStop;
            db.SaveChanges();
        }

        private static void UpdateStopArrivalTime(RouteStop currentRouteStop)
        {
            if (currentRouteStop == null) return;
            currentRouteStop.LastTimeAtStop = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);

            var dbRouteStop = (from rs in db.RouteStops
                             where rs.ID == currentRouteStop.ID
                             select rs).FirstOrDefault();
            if (dbRouteStop == null) return;
            dbRouteStop.LastTimeAtStop = currentRouteStop.LastTimeAtStop;
            db.SaveChanges();

        }

        /// <summary>
        /// Test that route path next to this stop is heading in the same direction as trolley,
        /// and not the return path
        /// </summary>
        /// <param name="trackingInfo">Tracking info - CurrentRouteStop and PreviousRouteStop will be assigned here</param>
        /// <param name="currentLocation"></param>
        /// <param name="stopSummary"></param>
        /// <returns></returns>
        private static bool StopIsOnRight(TrolleyTrackingInfo trackingInfo, Coordinate currentLocation, StopSummary stopSummary)
        {
            // Find route direction at our current location (stop)
            var routeStopIndex = FindRouteStopIndex(trackingInfo, stopSummary.ID);
            if (routeStopIndex < 0)
            {
                Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Route Stop Index not found");
                return false;
            }
            var routeStop = trackingInfo.CurrentRoute.RouteStops[routeStopIndex];
            var numStops = trackingInfo.CurrentRoute.RouteStops.Count;
            trackingInfo.PreviousRouteStop = trackingInfo.CurrentRoute.RouteStops[(routeStopIndex + numStops - 1) % numStops];
            trackingInfo.CurrentRouteStop = routeStop;
            trackingInfo.CurrentRouteStopIndex = routeStopIndex;
            var routeSegmentIndex = routeStop.RouteSegmentIndex;
            if (routeSegmentIndex < 0)
            {
                Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Route Stop Index not set");
                return false;
            }

            var segmentCount = trackingInfo.CurrentRoute.Shapes.Count;
            var segmentEnd = trackingInfo.CurrentRoute.Shapes[routeSegmentIndex];
            var segmentStart = trackingInfo.CurrentRoute.Shapes[(routeSegmentIndex + segmentCount - 1) % segmentCount];
            var routeSegmentStart = new Coordinate(segmentStart.Lat, segmentStart.Lon);
            var routeSegmentEnd = new Coordinate(segmentEnd.Lat, segmentEnd.Lon);
            var routeDirection = routeSegmentStart.DirectionToward(routeSegmentEnd);
            var trolleyDirection = trackingInfo.CurrentHeading;

            // Convert directions from 0..360 to +/-180 for comparison
            if (routeDirection > 180) routeDirection = routeDirection - 360;
            if (trolleyDirection > 180) trolleyDirection = trolleyDirection - 360;


            Debug.WriteLine($"Trolley {trackingInfo.CurrentTrolley.Number} Route direction: {routeDirection}, trolley heading: {trolleyDirection}");
            if (Math.Abs((int)routeDirection - trolleyDirection) < 130)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find stop index in the ordered stop list
        /// </summary>
        /// <param name="trackingInfo"></param>
        /// <param name="stopID"></param>
        /// <returns>Stop index or -1 if not found</returns>
        private static int FindRouteStopIndex(TrolleyTrackingInfo trackingInfo, int stopID)
        {
            var stopIndex = -1;
            for (int i=0; i< trackingInfo.CurrentRoute.RouteStops.Count; i++)
            {
                if (trackingInfo.CurrentRoute.RouteStops[i].StopID == stopID)
                {
                    stopIndex = i;
                    break;
                }

            }
            return stopIndex;
        }
    }

    public class TrolleyTrackingInfo
    {
        public TrolleyTrackingInfo(Trolley trolley)
        {
            LastStopID = -1;
            LastReportedLocation = new Coordinate((double)trolley.CurrentLat, (double)trolley.CurrentLon);
            CurrentTrolley = trolley;
        }

        public Trolley CurrentTrolley { get; set; }
        public Route CurrentRoute { get; set; }

        /// <summary>
        /// Current direction of travel in degrees - 0..359, 0 is due north
        /// </summary>
        public int CurrentHeading { get; set; }
        public int LastStopID { get; set; }
        public int CurrentRouteStopIndex { get; set; }
        public RouteStop CurrentRouteStop { get; set; }
        public RouteStop PreviousRouteStop { get; set; }
        public Coordinate LastReportedLocation { get; set; }

    }
}