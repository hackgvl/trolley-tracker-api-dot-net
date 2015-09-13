using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrolleyTracker.Controllers
{
    // NodaTime could be a better way https://github.com/nodatime/nodatime

    public class UTCToLocalTime
    {
        public static DateTime LocalTimeFromUTC(DateTime utcTime)
        {
            var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");  // For Greenville, SC
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, myTimeZone);
        }
    }
}