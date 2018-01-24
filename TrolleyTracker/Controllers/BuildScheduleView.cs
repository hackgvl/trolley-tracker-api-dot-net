using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;
using MvcSchedule.Objects;
using NLog;

namespace TrolleyTracker.Controllers
{
    public class BuildScheduleView
    {
        public static List<string> daysOfWeek = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static RouteScheduleViewModel ConfigureScheduleView(bool calculateEffectiveSchedule)
        {
            var vm = new RouteScheduleViewModel();

            using (var db = new TrolleyTrackerContext())
            {
                var routeScheduleList = db.RouteSchedules
                    .Include(rs => rs.Route)
                    .OrderBy(rs => rs.DayOfWeek)
                    .ThenBy(rs => rs.StartTime)
                    .ThenBy(rs => rs.Route.ShortName)
                    .ToList();

                vm.RouteSchedules = routeScheduleList;

                if (calculateEffectiveSchedule)
                {
                    var routeScheduleOverrideList = db.RouteScheduleOverrides
                        .Include(rso => rso.OverriddenRoute)
                        .Include(rso => rso.NewRoute)
                        .OrderBy(rso => rso.OverrideDate)
                        .ThenBy(rso => rso.StartTime)
                        .ThenBy(rso => rso.NewRoute.ShortName)
                        .ToList();

                    vm.EffectiveRouteSchedules = BuildEffectiveRouteSchedule(routeScheduleList, routeScheduleOverrideList);
                }
            }

            vm.Options = new MvcScheduleGeneralOptions
            {
                Layout = LayoutEnum.Horizontal,
                SeparateDateHeader = false,
                FullTimeScale = true,
                TimeScaleInterval = 60,
                StartOfTimeScale = new TimeSpan(6, 0, 0),
                EndOfTimeScale = new TimeSpan(23, 59, 59),
                IncludeEndValue = false,
                ShowValueMarks = true,
                ItemCss = "normal",
                AlternatingItemCss = "normal2",
                RangeHeaderCss = "heading",
                TitleCss = "heading",
                AutoSortTitles = false,
                BackgroundCss = "empty"
            };

            return vm;
        }


        /// <summary>
        /// Obtain effective schedules, and format for display into RouteScheduleSummary
        /// NOTES:  Schedules that result from an override do not have a real database ID, and are not stored in the database
        /// There may be multiple ID 0 values from created schedules
        /// </summary>
        /// <param name="dateLabels"></param>
        /// <param name="routeSchedules"></param>
        /// <param name="routeScheduleOverrides"></param>
        /// <returns></returns>
        public static List<RouteScheduleSummary> BuildEffectiveRouteSchedule(IEnumerable<RouteSchedule> routeSchedules,
            IEnumerable<RouteScheduleOverride> routeScheduleOverrides
            )
        {
            var effectiveScheduleSummaries = new List<RouteScheduleSummary>();

            try
            {

                var localNow = UTCToLocalTime.LocalTimeFromUTC(DateTime.UtcNow);
                var scheduleToDate = new Dictionary<RouteSchedule, DateTime>();
                var effectiveRouteSchedules = BuildEffectiveRouteSchedule(localNow, 14, routeSchedules, scheduleToDate, routeScheduleOverrides);

                foreach (var routeSchedule in effectiveRouteSchedules)
                {
                    var scheduleSummary = new RouteScheduleSummary(routeSchedule);
                    var scheduleDate = scheduleToDate[routeSchedule];
                    scheduleSummary.DayOfWeek = scheduleDate.ToShortDateString() + " " + scheduleSummary.DayOfWeek;
                    effectiveScheduleSummaries.Add(scheduleSummary);
                }

                // Sorting is needed to avoid some unusual MVCSchedule bugs - order may have been changed
                // after some route start times were delayed.
                effectiveScheduleSummaries.Sort(CompareRouteStartTimes);
            }
            catch (Exception ex)
            {
                var message = String.Format($"Exception building schedule: {ex.GetType()}; with message: {ex.Message}");
                logger.Error(ex, message);

            }

            return effectiveScheduleSummaries;

        }

