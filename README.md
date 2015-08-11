# trolley-tracker

###Experimental project to test Visual Studio 2015

General layout - 

 * Visual Studio 2015 (Release)
 * SQL Server 2014
 * Database-First development
 * .NET 4.5.2
 * ASP.NET version?
 * MVC version?
 * Entity Framework 6.3


Parameters may be viewed from a web browser.  For updates, Login to the web app with pre-registered user ManageTrolley@yeahthattrolley.com , ManageTrolley7!   (That is not a real Email address)


##API

API views may be entered into a browser, but the results will be in XML.  API views called from CURL return as JSON.   Applications wishing for JSON should specify in their request “Content-type: application/json; charset=utf-8”

####GET /api/v1/Trolleys/Running
Returns list of all active trollies and their current locations.  This should be the call to use to get all trolley positions because it can handle many clients with minimal overhead.

####POST /api/v1/Trolleys/:ID/Location

Updates specified trolley location with the posted Lat and Lon parameters.  This API requires BASIC authentication - for example - 

curl --user Brigade:brigade --data "Lat=34.8506231&Lon=-82.4003675" http://yeahthattrolley.azurewebsites.net/api/v1/Trolleys/5/Location 

Note: The URL and authorization parameters will change when the application is moved to the new server.

To find out the :ID to use for the application -
 * Get the vehicle number that has been assigned where the vehicle app is running
 * Query the list of trolleys using the API /api/vx/Trolleys
 * Find the vehicle number in the 'Number' field, then use that trolley ID to post the location
 * If the 'Number' for the current vehicle is not found, the vehicle app shouldn't post location updates.   Add that trolley number to the database using the web editor.
 * NOTE: The database IDs are currently 1..3, but may change or be any number.



####GET  /api/v1/Trolleys
Gets list of Trolleys.   ID is the database handle, while the 'Number' field is the number assigned to that trolley by Greenlink or the instance of our Vehicle app.

####GET /api/v1/Trolleys/:ID/Location
Details about a specific trolley

####GET /api/v1/Routes
Returns a summary of all routes.   A route that is Flag-Stop only will have no identified stops.   (Trolley stops anywhere that is safe upon being hailed).

####GET /api/v1/Routes/:ID
Route detail - including stops in order and route path

####GET /api/v1/Stops
Get list of stops on all routes

####GET /api/v1/Stops/:ID
Gets info about a single stop

####GET /api/v1/Routes/Active
Returns list of routes active according to the current schedule.

####GET /api/v1/RouteSchedules
Returns a list of route schedules.   Mostly for information, but probably no direct usage.

####GET /api/v1/RouteSchedules/:ID
Return a specific route schedule.


##User Credentials

User credentials are stored separately from the main database (currently in SQL Server).  They are stored in a LocalDB file in the web site directory in the **app_data** folder as **DefaultConnection_sav.mdf**.


Anyone may view public data from the web app without a login.    Public procedures are those controller endpoints that have no 'CustomAuthorization' parameters, such as **[CustomAuthorize(Roles = "Vehicles")**.   In order for a user to interact with that endpoint, they must be a member of that named role.


###Setting up a new development environment or on a new server where the old user database was lost.

 1. The application checks for the existance of the database during a log in attempt, and if not found in the **app_data** web folder, it will automatically be created and seeded with the 3 roles: Administrators, RouteManagers, and Vehicles.
 * Bring up the web site in a browser.  It should show that it is not logged in.
 * Click on the Log In Link
 * Enter your Email and a password and click **Log in**.  The log in attempt will fail, but that action will create and seed a new database.
 * Click on the **Register as a new user** link.
 * Enter your Email and password and click **Register**.   Note - double check that the email address is correct; there is no counter-verification by Email to confirm a valid Email account.
 * Confirm that the browser shows your Email address as logged in in the upper right corner.
 * Click on **Role Manager**.
 * The first user in is automatically made a system administrator.
 * Create a user login for the Vehicles and add it to the Vehicles role.
 * Create other user logins, or have them self-register and add them to the appropriate role(s).

   
  Suggest **TrolleyUpdates@yeahthattrolley.com**  - role = **Vehicles** 

  Suggest **ManageTrolley@yeahthattrolley.com** - role = **RouteManagers**
 
Because the web page is not secured, the public pages can be viewed by anyone in the public with no login.  People will come across it and might register, but it will have no effect unless they bulk register a zillion user IDs and fill the database quota.

