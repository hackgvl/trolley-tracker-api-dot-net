var showSchedules = false;
var oMap; // map object
var currentPosMarker; //user location via browser get location
var routes = []; //array of all routes
var stopMarkers = {}; // Active stop markers (for all routes)
var routeDisplay = []; //All displayed routes
var DefaultRouteOpacity = 0.7;
var DefaultArrowOpacity = 0.7;
var updateCount = 0;

//Specify your Mapbox API access token
L.MakiMarkers.accessToken = "pk.eyJ1IjoiYmlrZW9pZCIsImEiOiJTSW9oVHA0In0.4xG7icLNIAIArqh6xGpOOg";


//custom control
var oMapControl = L.Control.extend({
    options: {
      position: 'topright'
    },
    onAdd: function (map) {
      // create the control container with a particular class name
      var container = L.DomUtil.create('div', 'my-custom-control');
      // ... initialize other DOM elements, add listeners, etc.
      return container;
    }
  });

var oScheduleControl = L.Control.extend({
    options: {
      position: 'bottomleft'
    },
    onAdd: function (map) {
      // create the control container with a particular class name
      var container = L.DomUtil.create('div', 'schedulecontrol');
      // ... initialize other DOM elements, add listeners, etc.
      return container;
    }
});

var oBackControl = L.Control.extend({
    options: {
      position: 'bottomright'
    },
    onAdd: function (map) {
      // create the control container with a particular class name
      var container = L.DomUtil.create('div', 'backcontrol');
      // ... initialize other DOM elements, add listeners, etc.
      return container;
    }
});

function GetStopIcon(markerColor) {
	var markerIcon = L.MakiMarkers.icon({ icon: "embassy", color: markerColor, size: "s" });
	return markerIcon;
}



function initMap(data){

  //console.log("lat: " + data.lat, "lng: " + data.lng);
  window.oMap = L.map('map', {
    scrollWheelZoom: true
  }).setView([data.lat, data.lng], 15);
  L.AwesomeMarkers.Icon.prototype.options.prefix = 'fa';
  L.tileLayer('https://api.mapbox.com/styles/v1/linktheoriginal/ciom3jx8k0006bolzuqwm7o3m/tiles/{z}/{x}/{y}?access_token=pk.eyJ1IjoibGlua3RoZW9yaWdpbmFsIiwiYSI6IjFjODFkODU1NGVkNWJhODQ2MTk5ZTk0OTVjNWYyZDE0In0.ptQUIfB07dQrUwDM2uMgUw', {
    maxZoom: 18,
    tileSize: 512,
    zoomOffset: -1,
    id: 'examples.map-i875mjb7'
  }).addTo(oMap);



  oMap.addControl(new oMapControl());
  jQuery('.my-custom-control').append(jQuery('#controls').clone());

  oMap.addControl(new oScheduleControl());
  jQuery('.schedulecontrol').append(jQuery('#schedule').clone());

  oMap.addControl(new oBackControl());
  jQuery('.backcontrol').append(jQuery('#back').clone());
}




function buildRoute(data, route_name, color, route) {
  //data is an array of lat/lon objects
  //[{lat:1, lon:1}, {lat:2, lon:2}, ...]

  var pointList = [];
  data.forEach(function(loc, index, array){
    pointList.push(new L.LatLng(loc.Lat, loc.Lon));
  });

  var routePolyLine = new L.Polyline(pointList, {
    color: color,
    weight: 3,
    opacity: DefaultRouteOpacity,
    smoothFactor: 1
  });

  //use the settext plugin to add directional arrows to the route.
  routePolyLine.setText('  ►  ', {repeat: true, attributes: {fill: color, opacity:DefaultArrowOpacity}});

  //store the new polyline in the routes object
  routes.push(routePolyLine);

  var visibleRoute = {};
  visibleRoute.Line = routePolyLine;
  visibleRoute.Route = route;
  routeDisplay.push(visibleRoute);
}

function buildStops(stoplocs, color) {
	stoplocs.forEach(function(stop, index, array) {
		//var stopMarker = L.divIcon({className: "trolley-stop-icon " + color.css});
		var stopMarker = GetStopIcon("#000000");
		
		var oMapMarker = L.marker([stop.Lat, stop.Lon], {
			icon: stopMarker
		});

		oMapMarker.Name = StopTitle(stop); // stop.Name;
		oMapMarker.Stop = stop;
		oMapMarker.StopImageURL = stop.StopImageURL;

		bStopExists = false;

		// Could be used to identify stops used on multiple routes if there was a unique way to display them
		//stopMarkers.forEach(function(testStop, existindex, existarray) {
		//  if (testStop.ID == stop.ID) {
			//then this stop is on two routes.  relying on setting the color name as the LAST class argument.  (first is trolley-stop-icon)
			//this will build color strings of all colors to show (red-green-blue if it's on three routes initialized in that order)
			//existloc.options.icon.options.className = existloc.options.icon.options.className + "-" + color.css;
		//  }
		//});

		stopMarkers[stop.ID] = oMapMarker;
	});
}

