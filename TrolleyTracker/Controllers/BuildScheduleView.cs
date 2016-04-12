using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;
using MvcSchedule.Objects;

namespace TrolleyTracker.Controllers
{
    public class BuildScheduleView
    {
        public static List<string> daysOfWeek = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public static RouteScheduleViewModel ConfigureScheduleView(TrolleyTrackerContext db, bool calculateEffectiveSchedule)
        {
            var vm = new RouteScheduleViewModel();
            var routeSchedules = from rs in db.RouteSchedules.Include(rs => rs.Route)
                                 orderby rs.DayOfWeek, rs.StartTime ascending
                                 select rs;
            vm.RouteSchedules = (System.Data.Entity.Infrastructure.DbQuery<RouteSchedule>)routeSchedules;

            if (calculateEffectiveSchedule)
            {
                var routeScheduleOverrideList = (from rso in db.RouteScheduleOverrides.Include(rso => rso.NewRoute)
                                                 orderby rso.OverrideDate, rso.StartTime, rso.NewRoute.ShortName
                                                 select rso).ToList<RouteScheduleOverride>();

                var routeScheduleList = routeSchedules.ToList<RouteSchedule>();
                vm.EffectiveRouteSchedules = BuildEffectiveRouteSchedule(routeScheduleList, routeScheduleOverrideList);
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
            var effectiveRouteSchedules = BuildEffectiveRouteSchedule(DateTime.Now, 14, routeSchedules, routeScheduleOverrides);

            var sundayDate = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);  // Date of start of week
            var effectiveScheduleSummaries = new List<RouteScheduleSummary>();

            foreach(var routeSchedule in effectiveRouteSchedules)
            {
                var scheduleSummary = new RouteScheduleSummary(routeSchedule);
                var scheduleDate = sundayDate.AddDays(routeSchedule.DayOfWeek);
                scheduleSummary.DayOfWeek = scheduleDate.ToShortDateString() + " " + scheduleSummary.DayOfWeek;
                effectiveScheduleSummaries.Add(scheduleSummary);
            }

            return effectiveScheduleSummaries;

        }

        public static List<RouteSchedule> BuildEffectiveRouteSchedule(DateTime startDate, int numDays,
            IEnumerable<RouteSchedule> routeSchedules,
            IEnumerable<RouteScheduleOverride> routeScheduleOverrides
                                )
        {
            var effectiveSchedules = new List<RouteSchedule>();


            var labelDate = startDate;
            for (int day = 0; day < numDays; day++)
            {
                ProcessEffectiveDay(labelDate, day, routeSchedules, routeScheduleOverrides, effectiveSchedules);
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
        /// Based on the possible overlap of the special schedule, the result could be 0, 1, 2, or 3 new schedule time slots.
        /// </summary>
        /// <param name="routeSchedule"></param>
        /// <param name="specialSchedules"></param>
        /// <param name="dayLabel"></param>
        /// <returns></returns>
        private static List<RouteSchedule> ModifyRouteScheduleForOverlap(RouteSchedule routeSchedule, IEnumerable<RouteScheduleOverride> specialSchedules)
        {
            var schedules = new List<RouteSchedule>();

            bool keepFixedRoute = true;

            foreach (var specialSchedule in specialSchedules)
            {
                if (!specialSchedule.OverriddenRouteID.HasValue ||   // Applies to all routes if null
                    (specialSchedule.OverriddenRouteID == routeSchedule.RouteID) )
                {
                    // This route is targeted
                    if (routeSchedule.StartTime.TimeOfDay == specialSchedule.StartTime.TimeOfDay &&
                        routeSchedule.EndTime.TimeOfDay == specialSchedule.EndTime.TimeOfDay)
                    {
                        // Exact overlap - replace with new / cancel all
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
                                keepFixedRoute = false;
                            }
                            if (routeSchedule.EndTime.TimeOfDay > specialSchedule.EndTime.TimeOfDay)
                            {
                                // End part of orignal schedule is to be retained
                                var newSchedule = new RouteSchedule(routeSchedule);
                                newSchedule.StartTime = specialSchedule.EndTime;
                                schedules.Add(newSchedule);
                                keepFixedRoute = false;
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