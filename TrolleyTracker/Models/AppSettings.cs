using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrolleyTracker.Models
{
    /// <summary>
    /// Single instance of this record in database to control application
    /// runtime settings
    /// </summary>
    public partial class AppSettings
    {
        public int ID { get; set; }
        /// <summary>
        /// Whether to use Syncromatics or Beacon interface
        /// </summary>
        public bool UseSyncromatics { get; set; }
    }
}