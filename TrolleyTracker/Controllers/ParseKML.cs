using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using TrolleyTracker.Models;
using System.Text.RegularExpressions;

namespace TrolleyTracker.Controllers
{
    /// <summary>
    /// Manual KML parsing - should reasonably recognize KML 2.1 or 2.2.  
    /// Prepackaged NuGet KML parsers are more strict.   This just validates
    /// attributes important to this application
    /// </summary>
    public class ParseKML
    {
        public List<Coordinate> RouteShape { get; set; }

        /// <summary>
        /// Stops found in KML for this route
        /// </summary>
        public List<Stop> RouteStops { get; set; }

        /// <summary>
        /// Parses KML from stream, fills RouteShape, RouteStops properties on success
        /// </summary>
        /// <param name="kmlStream"></param>
        public ParseKML(Stream kmlStream)
        {
            var kmlDoc = new XmlDocument();
            kmlDoc.Load(kmlStream);

            var rootNode = kmlDoc.DocumentElement;

            // Expected count:
            //     1         n             1
            // /folder / placemark / [coordinates]
            //  Placemarks may contain the single route path or multiple as stops
            var folders = rootNode.GetElementsByTagName("Folder");
            if (folders.Count == 0)
            {
                // Try KML 2.2 style Document container tag
                folders = rootNode.GetElementsByTagName("Document");
            }
            if (folders.Count != 1)
            {
                throw new KMLParseException($"Unrecognized KML format: Found {folders.Count} folders or documents, expected 1 ");
            }

            int routePathCount = 0;
            RouteShape = new List<Coordinate>();
            RouteStops = new List<Stop>();
            var mainFolder = folders[0];

            foreach (XmlNode folderElement in mainFolder.ChildNodes)
            {
                switch (folderElement.Name)
                {
                    case "name":
                        var strFolderName = folderElement.InnerText;  // Not currently used
                        break;
                    case "Placemark":

                        ParsePlacemark(folderElement, ref routePathCount);
                        break;
                }
            }

            if (routePathCount != 1 || RouteShape == null)
            {
                throw new KMLParseException($"Unrecognized KML format: Found {routePathCount} route paths, expect a single closed route path");
            }
            else if (RouteShape.Count < 3)
            {
                throw new KMLParseException($"Invalid KML route path: Found only {RouteShape.Count} points, expect at least 3");
            }
            else if (!CloseEnough(RouteShape[0], RouteShape[RouteShape.Count-1]))
            {
                throw new KMLParseException($"Unclosed KML route loop: expecting last point to end at beginning");
            }

        }

        private void ParsePlacemark(XmlNode placemark, ref int pathCount)
        {

            var stopName = "";  // If this is a stop
            foreach (XmlNode placemarkELemnent in placemark.ChildNodes)
            {
                switch (placemarkELemnent.Name)
                {
                    case "name":
                        stopName = placemarkELemnent.InnerText;
                        // Remove tool ID from the name "remix-160735536 - Falls Park Drive"
                        // Theoretically, this could be used to keep things in sync in the future
                        stopName = Regex.Replace(stopName, @"remix-\d*\s*-\s*", "");
                        break;
                    case "LineString":
                        ParseRoutePath(placemarkELemnent, ref pathCount);
                        break;
                    case "Point":
                        var stop = new Stop();
                        stop.Name = stopName;
                        ParseStop(placemarkELemnent, stop);
                        RouteStops.Add(stop);
                        break;
                }
            }


        }

        private void ParseStop(XmlNode placemarkELemnent, Stop stop)
        {
            foreach (XmlNode pointELemnent in placemarkELemnent.ChildNodes)
            {
                switch (pointELemnent.Name)
                {
                    case "coordinates":
                        var strCoordinate = pointELemnent.InnerText;
                        var strLonLat = strCoordinate.Split(',');
                        stop.Lon = Convert.ToDouble(strLonLat[0]);
                        stop.Lat = Convert.ToDouble(strLonLat[1]);
                        break;
                }
            }
        }

        private void ParseRoutePath(XmlNode placemarkELemnent, ref int pathCount)
        {

            foreach (XmlNode coordELemnent in placemarkELemnent.ChildNodes)
            {
                switch (coordELemnent.Name)
                {
                    case "coordinates":
                        var strCoordinates = coordELemnent.InnerText;
                        // Remove all possible extra spaces and new lines
                        strCoordinates = strCoordinates.Replace("\n", "");
                        strCoordinates = Regex.Replace(strCoordinates, @"\s+", " "); // Collapse multiple whitespaces into one
                        strCoordinates = strCoordinates.Trim();

                        var strPairs = strCoordinates.Split(' ');
                        Coordinate lastCoordinate = null;
                        foreach (var strPair in strPairs)
                        {
                            // An optional elevation may be included but ignored here
                            var strLonLat = strPair.Split(',');
                            var coordinate = new Coordinate(
                                Convert.ToDouble(strLonLat[1]),
                                Convert.ToDouble(strLonLat[0]));

                            // Check for and discard consecutive duplicate points
                            bool wasDuplicate = false;
                            if (lastCoordinate != null)
                            {
                                if (coordinate.Lat == lastCoordinate.Lat && coordinate.Lon == lastCoordinate.Lon)
                                {
                                    wasDuplicate = true;
                                }
                            }

                            if (!wasDuplicate)
                            {
                                RouteShape.Add(coordinate);
                            }

                            lastCoordinate = coordinate;
                        }
                        pathCount++;
                        break;
                }
            }


        }

        /// <summary>
        /// Test whether coordinate values are within a close enough distance to each other.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name=""></param>
        /// <returns></returns>
        private bool CloseEnough(Coordinate c1, Coordinate c2)
        {
            // Some generating programs don't snap them together.

            double Epsilon = 5.0;   // Max allowable mismatch, in meters

            var distance = c1.GreatCircleDistance(c2);

            return (distance < Epsilon);
        }


    }


    /// <summary>
    /// Exception type for message to be shown to end user in the hope
    /// that they can self-correct any possible file format errors
    /// </summary>
    public class KMLParseException : Exception
    {
        public KMLParseException(string errorMessage)
        {
            this.ParseErrorMessage = errorMessage;
        }
        public string ParseErrorMessage { get; set; }
    }
}