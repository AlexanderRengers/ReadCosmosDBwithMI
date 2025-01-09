# Goal
The goal is to demonstrate how to access Azure CosmosDB with a managed identity from an Azure Function App.
As the CosmosDB RBAC is proprietary RBAC this project includes the neccesary bicep files to create to DB account and related resources.

1. Create db account, role definitions and assignements
     - Developer-CMD: `az deployment group create --resource-group test-cosmosdb-mi --template-file infra\main.bicep`
     - CosmosDB Account: create db, container and one example item
