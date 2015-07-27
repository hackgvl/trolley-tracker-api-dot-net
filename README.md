# trolley-tracker

###Experimental project to test Visual Studio 2015

General layout - 

 * Visual Studio 2015 (Release)
 * SQL Server 2014
 * Database-First development
 * .NET 4.6
 * ASP.NET version?
 * MVC version?
 * Entity Framework 6.3


###For updatesEdits Login to the web app with pre-registered user ManageTrolley@yeahthattrolley.com , ManageTrolley7!

  (not a real Email address)

##API

API views may be entered into a browser, but the results will be in XML.  API views called from CURL return as JSON.   Applications wishing for JSON should specify in their request “Content-type: application/json; charset=utf-8”

####GET  /api/v1/Trolleys
Gets list of Trolleys.   ID is the database handle, while the 'Number' field is the number assigned to that trolley by Greenlink or the instance of our Vehicle app.

####GET /api/v1/Trolleys/:ID/Location
Details about a specific trolley

####GET /api/v1/Trolleys/Running
Returns list of all active trollies and their current locations.  This should be the call to use to get all trolley positions because it can handle many clients with minimal overhead.

####GET /api/v1/Routes
Returns a summary of all routes.   A route that is Flag-Stop only will have no identified stops.   (Trolley stops anywhere that is safe upon being hailed).

####GET /api/v1/Routes/:ID
Route detail - including stops in order and route path

####GET ./api/v1/Stops
Get list of stops on all routes

####GET /api/v1/Stops/:ID
Gets info about a single stop

####GET /api/v1/Routes/Active
Returns list of routes active according to the current schedule.

####GET /api/v1/RouteSchedules
Returns a list of route schedules.   Mostly for information, but probably no direct usage.

####GET /api/v1/RouteSchedules/:ID
Return a specific route schedule.

####POST /api/v1/Trolleys/:ID/Location

Updates trolley location with the posted Lat and Lon parameters - for example - 

curl --user Brigade:brigade --data "Lat=34.8506231&Lon=-82.4003675" http://localhost:51304/api/v1/Trolleys/5/Location 

