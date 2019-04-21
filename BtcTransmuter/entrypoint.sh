#!/bin/sh

# Kill any living .net processes and then start transmuter. This is so that we can restart transmuter from inside the container
pkill -9 -e -f dotnet 
exec dotnet BtcTransmuter.dll