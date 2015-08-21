@echo off
if "%1"=="" Goto NoPassword
rem set Server=localhost:51304
set Server=api.yeahthattrolley.com/

curl --user TrolleyUpdates@yeahthattrolley.com:%1 --data "Lat=34.8506231&Lon=-82.4003675" http://%Server%/api/v1/Trolleys/999/Location 
goto ScriptEnd

:NoPassword
Echo .
Echo Missing password
Echo Usage:  TestUpdateLocation Password
Echo  Where 'password' is the password for TrolleyUpdates@yeahthattrolley.com


:ScriptEnd