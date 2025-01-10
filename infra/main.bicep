@description('Location for all resources.')
param location string = resourceGroup().location

@description('Cosmos DB account name, max length 44 characters')
param accountName string = toLower('cdb-sql-rbac-${uniqueString(resourceGroup().id)}')

@description('Friendly name for the SQL Role Definition')
param roleDefinitionName string = 'My Read Write Role'

@description('Data actions permitted by the Role Definition')
param dataActions array = [
  'Microsoft.DocumentDB/databaseAccounts/readMetadata'
  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
  'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
]

@description('Object ID of the AAD identity (in this case the local developer). Must be a GUID.')
param localDeveloperPrincipalId string = '46053924-fcc2-4c7a-94ae-daba6d1e5e7b'

param functionAppManagedIdPrincipalId string = '8a5f0df2-db03-4439-9146-b4e8beed0e89'

var roleDefinitionId = guid('sql-role-definition-', localDeveloperPrincipalId, account.id)

var roleAssignmentId = guid(roleDefinitionId, localDeveloperPrincipalId, account.id)
var roleAssignmentId2 = guid(roleDefinitionId, functionAppManagedIdPrincipalId, account.id)

var locations = [
  {
    locationName: location
    failoverPriority: 0
    isZoneRedundant: false
  }
]

resource account 'Microsoft.DocumentDB/databaseAccounts@2021-04-15' = {
  name: accountName
  kind: 'GlobalDocumentDB'
  location: location
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: locations
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
  }
}

resource customRoleDefinition 'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions@2021-04-15' = {
  parent: account
  name: roleDefinitionId
  properties: {
    roleName: roleDefinitionName
    type: 'CustomRole'
    assignableScopes: [
      account.id
    ]
    permissions: [
      {
        dataActions: dataActions
      }
    ]
  }
}

resource customRoleAssignement 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2021-04-15' = {
  parent: account
  name: roleAssignmentId
  properties: {
    roleDefinitionId: resourceId(
      'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions',
      split('${accountName}/${roleDefinitionId}', '/')[0],
      split('${accountName}/${roleDefinitionId}', '/')[1]
    )
    principalId: principalId
    scope: account.id
  }
  dependsOn: [
      customRoleDefinition
  ]
}


resource accountName_roleAssignmentId2 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2021-04-15' = {
  parent: account
  name: roleAssignmentId2
  properties: {
    roleDefinitionId: resourceId(
      'Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions',
      split('${accountName}/${roleDefinitionId}', '/')[0],
      split('${accountName}/${roleDefinitionId}', '/')[1]
    )
    principalId: '8a5f0df2-db03-4439-9146-b4e8beed0e89'
    scope: account.id
  }
  dependsOn: [
      accountName_roleDefinitionId
  ]
}

output roleDefinitionId string = resourceId('Microsoft.DocumentDB/databaseAccounts/sqlRoleDefinitions',split('${accountName}/${roleDefinitionId}', '/')[0],split('${accountName}/${roleDefinitionId}', '/')[1])
