# Azure Event Hubs Flood
Opens the flood gates by sending an Azure Event Hub a load of messages

## Getting Started
Update the value of *EventHub* in appsettings.json with a valid connection string that includes the EntityPath. Then just execute `dotnet run -p EventHubsFlood/EventHubsFlood.csproj`.

## What it does
This app simply publishes a random number of messages to an Event Hub of your choosing.

## Why?!
I was looking for a way to performance test a system, so I needed to pump messages into a hub...