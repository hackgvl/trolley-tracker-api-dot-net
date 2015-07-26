using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrolleyTracker.Models;
using MvcSchedule.Objects;

namespace TrolleyTracker.ViewModels
{
    public class RouteScheduleViewModel
    {
        public List<string> DaysOfWeek { get; set; } = new List<string> { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };


        public RouteScheduleViewModel()
        {
            //RouteSchedules = new IOrderedQueryable<RouteSchedule>();
            Options = new MvcScheduleGeneralOptions();
        }

        public System.Data.Entity.Infrastructure.DbQuery<RouteSchedule> RouteSchedules { get; set; }
        public MvcScheduleGeneralOptions Options { get; set; }

    }
}