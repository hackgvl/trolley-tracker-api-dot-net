using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GreenlinkTracker;
using NLog;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    public class PollTrolleysHandler
    {
        private CancellationToken cancellationToken;
        private Syncromatics syncromatics;

        private Syncromatics.Service trolleyService;

        private Dictionary<int, Route> activeRoutes;
        private DateTime lastRouteUpdated = DateTime.Now.AddSeconds(-60);
        private int RouteUpdateInterval = 60; // in Seconds

        private Dictionary<int, Syncromatics.Route> localRouteIDToSyncromaticsRoute = new Dictionary<int, Syncromatics.Route>();

        private Dictionary<string, int> syncroTrolleyNumberToLocalTrolleyNumber=null;
        // vehicle.name contains the number Greenlink has assigned to the trolley
        private Dictionary<string, Trolley> syncroTrolleyNumberToLocalTrolley = null;

        // Last update time seen from Syncromatics API for each vehicle / by vehicle.id
        private Dictionary<int, DateTime> lastVehicleUpdateTime = new Dictionary<int, DateTime>();

        // Items to log single message type per application execution
        private enum SingleLogType {TrolleyNotFound=0, UnmatchedSyncromaticsVehicle=1, UnmatchedRouteName=2 };
        private BitArray logSent = new BitArray(3);
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private DateTime lastTrolleySaveTime = DateTime.Now;
        private const int TrolleySaveInterval = 5; // Minutes


        // Kludge to find vehicles not on a route
        private List<Syncromatics.Route> ghostRoutes;


        public PollTrolleysHandler(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            ghostRoutes = new List<Syncromatics.Route>();
            syncromatics = new Syncromatics(cancellationToken);
            logSent = new BitArray(Enum.GetNames(typeof(SingleLogType)).Length);
        }


        /// <summary>
        /// Call periodically to update trolley locations
        /// </summary>
        public async Task UpdateTrolleys()
        {
            if (!AppSettingsInterface.UseSyncromatics) return;  // Not configured for Syncromatics
            CheckActiveRoutes();
            if (activeRoutes.Count == 0) return;  // Nothing scheduled, nothing to poll from Syncromatics

            if (trolleyService == null)
            {
                if (!await CheckSyncromaticsData()) return;
            }

            await QueryVehiclePositions();

        }

        private async Task QueryVehiclePositions()
        {
            // Determine if time to save trolley positions
            var saveTrolleysToDB = false;
            if ((DateTime.Now - lastTrolleySaveTime).TotalMinutes > TrolleySaveInterval)
            {
                saveTrolleysToDB = true;
                lastTrolleySaveTime = DateTime.Now;
            }

            foreach (var route in activeRoutes.Values)
            {
                await GetVehiclesOnRoute(route, saveTrolleysToDB);
            }

            await CheckForAdditionalVehicles(saveTrolleysToDB);
        }


        /// <summary>
        /// Check for the case of vehicle(s) running on a route other than the scedule.
        /// This might be in the case of being schedule for top-of-main + heart-of-main,
        /// however due to vehicle breakdown, those routes are combined as one of the
        /// combination routes.
        /// </summary>
        /// <param name="saveTrolleysToDB"></param>
        /// <returns></returns>
        private async Task CheckForAdditionalVehicles(bool saveTrolleysToDB)
        {
            var runningTrolleys = TrolleyCache.GetRunningTrolleys(false);
            if (runningTrolleys.Count >= activeRoutes.Count)
            {
                await TrackGhostVehicles(saveTrolleysToDB);
                return;  // All expected trolleys found
            }

            // Poll all remaining routes for any possible vehicles
            foreach (var syncroRoute in trolleyService.routes)
            {
                if (!localRouteIDToSyncromaticsRoute.Values.Contains(syncroRoute)) {
                    await CheckForVehiclesOnRoute(syncroRoute, saveTrolleysToDB);
                }
            }

        }


        private async Task TrackGhostVehicles(bool saveTrolleysToDB)
        {
            foreach (var syncroRoute in ghostRoutes)
            {
                await CheckForVehiclesOnRoute(syncroRoute, saveTrolleysToDB);
            }
            
        }


        /// <summary>
        /// Copy of GetVehiclesOnRoute - kludge to handle 'ghost vehicles' not
        /// on route.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="saveTrolleysToDB">True if time to save trolley positions</param>
        private async Task CheckForVehiclesOnRoute(Syncromatics.Route syncromaticsRoute, bool saveTrolleysToDB)
        {
            var vehicles = await syncromatics.GetVehiclesOnRoute(syncromaticsRoute.id);

            if (vehicles.Count == 0)
            {
                if (ghostRoutes.Contains(syncromaticsRoute))
                {
                    // Drive logged out of this route
                    ghostRoutes.Remove(syncromaticsRoute);
                }
                return;
            }

            foreach (var vehicle in vehicles)
            {
                if (lastVehicleUpdateTime.ContainsKey(vehicle.id))
                {
                    // Check for stall (no update from Syncromatics)
                    if (lastVehicleUpdateTime[vehicle.id] == vehicle.lastUpdated)
                    {
                        //Trace.WriteLine("Stalled vehicle, syncromatics # " + vehicle.name);
                        continue;
                    }
                    lastVehicleUpdateTime[vehicle.id] = vehicle.lastUpdated;
                }
                else
                {
                    lastVehicleUpdateTime.Add(vehicle.id, vehicle.lastUpdated);
                }

                var trolley = FindMatchingTrolley(vehicle);
                if (trolley != null)
                {
                    Trace.WriteLine("Tracking Ghost trolley " + trolley.Number);

                    trolley.CurrentLat = vehicle.lat;
                    trolley.CurrentLon = vehicle.lon;
                    trolley.Capacity = vehicle.capacity;
                    trolley.PassengerLoad = vehicle.passengerLoad;
                    var colorBlack = "#000000";
                    if (trolley.IconColorRGB != colorBlack) {
                        trolley.IconColorRGB = colorBlack;
                        saveTrolleysToDB = true;
                    }
                    trolley.LastBeaconTime = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);

                    if (saveTrolleysToDB)
                        await SaveTrolleyToDB(trolley);

                    TrolleyCache.UpdateTrolley(trolley);
                    StopArrivalTime.UpdateTrolleyStopArrivalTime(trolley);
                    if (!ghostRoutes.Contains(syncromaticsRoute))
                    {
                        ghostRoutes.Add(syncromaticsRoute);
                    }

                }
            }
        }



        /// <summary>
        /// Get all vehicles on this route - normally just a single trolley
        /// </summary>
        /// <param name="route"></param>
        /// <param name="saveTrolleysToDB">True if time to save trolley positions</param>
        private async Task GetVehiclesOnRoute(Route route, bool saveTrolleysToDB)
        {
            var syncromaticsRoute = await FindMatchingRoute(route);
            if (syncromaticsRoute == null)
            {
                //Trace.WriteLine("No route match found to " + syncromaticsRoute.name);
                return;
            }
            var vehicles = await syncromatics.GetVehiclesOnRoute(syncromaticsRoute.id);

            if (ghostRoutes.Contains(syncromaticsRoute))
            {
                ghostRoutes.Remove(syncromaticsRoute);
            }

            if (vehicles.Count == 0)
            {
                // Drive logged out of this route
                localRouteIDToSyncromaticsRoute.Remove(route.ID);
                return;
            }

            foreach (var vehicle in vehicles)
            {
                if (lastVehicleUpdateTime.ContainsKey(vehicle.id))
                {
                    // Check for stall (no update from Syncromatics)
                    if (lastVehicleUpdateTime[vehicle.id] == vehicle.lastUpdated)
                    {
                        //Trace.WriteLine("Stalled vehicle, syncromatics # " + vehicle.name);
                        continue;
                    }
                    lastVehicleUpdateTime[vehicle.id] = vehicle.lastUpdated;
                }
                else
                {
                    lastVehicleUpdateTime.Add(vehicle.id, vehicle.lastUpdated);
                }

                var trolley = FindMatchingTrolley(vehicle);
                if (trolley != null)
                {
                    //Trace.WriteLine("Tracking trolley " + trolley.Number);

                    trolley.CurrentLat = vehicle.lat;
                    trolley.CurrentLon = vehicle.lon;
                    trolley.Capacity = vehicle.capacity;
                    trolley.PassengerLoad = vehicle.passengerLoad;
                    trolley.LastBeaconTime = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);

                    if (saveTrolleysToDB)
                        await SaveTrolleyToDB(trolley);

                    await CheckTrolleyColorMatch(trolley, route);

                    TrolleyCache.UpdateTrolley(trolley);
                    StopArrivalTime.UpdateTrolleyStopArrivalTime(trolley);

                }
            }
        }

        /// <summary>
        /// Confirm that trolley color matches route color.   If not, change it and
        /// save to DB and reset arrival time logic for that trolley.
        /// </summary>
        /// <param name="trolley"></param>
        /// <param name="route"></param>
        private async Task CheckTrolleyColorMatch(Trolley trolley, Route route)
        {
            if (trolley.IconColorRGB.ToLower() == route.RouteColorRGB.ToLower()) return;
            trolley.IconColorRGB = route.RouteColorRGB;
            await SaveTrolleyToDB(trolley);
            
            StopArrivalTime.ResetTrolleyInfo(trolley);
        }


        private async Task SaveTrolleyToDB(Trolley trolley)
        {
            using (var db = new TrolleyTracker.Models.TrolleyTrackerContext())
            {
                Trolley dbTrolley = (from Trolley t in db.Trolleys
                                     where t.ID == trolley.ID
                                     select t).FirstOrDefault<Trolley>();

                dbTrolley.CurrentLat = trolley.CurrentLat;
                dbTrolley.CurrentLon = trolley.CurrentLon;
                dbTrolley.LastBeaconTime = trolley.LastBeaconTime;
                dbTrolley.IconColorRGB = trolley.IconColorRGB;
                dbTrolley.Capacity = trolley.Capacity;
                dbTrolley.PassengerLoad = trolley.PassengerLoad;
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        private Trolley FindMatchingTrolley(Syncromatics.Vehicle vehicle)
        {
            if (syncroTrolleyNumberToLocalTrolley == null)
            {
                LoadSyncroTrolleyNumberMap();
            }

            if (syncroTrolleyNumberToLocalTrolley.ContainsKey(vehicle.name))
            {
                return syncroTrolleyNumberToLocalTrolley[vehicle.name];
            }

            if (!syncroTrolleyNumberToLocalTrolleyNumber.ContainsKey(vehicle.name))
            {
                SingleLog(SingleLogType.UnmatchedSyncromaticsVehicle, $"Unable to match Syncromatics vehicle '{vehicle.name}' to any trolley");
                return null;
            }
            var trolley = GetTrolleyByNumber(syncroTrolleyNumberToLocalTrolleyNumber[vehicle.name]);
            syncroTrolleyNumberToLocalTrolley[vehicle.name] = trolley;
            return trolley;

        }

        private void LoadSyncroTrolleyNumberMap()
        {
            syncroTrolleyNumberToLocalTrolley = new Dictionary<string, Trolley>();
            var trolleyList = GetAllTrolleys();
            foreach (var trolley in trolleyList)
            {
                var strKey = trolley.SyncromaticsNumber.ToString();
                // Check before add to allow all 0 to be used- mapping will be invalid
                if (!syncroTrolleyNumberToLocalTrolley.ContainsKey(strKey))
                {
                    syncroTrolleyNumberToLocalTrolley.Add(strKey, trolley);
                }
            }
        }

        private List<Trolley> GetAllTrolleys()
        {

            using (var db = new TrolleyTrackerContext())
            {
                var trolleys = from t in db.Trolleys
                               select t;
                return trolleys.ToList();
            }

        }


        private Trolley GetTrolleyByNumber(int trolleyNumber)
        {
            using (var db = new TrolleyTracker.Models.TrolleyTrackerContext())
            {
                Trolley trolley = (from Trolley t in db.Trolleys
                               where t.Number == trolleyNumber
                               select t).FirstOrDefault<Trolley>();

                if (trolley == null)
                {
                    SingleLog(SingleLogType.UnmatchedSyncromaticsVehicle, $"Unable to find Trolley {trolleyNumber}");
                }

                return trolley;
            }
        }


        /// <summary>
        /// Match local route to a Syncromatics route as already defined or by keyword
        /// There may be many route variations.
        /// </summary>
        /// <param name="route"></param>
        /// <returns>syncromatics route ID or null if not found</returns>
        private async Task<Syncromatics.Route> FindMatchingRoute(Route route)
        {
            if (localRouteIDToSyncromaticsRoute.ContainsKey(route.ID))
            {
                return localRouteIDToSyncromaticsRoute[route.ID];
            }

            var localKeyword = FindLocalKeyword(route.LongName);
            if (localKeyword == "") return null;

            foreach (var syncroRoute in trolleyService.routes)
            {
                var lcaseName = syncroRoute.name.ToLower();
                if (lcaseName.Contains(localKeyword))
                {
                    // Hack: since we cannot directly query by vehicle, don't perform final
                    // route selection until it can be confirmed to have a vehicle.
                    var vehicles = await syncromatics.GetVehiclesOnRoute(syncroRoute.id);
                    if (vehicles.Count > 0)
                    {
                        // At least one vehicle running - use this route match
                        localRouteIDToSyncromaticsRoute.Add(route.ID, syncroRoute);
                        return syncroRoute;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find defining keyword that will match routes by names on both systems
        /// </summary>
        /// <param name="longName"></param>
        /// <returns>keyword or "" if not found</returns>
        private string FindLocalKeyword(string longName)
        {
            var lcaseName = longName.ToLower();
            if (lcaseName.Contains("comb")) return "comb";
            if (lcaseName.Contains("heart")) return "heart";
            if (lcaseName.Contains("arts")) return "arts";
            if (lcaseName.Contains("augusta")) return "augusta";
            if (lcaseName.Contains("lunch")) return "lunch";
            if (lcaseName.Contains("drive")) return "drive";
            if (lcaseName.Contains("well")) return "well";
            if (lcaseName.Contains("top")) return "top";  // Last to avoid accidenatal match by substring of a longer name

            SingleLog(SingleLogType.UnmatchedRouteName, $"Unable to find keywords of route '{longName}' to match");

            return ""; // Not found
        }

        private async Task<bool> CheckSyncromaticsData()
        {
            var services = await syncromatics.GetServices();
            foreach(var service in services)
            {
                string lcaseService = service.name.ToLower();
                if (lcaseService.Contains("trolley"))
                {
                    trolleyService = service;
                    break;
                }
            }

            return (trolleyService != null);
        }

        private void CheckActiveRoutes()
        {
            if (activeRoutes == null ||
                (DateTime.Now - lastRouteUpdated).TotalSeconds > RouteUpdateInterval)
            {
                activeRoutes = StopArrivalTime.GetActiveRoutes();
                lastRouteUpdated = DateTime.Now;
            }

        }

        private void SingleLog(SingleLogType logType, string message)
        {
            if (!logSent[(int)logType])
            {
                logger.Info(message);
                logSent[(int)logType] = true;
            }
        }
    }
}