        private static int CompareRouteStartTimes(RouteScheduleSummary routeSchedule1, RouteScheduleSummary routeSchedule2)
        {
            DateTime startDate1 = DateTime.Parse(routeSchedule1.DayOfWeek);
            DateTime startDate2 = DateTime.Parse(routeSchedule2.DayOfWeek);
            int dateCompare = startDate1.CompareTo(startDate2);
            if (dateCompare != 0) return dateCompare;
            DateTime startTime1 = DateTime.Parse(routeSchedule1.StartTime);
            DateTime startTime2 = DateTime.Parse(routeSchedule2.StartTime);
            return startTime1.CompareTo(startTime2);
        }

        public static List<RouteSchedule> BuildEffectiveRouteSchedule(DateTime startDate, int numDays,
            IEnumerable<RouteSchedule> routeSchedules,
            Dictionary<RouteSchedule, DateTime> scheduleToDate,
            IEnumerable<RouteScheduleOverride> routeScheduleOverrides
                                )
        {
            var effectiveSchedules = new List<RouteSchedule>();


            var labelDate = startDate;
            for (int day = 0; day < numDays; day++)
            {
                // Since schedules don't contain full date, the actual date is a separate associative array.  This is built
                // After every day by adding the date for that day.
                var newEffectiveSchedules = new List<RouteSchedule>();
                ProcessEffectiveDay(labelDate, day, routeSchedules, routeScheduleOverrides, newEffectiveSchedules);
                foreach(var routeSchedule in newEffectiveSchedules)
                {
                    scheduleToDate.Add(routeSchedule, labelDate);
                }
                effectiveSchedules.AddRange(newEffectiveSchedules);
                labelDate = labelDate.AddDays(1);

            }

            return effectiveSchedules;

        }


        private static void ProcessEffectiveDay(DateTime scheduleDate, int day, IEnumerable<RouteSchedule> routeSchedules, IEnumerable<RouteScheduleOverride> routeScheduleOverrides, List<RouteSchedule> effectiveSchedules)
        {

            // Obtain any special schedules for the day
            var specialSchedules = FindOverrideSchedules(routeScheduleOverrides, scheduleDate);

            var todaysSchedules = FindTodaysSchedules(routeSchedules, scheduleDate);

            foreach(var routeSchedule in todaysSchedules)
            {
                var modifiedSchedules = ModifyRouteSchedule(routeSchedule, specialSchedules);
                if (modifiedSchedules != null)
                {
                    foreach(var schedule in modifiedSchedules)
                    {
                        schedule.DayOfWeek += ((day / 7) * 7);  // Adjust for additional weeks
                        effectiveSchedules.Add(schedule);
                    }
                }

            }

            if (specialSchedules != null)
            {
                // Add special route
                foreach (var specialRoute in specialSchedules)
                {
                    if (specialRoute.OverrideType != RouteScheduleOverride.OverrideRule.NoService)
                    {
                        var routeSchedule = new RouteSchedule(specialRoute);
                        routeSchedule.DayOfWeek += ((day / 7) * 7);  // Adjust for additional weeks
                        effectiveSchedules.Add(routeSchedule);
                    }
                }

            }


            // Add any new routes
            var addedSchedules = FindAddedSchedules(routeScheduleOverrides, scheduleDate);
            if (addedSchedules != null)
            {
                foreach (var addedRoute in addedSchedules)
                {
                    var routeSchedule = new RouteSchedule(addedRoute);
                    routeSchedule.DayOfWeek += ((day / 7) * 7);  // Adjust for additional weeks
                    effectiveSchedules.Add(routeSchedule);
                }
            }
        }

        /// <summary>
        /// Apply Delete or Replace cases to fixed route
        /// </summary>
        /// <param name="routeSchedule"></param>
        /// <param name="specialSchedules"></param>
        /// <returns></returns>
        private static List<RouteSchedule> ModifyRouteSchedule(RouteSchedule routeSchedule, IEnumerable<RouteScheduleOverride> specialSchedules)
        {
            var schedules = new List<RouteSchedule>();

            if (specialSchedules != null)
            {
                schedules.AddRange(ModifyRouteScheduleForOverlap(routeSchedule, specialSchedules));

            } else
            {
                schedules.Add(new RouteSchedule(routeSchedule));
            }

            return schedules;
        }


