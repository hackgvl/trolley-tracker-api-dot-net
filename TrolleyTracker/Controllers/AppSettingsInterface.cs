using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TrolleyTracker.Models;

namespace TrolleyTracker.Controllers
{
    /// <summary>
    /// Provide runtime access to application settings withouth needing to poll
    /// from the database each time.
    /// </summary>
    public static class AppSettingsInterface
    {
        // Anything more complicated than a boolean may need lock protection
        public static bool UseSyncromatics { get; set; }


        public static void LoadAppSettings()
        {
            using (var db = new TrolleyTrackerContext())
            {
                var appSettings = (from a in db.AppSettings select a).FirstOrDefault();
                if (appSettings != null) UpdateSettings(appSettings);

            }
        }

        public static void UpdateSettings(AppSettings appSettings)
        {
            UseSyncromatics = appSettings.UseSyncromatics;
        }


    }
}