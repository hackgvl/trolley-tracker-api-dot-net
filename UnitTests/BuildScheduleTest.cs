using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrolleyTracker.Controllers;
using System.Collections.Generic;
using TrolleyTracker.Models;

namespace UnitTests
{
    [TestClass]
    public class BuildScheduleTest
    {

        public static DateTime ScheduleTime(int hour, int minute, int second)
        {
            return new DateTime(1970, 1, 1, hour, minute, second);
        }


        private static RouteSchedule InitialSchedule(int fixedRouteStartHour, int fixedRouteEndHour,
                                                     int overrideStartHour, int overrideEndHour,
                                                     out List<RouteScheduleOverride> routeScheduleOverrides)
        {
            routeScheduleOverrides = new List<RouteScheduleOverride>();

            var schedule = new RouteSchedule();
            var route = new Route();
            schedule.Route = route;


            schedule.StartTime = ScheduleTime(fixedRouteStartHour, 0, 0);
            schedule.EndTime = ScheduleTime(fixedRouteEndHour, 0, 0);

            var scheduleOverride = new RouteScheduleOverride();
            scheduleOverride.OverrideType = RouteScheduleOverride.OverrideRule.NoService;  // Result will be same for either NoService or Replace
            scheduleOverride.StartTime = ScheduleTime(overrideStartHour, 0, 0);
            scheduleOverride.EndTime = ScheduleTime(overrideEndHour, 0, 0);
            routeScheduleOverrides.Add(scheduleOverride);

            return schedule;
        }


        [TestMethod]
        public void TestRegularSchedule()
        {

            List<RouteScheduleOverride> routeScheduleOverrides;
            var schedule = InitialSchedule(6, 10, 12, 14, out routeScheduleOverrides);  // Non-overlapping times (No effect on fixed schedule)

            var realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(1, realSchedules.Count);

            Assert.AreEqual(realSchedules[0].StartTime, schedule.StartTime);
            Assert.AreEqual(realSchedules[0].EndTime, schedule.EndTime);
        }

        /// <summary>
        /// Case where modified schedule removes original schedule exactly
        /// </summary>
        [TestMethod]
        public void TestCoincidentOverride()
        {

            List<RouteScheduleOverride> routeScheduleOverrides;
            var schedule = InitialSchedule(6, 10, 6, 10, out routeScheduleOverrides);

            var realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(0, realSchedules.Count);  // Original schedule completely removed

        }

        /// <summary>
        /// Case where modified schedule begins before fixed schedule, and modifies
        /// </summary>
        [TestMethod]
        public void TestLeadingOverride()
        {

            List<RouteScheduleOverride> routeScheduleOverrides;
            var schedule = InitialSchedule(8, 10, 6, 10, out routeScheduleOverrides);  // Override starts early, Both end at same time

            var realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(0, realSchedules.Count);  // Original schedule completely removed

            routeScheduleOverrides[0].EndTime = ScheduleTime(9, 0, 0);  // Override ends early, leaving 1 hour of original schedule
            realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(1, realSchedules.Count);

            Assert.AreEqual(ScheduleTime(9, 0, 0), realSchedules[0].StartTime);
            Assert.AreEqual(schedule.EndTime, realSchedules[0].EndTime);

            routeScheduleOverrides[0].EndTime = ScheduleTime(12, 0, 0);  // Override ends past original schedule, cancelling it all
            realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(0, realSchedules.Count);

        }

        /// <summary>
        /// Case where modified schedule begins after fixed schedule, and modifies
        /// </summary>
        [TestMethod]
        public void TestTrailingOverride()
        {

            List<RouteScheduleOverride> routeScheduleOverrides;
            var schedule = InitialSchedule(8, 12, 9, 12, out routeScheduleOverrides);  // Override starts later, Both end at same time, leaving first part of original schedule

            var realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(1, realSchedules.Count);

            Assert.AreEqual(schedule.StartTime, realSchedules[0].StartTime);
            Assert.AreEqual(ScheduleTime(9, 0, 0), realSchedules[0].EndTime);


            routeScheduleOverrides[0].EndTime = ScheduleTime(13, 0, 0);  // Override ends past fixed schedule leaving the first part of original schedule
            realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(1, realSchedules.Count);

            Assert.AreEqual(schedule.StartTime, realSchedules[0].StartTime);
            Assert.AreEqual(ScheduleTime(9, 0, 0), realSchedules[0].EndTime);

            routeScheduleOverrides[0].EndTime = ScheduleTime(10, 0, 0);  // Override ends in middle of original schedule, leaving first and last segment
            realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(2, realSchedules.Count);

            Assert.AreEqual(schedule.StartTime, realSchedules[0].StartTime);
            Assert.AreEqual(ScheduleTime(9, 0, 0), realSchedules[0].EndTime);

            Assert.AreEqual(ScheduleTime(10, 0, 0), realSchedules[1].StartTime);
            Assert.AreEqual(schedule.EndTime, realSchedules[1].EndTime);


        }



        /// <summary>
        /// Case with multiple modified schedules breaking up fixed schedule
        /// </summary>
        [TestMethod]
        public void TestMultipleOverride()
        {

            List<RouteScheduleOverride> routeScheduleOverrides;
            var schedule = InitialSchedule(6, 23, 9, 12, out routeScheduleOverrides);  // Override starts later, Both end at same time, leaving first part of original schedule


            // Create second override
            var scheduleOverride = new RouteScheduleOverride();
            scheduleOverride.OverrideType = RouteScheduleOverride.OverrideRule.NoService;  // Result will be same for either NoService or Replace
            scheduleOverride.StartTime = ScheduleTime(15, 0, 0);
            scheduleOverride.EndTime = ScheduleTime(16, 0, 0);
            routeScheduleOverrides.Add(scheduleOverride);

            var realSchedules = BuildScheduleView.ModifyRouteScheduleForOverlap(schedule, routeScheduleOverrides);
            Assert.AreEqual(3, realSchedules.Count);

            // Leaves segments 6:00-9:00, 12:00-15:00, 16:00-23:00

            Assert.AreEqual(schedule.StartTime, realSchedules[0].StartTime);
            Assert.AreEqual(ScheduleTime(9, 0, 0), realSchedules[0].EndTime);

            Assert.AreEqual(ScheduleTime(12, 0, 0), realSchedules[1].StartTime);
            Assert.AreEqual(ScheduleTime(15, 0, 0), realSchedules[1].EndTime);

            Assert.AreEqual(ScheduleTime(16, 0, 0), realSchedules[2].StartTime);
            Assert.AreEqual(ScheduleTime(23, 0, 0), realSchedules[2].EndTime);


        }






    }
}
