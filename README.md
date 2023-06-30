# Trolley Tracker - [Archived in June 2023]

<p>From 2014-2021, Code For Greenville members built and maintained the technology which allowed thousands of locals and visitors to track the downtown Greenville trolleys in real-time from their mobile devices.<p>

<p>As of June 30, 2023, <a href="https://codeforamerica.org/news/reflections-on-the-brigade-networks-next-chapter/">Code For America officially withdrew fiscal sponsorship and use of the "Code For" trademark to <strong>all</strong> national brigades</a>, including Code For Greenville.</p>

<p>After July 1st, 2023, contributors can get involved with two re-branded efforts:</p>

<ul>
	<li>For ongoing civic projects, connect with <a href="https://opencollective.com/code-for-the-carolinas">Code For The Carolina</a> (which itself will rebrand by the end of 2023)</li>
	<li>For local tech APIs and OpenData projects, see the <a href="https://github.com/hackgvl">HackGreenville Labs repositories on GitHub</a> and connect with the team in the _#hg-labs_ channel on the <a href="https://hackgreenville.com/join-slack">HackGreenville Slack</a></li>
</ul>



### Trolley Tracker route data management and API interface

Toolset -

 * Visual Studio 2017 (Release), Visual Studio 2015 might still work
 * SQL Server 2014 / SQL Server Express
 * Code-First development
 * .NET 4.6.2
 * ASP.NET version 4.x
 * MVC version 5
 * Entity Framework 6.3

## General Layout

![System Block Diagram](https://raw.githubusercontent.com/codeforgreenville/trolley-tracker-api-dot-net/master/Doc/SystemBlockDiagram.jpg)

Parameters may be viewed from a web browser.  For changes, Login to the web app with a user with a route management role - ManageTrolley@yeahthattrolley.com for example.  

## Server location

 The API and web Server is located at http://api.yeahthattrolley.com

 There is a Dev server located at http://yeahthattrolley.azurewebsites.net/ .  The purpose of the dev server is for testing client apps when there is no current trolley running.  The schedule may be changed as needed to activate route(s).


## API

 NOTE: Refer to the Javascript located in source folder DBVisualizer / index.html for example parse sequences.   Most applications will only need a subset of those calls.

API views may be entered into a browser and the results will be in XML.  API views called from CURL and specified with text/xml return as XML.   Applications wishing for JSON should specify in their request “Content-type: application/json; charset=utf-8”

#### GET /api/v1/Trolleys/Running
Returns list of all active trollies and their current locations.  This should be the call to use to get all trolley positions because it can handle many clients with minimal overhead.  
No trolleys will be returned if no route is currently scheduled.   To see any active trolley beacon, regardless of schedule, append a debug parameter formatted as GET /api/v1/Trolleys/Running?debug=true

#### POST /api/v1/Trolleys/:Number/Location

Updates specified trolley location with the posted Lat and Lon parameters.  This API may use BASIC authentication - for example - 

curl --user TrolleyUpdates@yeahthattrolley.com:{PASSWORD} --data "Lat=34.8506231&Lon=-82.4003675" http://api.yeahthattrolley.com/api/v1/Trolleys/999/Location 

Note: The URL and authorization parameters will change when the application is moved to the new server.

The :Number is an arbitrary number assigned to the vehicle beacon in the settings as "Trolley ID" when installed.  This is different from the database ID.   All other API settings below use the database ID and not the trolley number.   If Greenlink has any identification for each trolley, be sure to add that to the description when installing.


#### GET  /api/v1/Trolleys
Gets list of Trolleys.   ID is the database handle, while the 'Number' field is the number assigned to that trolley by Greenlink or the instance of our Vehicle app.

#### GET /api/v1/Trolleys/:ID/Location
Details about a specific trolley

#### GET /api/v1/Routes
Returns a summary of all routes.   A route that is Flag-Stop only will have no identified stops.   (Trolley stops anywhere that is safe upon being hailed).

#### GET /api/v1/Routes/:ID
Route detail - including stops in order and route path

#### GET /api/v1/Stops
Get list of stops on all routes

#### GET /api/v1/Stops/Regular
Get list of stops on all routes which have a regular, fixed schedule

#### GET /api/v1/Stops/:ID
Gets info about a single stop

#### GET /api/v1/Routes/Active
Returns list of routes active according to the current schedule.

#### GET /api/v1/RouteSchedules
Returns a list of route schedules.   Mostly for information, but probably no direct usage.

#### GET /api/v1/RouteSchedules/:ID
Return a specific route schedule.


## Stop editor

Stops have been moved out onto a map display.    If the user has edit privileges:

 * Hover the mouse over a stop for the stop title
 * Right click on a stop to remove it.
 * Left click and drag on a stop to move to a different position.
 * Left click to see or edit information about the stop, including a photo
 * Photos should be resized before uploading - make them as small as possible for fastest client download.  The editor shows them in a small non-proportional thumbnail, but the photo URL from the web API shows them in full size.


Stops List (text) is the old summary screen, placed there for reference, but may be removed if not needed in the future.


## User Credentials

Anyone may view public data from the web app without a login.    Public procedures are those controller endpoints that have no 'CustomAuthorization' parameters, such as **[CustomAuthorize(Roles = "Vehicles")**.   In order for a user to interact with that endpoint, they must be a member of that named role.


### Setting up a new development environment or on a new server where the old user database was lost.   

**Note:** It may be possible to run from a local DB on a development system by changing the connection string in Web.Config

 1. Build the solution.
 * Run the application which brings up the web site in a browser.  It should show that it is not logged in.
 * Click on the **Register as a new user** link.
 * Enter your Email and password and click **Register**.   Note - double check that the email address is correct; there is no counter-verification by Email to confirm a valid Email account.   This will be the system manager account - choose a meaningful name or your own email address.
 * Confirm that the browser shows your Email address as logged in in the upper right corner.  The first user in the system is the system administrator.   The Role Manager option is now added to the Database dropdown.
 * Click on **Database / Role Manager**.   **NOTE:** No need to go further for a local development system.
 * Logout, Create a user login for the Vehicles and add it to the Vehicles role.
 * Create other user logins, or have them self-register and add them to the appropriate role(s).

   
  Suggest **TrolleyUpdates@yeahthattrolley.com**  - role = **Vehicles** 

  Suggest **ManageTrolley@yeahthattrolley.com** - role = **RouteManagers**
 
Because the web page is not secured, the public pages can be viewed by anyone in the public with no login.  People will come across it and might register, and password guessing attacks could succeed if simple passwords are used.

