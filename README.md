# TicketSystem
A Ticket Sales System built on a three tier architecture

In TicketSystemAPI is the API endpoints that are doing the business logic.
In TicketSystemApp is the node.js Express web application

Please note that the API is using Elastic Search to operate so that needs to be setup.
By default the app is looking at 127.0.0.10 as the API source URL.
Future patches will change this from code to an application setting/configuration.
