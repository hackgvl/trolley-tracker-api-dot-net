@echo off

REM Possible trolley paths in City of Greenville, SC

REM How to re-create or update the city street data file from OpenStreetMap data.

REM - Requirements:
REM 1.  WGET for Windows in path
REM 2.  Osmosis and Java, modify invocation path below as required
REM     - Obtain Osmosis from http://wiki.openstreetmap.org/wiki/Osmosis
REM 3.  Set the bounding box according to the geographical area of interest.
REM     - Should be as small as possible so that the browser isn't overloaded during route edit.
REM     - Must be large enough to cover entire possible route area of interest

set StateSourceFile=south-carolina-latest.osm.pbf

IF EXIST .\%StateSourceFile% GOTO HaveStateFile

ECHO Fetching state file %StateSourceFile% ...
wget http://download.geofabrik.de/north-america/us/%StateSourceFile%

:HaveStateFile:
REM Have OpenStreetmap streets - now extract the area of interest in XML/OSM format
REM set options for Osmosis
set OSMOSIS_OPTIONS=--read-pbf file=.\%StateSourceFile% ^
--bounding-box bottom=34.8227 top=34.86689 left=-82.43721 right=-82.37587 ^
  --tf accept-ways highway=* ^
  --tf reject-ways highway=path,track,footway,cycleway ^
  --tf reject-relations ^
  --used-node ^
--write-xml file=.\greenvilleCityTrolleyPaths.osm

REM Execute Osmosis with the above options
Osmosis-Latest\bin\osmosis.bat