        /// <summary>
        /// Based on the possible overlap of the special schedule, the result could be 0, 1, 2, or 3 or more new schedule time slots.
        /// </summary>
        /// <param name="originalRouteSchedule"></param>
        /// <param name="specialSchedules"></param>
        /// <returns></returns>
        public static List<RouteSchedule> ModifyRouteScheduleForOverlap(RouteSchedule originalRouteSchedule, IEnumerable<RouteScheduleOverride> specialSchedules)
        {

            RouteSchedule routeSchedule = new RouteSchedule(originalRouteSchedule);  // Schedule might be modified in this procedure

            var schedules = new List<RouteSchedule>();

            bool keepFixedRoute = true;

            foreach (var specialSchedule in specialSchedules)
            {
                if (!specialSchedule.OverriddenRouteID.HasValue ||   // Applies to all routes if null
                    (specialSchedule.OverriddenRouteID == routeSchedule.RouteID) )
                {
                    // This route is targeted
                    if (routeSchedule.StartTime.TimeOfDay >= specialSchedule.StartTime.TimeOfDay &&
                        routeSchedule.EndTime.TimeOfDay <= specialSchedule.EndTime.TimeOfDay)
                    {
                        // Exact overlap or completely contained - replace with new / cancel all
                        keepFixedRoute = false;
                    } else
                    {
                        // Check for any overlap
                        if (routeSchedule.StartTime.TimeOfDay < specialSchedule.EndTime.TimeOfDay &&
                            specialSchedule.StartTime.TimeOfDay < routeSchedule.EndTime.TimeOfDay)
                        {
                            // Have overlap
                            if (routeSchedule.StartTime.TimeOfDay < specialSchedule.StartTime.TimeOfDay)
                            {
                                // First part of orignal schedule is to be retained
                                var newSchedule = new RouteSchedule(routeSchedule);
                                newSchedule.EndTime = specialSchedule.StartTime;
                                schedules.Add(newSchedule);
                                // See if fixed schedule extends beyond end of special schedule (may be further modified by another special schedule later)
                                if (routeSchedule.EndTime.TimeOfDay <= specialSchedule.EndTime.TimeOfDay)
                                {
                                    keepFixedRoute = false;  // Completely replaced /removed
                                } else
                                {
                                    routeSchedule.StartTime = specialSchedule.EndTime;
                                }


                            } else if (routeSchedule.EndTime.TimeOfDay > specialSchedule.EndTime.TimeOfDay)
                            {
                                // End part of orignal schedule is to be retained
                                routeSchedule.StartTime = specialSchedule.EndTime;

                            }
                            // Check for beginning or end aligned with special case
                            if ( (routeSchedule.StartTime == specialSchedule.StartTime) ||
                                 (routeSchedule.EndTime == specialSchedule.EndTime) )
                            {
                                keepFixedRoute = false;
                            }

                        }
                    }
                }
            }

            if (keepFixedRoute)
            {
                schedules.Add(new RouteSchedule(routeSchedule));
            }

            return schedules;

        }


        private static IEnumerable<RouteSchedule> FindTodaysSchedules(IEnumerable<RouteSchedule> routeSchedules, DateTime scheduleDate)
        {
            int weekday = (int)scheduleDate.DayOfWeek;
            return from rs in routeSchedules
                   where rs.DayOfWeek == weekday
                   select rs;
        }


        private static IEnumerable<RouteScheduleOverride> FindOverrideSchedules(IEnumerable<RouteScheduleOverride> routeScheduleOverrides, DateTime scheduleDate)
        {
            return from rso in routeScheduleOverrides
                   where rso.OverrideDate.Date == scheduleDate.Date &&
                   (rso.OverrideType != RouteScheduleOverride.OverrideRule.Added)
                   select rso;
        }

        private static IEnumerable<RouteScheduleOverride> FindAddedSchedules(IEnumerable<RouteScheduleOverride> routeScheduleOverrides, DateTime scheduleDate)
        {
            return from rso in routeScheduleOverrides
                   where rso.OverrideDate.Date == scheduleDate.Date &&
                   (rso.OverrideType == RouteScheduleOverride.OverrideRule.Added)
                   select rso;
        }
    }
}