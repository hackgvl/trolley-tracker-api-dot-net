var showSchedules = false;
var oMap; // map object
var currentPosMarker; //user location via browser get location
var routes = []; //array of all routes
var stopMarkers = {}; // Active stop markers (for all routes)
var trolleys = {}; //dictionary of trolleys and markers
var fulltrolleydatabyid = {}; //dictionary of full trolley info by ID
var routeDisplay = []; //All displayed routes
var checkTimer; //Timer object to check for trolley location updates
var DefaultRouteOpacity = 0.5;
var DefaultArrowOpacity = 0.7;
var updateCount = 0;

//Specify your Mapbox API access token
L.MakiMarkers.accessToken = "pk.eyJ1IjoiYmlrZW9pZCIsImEiOiJTSW9oVHA0In0.4xG7icLNIAIArqh6xGpOOg";

var PushPinIcon = L.Icon.extend({
    options: {
        iconUrl: '../content/images/pin-black-tiny-border.png',
        shadowUrl: '../content/images/pin-black-tiny-shadow.png',
        iconSize: [8, 15],
        shadowSize: [10, 13],
        iconAnchor: [4, 14],
        shadowAnchor: [1, 12],
        popupAnchor: [-3, -12]
    }
});


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


function GetBusIcon(markerColor) {
    var markerIcon = L.MakiMarkers.icon({ icon: "bus", color: markerColor, size: "m" });
    return markerIcon;
}

function GetStopIcon(markerColor) {
    //var markerIcon = L.MakiMarkers.icon({ icon: "embassy", color: markerColor, size: "s" });
    var markerIcon = new PushPinIcon();
    return markerIcon;
}


function initMap(data) {

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


    window.oMap.on('click', onClickMap);

    getTrolleyLocations(oMap);
    getUserLocation(oMap);

    oMap.addControl(new oMapControl());
    jQuery('.my-custom-control').append(jQuery('#controls').clone());
}

// Organize fulltrolleydata array into dictionary
function setTrolleyInfo() {
    fulltrolleydata.forEach(function (trolley) {
        fulltrolleydatabyid[trolley.ID] = trolley;
    });
}

L.Map = L.Map.extend({
    openPopup: function (popup, latlng, options) {
        return;

        if (!(popup instanceof Popup)) {
            popup = new Popup(options).setContent(popup);
        }

        if (latlng) {
            popup.setLatLng(latlng);
        }

        if (this.hasLayer(popup)) {
            return this;
        }

        if (this._popup && this._popup.options.autoClose) {
            // NOTE THIS LINE : COMMENTING OUT THE CLOSEPOPUP CALL
            //this.closePopup(); 
        }

        this._popup = popup;
        return this.addLayer(popup);
    }
});


function getTrolleyLocations(map) {
    var $ = jQuery;
    clearTimeout(checkTimer);
    //$('#getupdate').show();
    $.ajax({
        url: '/api/v1/Trolleys/Running',
        //url: 'http://localhost:51304/api/v1/Trolleys/Running',
        type: 'get',
        dataType: 'json',
        cache: false,
        crossDomain: true,
        success: function (trolleydata) {
            //console.log("success!", data);
            trolleydata.forEach(function (data) {
                var popupText = "<p>Last Seen: <b>" + moment(data.LastBeaconTime).fromNow() + "</b></p>";

                // create a marker for new trolleys, or update info on existing maker
                if (!trolleys[data.ID]) {
                    var trolleyColor = fulltrolleydatabyid[data.ID].IconColorRGB;
                    var trolleyIcon = GetBusIcon(trolleyColor);

                    var oMapMarker = L.marker([data.Lat, data.Lon], {
                        icon: trolleyIcon
                    }).addTo(oMap).bindPopup(popupText).on('click', onClickTrolley);

                    //add the data to the trolleys object
                    trolleys[data.ID] = {
                        agentData: data,
                        mapMarker: oMapMarker
                    };
                    // Save for click event access
                    oMapMarker.trolley = fulltrolleydatabyid[data.ID];
                } else {
                    // existing trolley: update location
                    var trolley = trolleys[data.ID];
                    trolley.mapMarker
                        .setLatLng([data.Lat, data.Lon]);

                    var newTitle = fulltrolleydatabyid[data.ID].TrolleyName + " " + fulltrolleydatabyid[data.ID].Number + "<br/>" +
                        "Capacity: " + data.Capacity + ", " + (data.PassengerLoad * 100.0).toFixed(0) + "% loaded";
                    trolley.mapMarker.setPopupContent(newTitle);

                    //.setPopupContent(popupText);
                }
            });
        },
        complete: function (data) {
            updateCount++;
            if (updateCount % 12 === 0) {
                //Update stops for arrival time
                checkTimer = setTimeout(function () { getStops(oMap); }, 5000);
            } else {
                //set the next call for 5 seconds
                checkTimer = setTimeout(function () { getTrolleyLocations(oMap); }, 5000);
            }

        }
    });
}

