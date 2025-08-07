@description('The location where the Cosmos DB account will be deployed')
param location string = resourceGroup().location

@description('The name of the Cosmos DB account')
param cosmosDbAccountName string

@description('The database name')
param databaseName string = 'ContentShare'

@description('Array of containers to create')
param containers array = []

@description('The consistency level of the Cosmos DB account')
@allowed([
  'Eventual'
  'ConsistentPrefix'
  'Session'
  'BoundedStaleness'
  'Strong'
])
param consistencyLevel string = 'Session'

@description('Whether to enable automatic failover')
param enableAutomaticFailover bool = false

@description('Whether to enable multiple write locations')
param enableMultipleWriteLocations bool = false

@description('Whether to enable serverless')
param enableServerless bool = true

@description('The resource ID of the Log Analytics workspace for diagnostic settings')
param logAnalyticsWorkspaceId string

@description('Tags to apply to the Cosmos DB account')
param tags object = {}

resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: cosmosDbAccountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: consistencyLevel
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: enableAutomaticFailover
    enableMultipleWriteLocations: enableMultipleWriteLocations
    capabilities: enableServerless ? [
      {
        name: 'EnableServerless'
      }
    ] : []
    backupPolicy: {
      type: 'Periodic'
      periodicModeProperties: {
        backupIntervalInMinutes: 240
        backupRetentionIntervalInHours: 8
        backupStorageRedundancy: 'Local'
      }
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' = {
  name: databaseName
  parent: cosmosDbAccount
  properties: {
    resource: {
      id: databaseName
    }
    options: enableServerless ? {} : {
      throughput: 400
    }
  }
}

resource cosmosDbContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [for container in containers: {
  name: container.name
  parent: cosmosDbDatabase
  properties: {
    resource: {
      id: container.name
      partitionKey: {
        paths: [
          container.partitionKeyPath
        ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        includedPaths: [
          {
            path: '/*'
          }
        ]
        excludedPaths: [
          {
            path: '/"_etag"/?'
          }
        ]
      }
      defaultTtl: -1
    }
    options: enableServerless ? {} : {
      throughput: 400
    }
  }
}]

resource cosmosDbDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${cosmosDbAccountName}-diagnostics'
  scope: cosmosDbAccount
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'Requests'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

@description('The resource ID of the Cosmos DB account')
output cosmosDbAccountId string = cosmosDbAccount.id

@description('The name of the Cosmos DB account')
output cosmosDbAccountName string = cosmosDbAccount.name

@description('The endpoint URL of the Cosmos DB account')
output cosmosDbEndpoint string = cosmosDbAccount.properties.documentEndpoint

@description('The name of the database')
output databaseName string = cosmosDbDatabase.name

@description('Array of container names created')
output containerNames array = [for i in range(0, length(containers)): cosmosDbContainer[i].name]

@description('Array of containers with their details')
output containers array = [for i in range(0, length(containers)): {
  name: cosmosDbContainer[i].name
  partitionKeyPath: containers[i].partitionKeyPath
}]
