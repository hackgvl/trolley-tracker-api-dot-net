# trolley-tracker

###Experimental project to test Visual Studio 2015

General layout - 

 * Visual Studio 2015 (Release)
 * SQL Server 2014
 * Database-First development
 * .NET 4.5.2
 * ASP.NET version 4.x
 * MVC version 5
 * Entity Framework 6.3
 * (Project tests have not been touched and will fail)


Parameters may be viewed from a web browser.  For changes, Login to the web app with a user with a route management role - ManageTrolley@yeahthattrolley.com for example.  

##Server location

 The API Server is located at http://tracker.wallinginfosystems.com


##API

 NOTE: Refer to the Javascript located in source folder DBVisualizer / index.html for example parse sequences.   Most applications will only need a subset of those calls.

API views may be entered into a browser, but the results will be in XML.  API views called from CURL return as JSON.   Applications wishing for JSON should specify in their request “Content-type: application/json; charset=utf-8”

####GET /api/v1/Trolleys/Running
Returns list of all active trollies and their current locations.  This should be the call to use to get all trolley positions because it can handle many clients with minimal overhead.

####POST /api/v1/Trolleys/:Number/Location

Updates specified trolley location with the posted Lat and Lon parameters.  This API may use BASIC authentication - for example - 

curl --user TrolleyUpdates@yeahthattrolley.com:{PASSWORD} --data "Lat=34.8506231&Lon=-82.4003675" http://tracker.wallinginfosystems.com/api/v1/Trolleys/999/Location 

Note: The URL and authorization parameters will change when the application is moved to the new server.

The :Number is an arbitrary number assigned to the vehicle beacon in the settings as "Trolley ID" when installed.  This is different from the database ID.   All other API settings below use the database ID and not the trolley number.   If Greenlink has any identification for each trolley, be sure to add that to the description when installing.


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

Anyone may view public data from the web app without a login.    Public procedures are those controller endpoints that have no 'CustomAuthorization' parameters, such as **[CustomAuthorize(Roles = "Vehicles")**.   In order for a user to interact with that endpoint, they must be a member of that named role.


###Setting up a new development environment or on a new server where the old user database was lost.

 The application checks for the existance of the database during a log in attempt, and if not found in the **app_data** web folder, it will automatically be created and seeded with the 3 roles: Administrators, RouteManagers, and Vehicles.

 1.Run the SQL Script schema and data dump to create the route data on SQL Server.
 * Bring up the web site in a browser.  It should show that it is not logged in.
 * Click on the **Register as a new user** link.
 * Enter your Email and password and click **Register**.   Note - double check that the email address is correct; there is no counter-verification by Email to confirm a valid Email account.   This will be the system manager account - choose a meaningful name or your own email address.
 * Confirm that the browser shows your Email address as logged in in the upper right corner.
 * Click on **Role Manager**.
 * The first user into the Role Manager on a new system is automatically made a system administrator.   This could be a possible security problem when rebuilding from scratch - check the list of users to be sure no other user is registered as system administrator.
 * Create a user login for the Vehicles and add it to the Vehicles role.
 * Create other user logins, or have them self-register and add them to the appropriate role(s).

   
  Suggest **TrolleyUpdates@yeahthattrolley.com**  - role = **Vehicles** 

  Suggest **ManageTrolley@yeahthattrolley.com** - role = **RouteManagers**
 
Because the web page is not secured, the public pages can be viewed by anyone in the public with no login.  People will come across it and might register, but it will have no effect unless they bulk register a zillion user IDs and fill the database quota.   Password guessing attacks could succeed if simple passwords are used.

