using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrolleyTracker.Controllers
{
    public class Coordinate
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public int StopID { get; set; }

        public Coordinate(double lat, double lon)
        {
            StopID = -1; // Unassigned
            Lat = lat;
            Lon = lon;
        }
        public Coordinate(string lat, string lon)
        {
            StopID = -1; // Unassigned
            Lat = Convert.ToDouble(lat);
            Lon = Convert.ToDouble(lon);
        }

        public double DegreeToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }


        public double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }


        /// <summary>
        /// Computes the distance between this coordinate and another point on the earth.
        /// Uses spherical law of cosines formula, not Haversine.
        /// </summary>
        /// <param name="other">The other point</param>
        /// <returns>Distance in meters</returns>
        public double GreatCircleDistance(Coordinate other)
        {
            var epsilon = Math.Abs(other.Lon - Lon) + Math.Abs(other.Lat - Lat);
            if (epsilon < 1.0e-6) return 0.0;

            double meters = (Math.Acos(
                    Math.Sin(DegreeToRadians(Lat)) * Math.Sin(DegreeToRadians(other.Lat)) +
                    Math.Cos(DegreeToRadians(Lat)) * Math.Cos(DegreeToRadians(other.Lat)) *
                    Math.Cos(DegreeToRadians(other.Lon - Lon))) * 6378135);

            return (meters);
        }

    }
}