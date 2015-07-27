@echo off
rem set Server=localhost:51304
set Server=yeahthattrolley.azurewebsites.net

curl --user Brigade:brigade --data "Lat=34.8506231&Lon=-82.4003675" http://%Server%/api/v1/Trolleys/5/Location 