function getStops(map) {
    var $ = jQuery;
    clearTimeout(checkTimer);
    $.ajax({
        url: '/api/v1/Stops',
        type: 'get',
        dataType: 'json',
        cache: false,
        crossDomain: true,
        success: function (stopdata) {
            stopdata.forEach(function (stop) {

                // create a marker for new trolleys, or update info on existing maker
                if (stopMarkers[stop.ID]) {
                    var stopMarker = stopMarkers[stop.ID];
                    var newTitle = StopTitleWithArrivalTime(stop);
                    stopMarker.setPopupContent(newTitle);
                }
            });
        },
        complete: function (data) {
            updateCount++;
            //set the next trolley call for 5 seconds
            checkTimer = setTimeout(function () { getTrolleyLocations(oMap); }, 5000);
        }
    });
}


// Dim all routes and trolleys other than current trolley and route
function onClickTrolley(e) {
    var trolley = e.target.trolley;
    for (var id in trolleys) {
        // There is a marker in running list
        var tr = fulltrolleydatabyid[id];
        var m = trolleys[id].mapMarker;
        var opacity = 1.0;
        if (trolley.ID != id) {
            opacity = 0.3;
        } else {
            m.openPopup();
        }
        m.setOpacity(opacity);
    }

    var color = trolley.IconColorRGB;

    routeDisplay.forEach(function (displayedRoute, routeIndex) {
        var routeOpacity = 1.0; // Max visibility
        if (displayedRoute.Route.RouteColorRGB !== color) {
            routeOpacity = 0.2;
        }
        displayedRoute.Line.setStyle({ opacity: routeOpacity });

        // Set arrows also, using undocumented hook into leaflet.textpath
        displayedRoute.Line._textOptions.attributes.opacity = routeOpacity;
        displayedRoute.Line._textRedraw();

    });
}

// Restore all route and trolley opacity
function onClickMap(e) {
    var trolley = e.target.trolley;
    for (var id in trolleys) {
        // There is a marker in running list
        var tr = fulltrolleydatabyid[id];
        var opacity = 1.0;
        var m = trolleys[id].mapMarker;
        m.setOpacity(opacity);
    }

    routeDisplay.forEach(function (displayedRoute, routeIndex) {
        displayedRoute.Line.setStyle({ opacity: DefaultRouteOpacity });

        // Reset arrows also, using undocumented hook into leaflet.textpath
        displayedRoute.Line._textOptions.attributes.opacity = DefaultArrowOpacity;
        displayedRoute.Line._textRedraw();

    });
}

