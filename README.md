# Goal
The goal is to demonstrate how to access Azure CosmosDB with a managed identity from an Azure Function App.
As the CosmosDB RBAC is proprietary RBAC this project includes the neccesary bicep files to create to DB account and related resources.

# Prerequisites
- An active Azure subscription
- local IDE (e.g. Visual Studio in this case)

# Setup
1. Create an azure function app and activate the managed identity. Note the identity principal id and paste it into main.bicep file.
2. Create db account, role definitions and assignements
     - Developer-CMD: `az deployment group create --resource-group <your-resource-group-name>--template-file infra\main.bicep`
     - CosmosDB Account: create db, container and one example item
3. Run code locally to retrieve document authenticated with the local developers identity
4. Deploy code to a azure function app
5. add the cosmos db endpoint to function config
    - Developer-CMD: 
    - `functionName="<your-functionApp-Name>"`
    - `cosmosName="<your-cosmosdb-name>"`
    - `resourceGroupName="<your-resource-group-name>"`
    - `cosmosEndpoint=$(az cosmosdb show --resource-group $resourceGroupName --name $cosmosName --query documentEndpoint --output tsv)`
    - `az functionapp config appsettings set --resource-group $resourceGroupName --name $functionName --settings "COSMOS_ENDPOINT=$cosmosEndpoint"`
