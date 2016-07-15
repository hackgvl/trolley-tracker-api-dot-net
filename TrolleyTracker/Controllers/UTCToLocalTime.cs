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
            // For other time zones, obtain Windows Zone Id string from registry 
            // using method at http://stackoverflow.com/questions/14149346/what-value-should-i-pass-into-timezoneinfo-findsystemtimezonebyidstring ,
            var myTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");  // For Greenville, SC
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, myTimeZone);  // This step applies Daylight Saving Time, when applicable
        }
    }
}