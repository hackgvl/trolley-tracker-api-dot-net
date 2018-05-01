using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TrolleyTracker.Models;

namespace TrolleyTracker.ViewModels
{
    /// <summary>
    /// Class to cache active trolley info and for structure to
    /// return as JSON
    /// </summary>
    [DataContract(Name = "RunningTrolley")]
    public class RunningTrolley
    {
        public RunningTrolley(Trolley trolley)
        {
            ID = trolley.ID;
            // lat, lon = 0,0 if no values
            if (trolley.CurrentLat.HasValue)
            {
                Lat = (double)trolley.CurrentLat;
            }
            if (trolley.CurrentLon.HasValue)
            {
                Lon = (double)trolley.CurrentLon;
            }
            LastUpdated = DateTime.Now;

        }

        [DataMember(Name = "ID")]
        public int ID { get; set; }
        [DataMember(Name = "Lat")]
        public double Lat { get; set; }
        [DataMember(Name = "Lon")]
        public double Lon { get; set; }
        [DataMember(Name = "Capacity")]
        public int Capacity { get; set; }
        [DataMember(Name = "PassengerLoad")]
        public double PassengerLoad { get; set; }

        [NonSerialized]
        public DateTime LastUpdated;


    }
}