function StopTitle(stop) {
	var newTitle = stop.Name;
//  var currentMilliseconds = new Date().getTime();
//  var arrivalTimes = stop.NextTrolleyArrivalTime;
//  for (var trolleyNumber in arrivalTimes) {
//  	var arrivalTime = arrivalTimes[trolleyNumber];
//  	var arrivalDate = new Date(arrivalTime);
//  	var minutesToArrival = Math.floor((arrivalDate.getTime() - currentMilliseconds) / 60 / 1000);
//  	if (minutesToArrival >= 0) {
//  		newTitle +=  "<br>Trolley " + trolleyNumber + "  arriving in " + minutesToArrival + " minutes <br> at " + arrivalDate.toLocaleTimeString();
//  	} else {
//  		// Stale / non-updated arrivalDate in stop
//  	}
//  }
	return newTitle;
}


function addStops() {
	for (var stopID in stopMarkers) {
		var stopMarker = stopMarkers[stopID];
		var sImageHTML = "";
		//the api currently has this set on some of the stops, but most are empty
		/*if (loc.StopImageURL != null) {
		  sImageHTML = "<br><img src='" + loc.StopImageURL + "'/>";
		}*/
		stopMarker.addTo(oMap).bindPopup("<p><b>" + stopMarker.Name + "</b>" + sImageHTML + "</p>");
  }
}

function addRoutes() {
  routes.forEach(function(route, routeIndex){
    route.addTo(oMap);
  });
}

function hideStops(map, hideStops) {
	//this is currently not used - left in for future use
	for (var stop in hideStops) {
		map.removeLayer(hideStops[stop]);			
	}
}

function hideTrolleyPaths(map) {
	routeDisplay.forEach(function(route, index, array) {
		if (route) {
			map.removeLayer(routes['route_' + index]);
		}
	});
}


function closeInfo() {
  if (showSchedules) {
    jQuery('#schedules').show();
  }
  jQuery('#info').hide(); 
  jQuery('#info-question').show();
  jQuery('#info-schedule').show();
}

function showInfo() {
  if (showSchedules) {
    jQuery('#schedules').hide();
  }
  jQuery('#info').show();
  jQuery('#info-question').hide();
  jQuery('#info-schedule').hide();
}

function closeSchedule() {
  jQuery('#schedules').hide();
  jQuery('#info-question').show();
  jQuery('#info-schedule').show();
}

function showSchedule() {
  jQuery('#schedules').show();
  jQuery('#info-question').hide();
  jQuery('#info-schedule').hide();
}

function addViewingRoute() {
  jQuery('#schedule_name').html('<b>Viewing Route:</b> <br>' + routedata[0].LongName);
}

function addViewingSchedule() {
  jQuery('#runs_when').html('<b>Runs on:</b><br>' + runson.join('<br>'));
}

(function($){
  scheduledata.forEach(function(schedule, scheduleIndex){
        
	  let builder = '<span style="background-color: ' + schedule.RouteColorRGB + '"></span>' +
			 '<div><h6><a href="' + schedule.RouteURL + '">' + schedule.RouteName + '</s></h6>' +
			 "<div class='hr' style='background-color:" + schedule.RouteColorRGB + "'></div>" +
			 "<p>" + schedule.DayOfWeek + " " + schedule.StartTime + " - " + schedule.EndTime + "</p></div>";

	  $("<div class='scheduleRoutes'>" + builder + "</div>").insertAfter("#scheduletitle");
       
       builder = "";
  });

  //for debugging no trolley behavior while trolleys are running
  //routedata= [];
  
  if (routedata.length == 0) {
    showSchedules = true;
    showSchedule();
  } else {
    routedata.forEach(function(route, routeIndex) {
      buildRoute(route.RouteShape, "route_" + routeIndex, route.RouteColorRGB, route);
    });
  }

  routedata.forEach(function(route, routeIndex) {
    buildStops(route.Stops, route.RouteColorRGB);
  });

  initMap({lat: 34.852432, lng: -82.398216});

  addRoutes();
  addStops();

  addViewingRoute();
  addViewingSchedule();

})(jQuery);