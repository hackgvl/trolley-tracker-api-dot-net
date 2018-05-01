using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrolleyTracker.ViewModels;

namespace TrolleyTracker.Models
{
    /// <summary>
    /// Global cache to quickly satisfy large number of 
    /// client queries from cache.
    /// </summary>
    public static class TrolleyCache
    {
        private static readonly object _lock = new object();

        private static Dictionary<int, RunningTrolley> trolleyCache;
        private static bool routeIsActive;  // True if any route is currently active

        private const int CacheCheckInterval = 60; // Seconds between cache check
        private const int MaxCacheAge = 300;  // Seconds before removing from cache

        private const int MinRouteCheckInterval = 10; // Minimum minutes between check
        private const int MaxRouteCheckInterval = 20; // Maximum minutes between check
        private static DateTime lastRouteCheck;

        private static DateTime lastCacheCheck;

        public static void Initialize()
        {
            trolleyCache = new Dictionary<int, RunningTrolley>();
            lastCacheCheck = DateTime.Now;

            //SetRoutesActive();
            lastRouteCheck = DateTime.Now.AddMinutes( - (MaxRouteCheckInterval * 2));  // Ensure route check happens first time through
        }



        public static List<RunningTrolley> GetRunningTrolleys(bool isDebug)
        {

            lock (_lock)
            {
                CheckActiveRoutes();

                var runningTrolleys = new List<RunningTrolley>();
                if (routeIsActive || isDebug)
                {
                    AgeCacheEntries();
                    foreach (var trolley in trolleyCache.Values)
                    {
                        runningTrolleys.Add(trolley);
                    }
                }

                return runningTrolleys;
            }
        }


        private static void CheckActiveRoutes()
        {
            var elapsedMinutes = (int)((DateTime.Now - lastRouteCheck).TotalMinutes);
            if (elapsedMinutes < MinRouteCheckInterval)
            {
                return;
            }
            // Check on :01, :16, :31 and :46...
            if ((DateTime.Now.Minute % 15 == 1) || (elapsedMinutes > 30))
            {
                lastRouteCheck = DateTime.Now;
                SetRoutesActive();
            }
        }

        private static void SetRoutesActive()
        {
            var runningList = Controllers.ActiveRoutes.GetActiveRoutes();
            routeIsActive = runningList.Count > 0;
        }

        /// <summary>
        /// Check items in cache periodically and remove
        /// any that are inactive (haven't called Update() for max age period)
        /// Call from inside a lock() statement
        /// </summary>
        private static void AgeCacheEntries()
        {
            if ((DateTime.Now - lastCacheCheck).TotalSeconds < CacheCheckInterval)
            {
                return;
            }
            var deleteList = new List<int>();
            foreach (var runningTrolley in trolleyCache.Values)
            {
                if ((DateTime.Now - runningTrolley.LastUpdated).TotalSeconds > MaxCacheAge)
                {
                    deleteList.Add(runningTrolley.ID);
                }
            }

            foreach (var trolleyID in deleteList)
            {
                trolleyCache.Remove(trolleyID);
            }

            lastCacheCheck = DateTime.Now;

        }


        public static void UpdateTrolley(Trolley trolley)
        {
            lock (_lock)
            {
                if (!trolleyCache.ContainsKey(trolley.ID))
                {
                    var runningTrolley = new RunningTrolley(trolley);
                    trolleyCache.Add(trolley.ID, runningTrolley);
                }
                else
                {
                    var runningTrolley = trolleyCache[trolley.ID];
                    if (trolley.CurrentLat.HasValue)
                    {
                        runningTrolley.Lat = (double)trolley.CurrentLat;
                    }
                    if (trolley.CurrentLon.HasValue)
                    {
                        runningTrolley.Lon = (double)trolley.CurrentLon;
                    }
                    runningTrolley.LastUpdated = DateTime.Now;
                    runningTrolley.PassengerLoad = trolley.PassengerLoad;
                    runningTrolley.Capacity = trolley.Capacity;
                }


            }

        }
    }

}