using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrolleyTracker.Models;
using TrolleyTracker.Controllers;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class RouteKMLTest
    {
        private Stream GetMemoryStream(string strSource)
        {
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(strSource));
        }

        [TestMethod]
        public void TestKMLGood()
        {
            var routeData = new ParseKML(GetMemoryStream(GoodKML));
            Assert.AreEqual(4, routeData.RouteShape.Count);
            Assert.AreEqual(2, routeData.RouteStops.Count);
        }

        [TestMethod]
        public void TestKMLGood2_2()
        {
            var routeData = new ParseKML(GetMemoryStream(KML2_2_Sample));
            Assert.AreEqual(4, routeData.RouteShape.Count);
            Assert.AreEqual(0, routeData.RouteStops.Count);
        }


        [TestMethod]
        [ExpectedException(typeof(KMLParseException))]
        public void TestKMLTooFewRoutePoints()
        {
            var routeData = new ParseKML(GetMemoryStream(KML_TooFewPoints));
        }


        [TestMethod]
        [ExpectedException(typeof(KMLParseException))]
        public void TestKMLExtraRouteSegment()
        {
            var routeData = new ParseKML(GetMemoryStream(KMLMultiSegment));
        }

        [TestMethod]
        [ExpectedException(typeof(KMLParseException))]
        public void TestKMLExtraRoute()
        {
            // Each KML file should contain just one route
            var routeData = new ParseKML(GetMemoryStream(KML_ExtraRoute));
        }



        private string GoodKML = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns = ""http://www.opengis.net/kml/2.2"">
  <Folder>
    <name>Augusta </name>
    <Placemark>
      <name>Augusta - inbound </name>
      <LineString>
        <coordinates>-82.4026,34.84573 -82.4027,34.84573 -82.4028,34.84573 -82.4026,34.84573</coordinates>
      </LineString>
    </Placemark>
    <Placemark>
      <name>remix-160735536 - Falls Park Drive</name>
      <Point>
        <coordinates>-82.40255981683733,34.8457675640172</coordinates>
      </Point>
    </Placemark>
    <Placemark>
      <name>remix-160735537 - Augusta St &amp; University St</name>
      <Point>
        <coordinates>-82.40573287010194,34.842996118001544</coordinates>
      </Point>
    </Placemark>
  </Folder>
</kml>";

        private string KML_TooFewPoints = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns = ""http://www.opengis.net/kml/2.2"">
  <Folder>
    <name>Augusta </name>
    <Placemark>
      <name>Augusta - inbound </name>
      <LineString>
        <coordinates>-82.4026,34.84573 -82.4027,34.84573</coordinates>
      </LineString>
    </Placemark>
    <Placemark>
      <name>remix-160735536 - Falls Park Drive</name>
      <Point>
        <coordinates>-82.40255981683733,34.8457675640172</coordinates>
      </Point>
    </Placemark>
    <Placemark>
      <name>remix-160735537 - Augusta St &amp; University St</name>
      <Point>
        <coordinates>-82.40573287010194,34.842996118001544</coordinates>
      </Point>
    </Placemark>
  </Folder>
</kml>";


        private string KMLMultiSegment = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns = ""http://www.opengis.net/kml/2.2"">
  <Folder>
    <name>Augusta </name>
    <Placemark>
      <name>Augusta - inbound </name>
      <LineString>
        <coordinates>-82.4026,34.84573 -82.4027,34.84573 -82.4028,34.84573 -82.4026,34.84573</coordinates>
      </LineString>
      <LineString>
        <coordinates>-82.4026,34.84573 -82.4027,34.84573 -82.4028,34.84573 -82.4026,34.84573</coordinates>
      </LineString>
    </Placemark>
    <Placemark>
      <name>remix-160735536 - Falls Park Drive</name>
      <Point>
        <coordinates>-82.40255981683733,34.8457675640172</coordinates>
      </Point>
    </Placemark>
    <Placemark>
      <name>remix-160735537 - Augusta St &amp; University St</name>
      <Point>
        <coordinates>-82.40573287010194,34.842996118001544</coordinates>
      </Point>
    </Placemark>
  </Folder>
</kml>";
        private string KML_ExtraRoute = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns = ""http://www.opengis.net/kml/2.2"">
  <Folder>
    <name>Augusta </name>
    <Placemark>
      <name>Augusta - inbound </name>
      <LineString>
        <coordinates>-82.4026,34.84573 -82.4027,34.84573 -82.4028,34.84573 -82.4026,34.84573</coordinates>
      </LineString>
    </Placemark>
    <Placemark>
      <name>remix-160735536 - Falls Park Drive</name>
      <Point>
        <coordinates>-82.40255981683733,34.8457675640172</coordinates>
      </Point>
    </Placemark>
    <Placemark>
      <name>remix-160735537 - Augusta St &amp; University St</name>
      <Point>
        <coordinates>-82.40573287010194,34.842996118001544</coordinates>
      </Point>
    </Placemark>
  </Folder>
  <Folder>
    <name>Arts West </name>
    <Placemark>
      <name>Arts West - inbound </name>
      <LineString>
        <coordinates>-82.4026,34.84573 -82.4027,34.84573 -82.4028,34.84573 -82.4026,34.84573</coordinates>
      </LineString>
    </Placemark>
    <Placemark>
      <name>remix-160735536 - Pendleton St</name>
      <Point>
        <coordinates>-82.40255981683733,34.8457675640172</coordinates>
      </Point>
    </Placemark>
    <Placemark>
      <name>remix-160735537 - Draper St</name>
      <Point>
        <coordinates>-82.40573287010194,34.842996118001544</coordinates>
      </Point>
    </Placemark>
  </Folder>
</kml>";

        private string KML2_2_Sample = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
  <Document>
    <name>West Greenville</name>
    <Style id=""line-1267FF-5000-nodesc-normal"">
      <LineStyle>
        <color>ffff6712</color>
        <width>5</width>
      </LineStyle>
      <BalloonStyle>
        <text><![CDATA[<h3>$[name]</h3>]]></text>
      </BalloonStyle>
    </Style>
    <Style id=""line-1267FF-5000-nodesc-highlight"">
      <LineStyle>
        <color>ffff6712</color>
        <width>7.5</width>
      </LineStyle>
      <BalloonStyle>
        <text><![CDATA[<h3>$[name]</h3>]]></text>
      </BalloonStyle>
    </Style>
    <StyleMap id=""line-1267FF-5000-nodesc"">
      <Pair>
        <key>normal</key>
        <styleUrl>#line-1267FF-5000-nodesc-normal</styleUrl>
      </Pair>
      <Pair>
        <key>highlight</key>
        <styleUrl>#line-1267FF-5000-nodesc-highlight</styleUrl>
      </Pair>
    </StyleMap>
    <Placemark>
      <name>Directions from 129-157 W Washington St, Greenville, SC 29601, USA to 128-156 W Washington St, Greenville, SC 29601, USA</name>
      <styleUrl>#line-1267FF-5000-nodesc</styleUrl>
      <LineString>
        <tessellate>1</tessellate>
        <coordinates>
          -82.4003,34.85116,0
          -82.40017,34.85112,0
          -82.39992,34.85105,0
          -82.4003,34.85116,0
        </coordinates>
      </LineString>
    </Placemark>
  </Document>
</kml>";


    }
}