function buildRoute(data, route_name, color, route) {
    //data is an array of lat/lon objects
    //[{lat:1, lon:1}, {lat:2, lon:2}, ...]

    var pointList = [];
    data.forEach(function (loc, index, array) {
        pointList.push(new L.LatLng(loc.Lat, loc.Lon));
    });

    var routePolyLine = new L.Polyline(pointList, {
        color: color,
        weight: 3,
        opacity: DefaultRouteOpacity,
        smoothFactor: 1
    });

    //use the settext plugin to add directional arrows to the route.
    routePolyLine.setText('  ►  ', { repeat: true, attributes: { fill: color, opacity: DefaultArrowOpacity } });

    //store the new polyline in the routes object
    routes.push(routePolyLine);

    var visibleRoute = {};
    visibleRoute.Line = routePolyLine;
    visibleRoute.Route = route;
    routeDisplay.push(visibleRoute);
}

function buildStops(stoplocs, color) {
    stoplocs.forEach(function (stop, index, array) {
        //var stopMarker = L.divIcon({className: "trolley-stop-icon " + color.css});
        var stopMarker = GetStopIcon("#000000");

        var oMapMarker = L.marker([stop.Lat, stop.Lon], {
            icon: stopMarker
        });

        oMapMarker.Name = StopTitleWithArrivalTime(stop); // stop.Name;
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

function StopTitleWithArrivalTime(stop) {
    var newTitle = stop.Name;
    var now = new Date();
    var currentMilliseconds = now.getTime();
    var arrivalTimes = stop.NextTrolleyArrivalTime;
    for (var trolleyNumber in arrivalTimes) {
        var arrivalTime = arrivalTimes[trolleyNumber] + 'Z';
        // Some browsers treat the string as local time without 'Z', Safari does not
        // Fake conversion to UTC for consistency across browsers, then adjust for time zone
        var arrivalMS = new Date(arrivalTime).getTime();
        arrivalMS += now.getTimezoneOffset() * 60000;
        var minutesToArrival = Math.floor((arrivalMS - currentMilliseconds) / 60 / 1000);
        if (minutesToArrival >= 0) {
            newTitle += "<br>Trolley " + trolleyNumber + "  arriving in " + minutesToArrival + " minutes <br> at " + new Date(arrivalMS).toLocaleTimeString();
        } else {
            // Stale / non-updated arrivalDate in stop
        }
    }
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
    routes.forEach(function (route, routeIndex) {
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
    routeDisplay.forEach(function (route, index, array) {
        if (route) {
            map.removeLayer(routes['route_' + index]);
        }
    });
}

/*
 * Attempt to geolocate the user through the browser, and, if 
 * successful, add a pin for the user's current location and
 * add a pin on it.  This only runs once on pageload.
 */
function getUserLocation(map) {
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {
            //console.log(position.coords.latitude + "," + position.coords.longitude);
            //oMap.setView([position.coords.latitude, position.coords.longitude], 16);
            var userPositionMarker = L.AwesomeMarkers.icon({
                icon: 'star',
                markerColor: 'green'
            });

            currentPosMarker = L.marker([position.coords.latitude, position.coords.longitude], {
                icon: userPositionMarker
            })
                .addTo(map)
                .bindPopup("<b>You are here!</b>");
        });
    }
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

(function ($) {
    scheduledata.forEach(function (schedule, scheduleIndex) {
        $("<p>" + schedule + "</p>").insertAfter("#scheduletitle");
    });

    setTrolleyInfo();

    //for debugging no trolley behavior while trolleys are running
    //routedata= [];

    if (routedata.length === 0) {
        showSchedules = true;
        showSchedule();
    } else {
        routedata.forEach(function (route, routeIndex) {
            buildRoute(route.RouteShape, "route_" + routeIndex, route.RouteColorRGB, route);
        });
    }

    routedata.forEach(function (route, routeIndex) {
        buildStops(route.Stops, route.RouteColorRGB);
    });

    initMap({ lat: 34.852432, lng: -82.398216 });

    addRoutes();
    addStops();
    getTrolleyLocations(oMap);

})(jQuery);