@echo off
set Server=localhost:51304
Echo List of all trolleys
Echo .
curl http://%Server%/api/v1/Trolleys
pause
Echo .
Echo Single Trolley Location
curl http://%Server%/api/v1/Trolleys/5/Location
pause
Echo .
Echo All running trollies and locations
curl http://%Server%/api/v1/Trolleys/Running
pause
Echo .
Echo List of routes
curl http://%Server%/api/v1/Routes
pause
Echo .
Echo Route detail - including stops in order and route path
curl http://%Server%/api/v1/Routes/1
pause
Echo .
Echo Stops
curl http://%Server%/api/v1/Stops
pause
Echo .
Echo A specific stop
curl http://%Server%/api/v1/Stops/24
pause
Echo .
Echo Active routes according to schedule
curl http://%Server%/api/v1/Routes/Active
pause
Echo .
Echo Route Schedules - info only
curl http://%Server%/api/v1/RouteSchedules
pause
Echo .
Echo Specific route schedule
curl http://%Server%/api/v1/RouteSchedules/5
pause

