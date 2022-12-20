Configure 
===========
local.settings
------------
{
    "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "MongoDBAtlasConnectionString": "mongodb+srv://ApplicationAccess:CjXakcd7d65ykg9R@iotech-cluster.ztsn53p.mongodb.net/?retryWrites=true&w=majority",
    "DatabaseName": "IndOMTechDB",
    "ContactCollection": "IOContacts",
    "DKUrl": "http://deepak.shaw.com"
  }
}
host.json
---------
{
    "version": "2.0",
  "logging": {
    "applicationInsights": {
      "samplingSettings": {
        "isEnabled": true,
        "excludedTypes": "Request"
      }
    }
  },
  "extensions": {"http": {"routePrefix": "DK-MONGO-API"}}
}
==============================

NuGet Package Manager
---------------------
- MongoDB.Driver 

ref
---
https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mongo-app?view=aspnetcore-7.0&tabs=visual-studio