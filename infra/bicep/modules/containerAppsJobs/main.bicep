@description('The location where the Container Apps Job will be deployed')
param location string = resourceGroup().location

@description('The name of the Container Apps Environment')
param containerAppsEnvironmentName string

@description('The name of the Cleanup Job Container App')
param cleanupJobName string

@description('The login server of the Container Registry')
param containerRegistryLoginServer string

@description('The Application Insights connection string')
param applicationInsightsConnectionString string

@description('The Cosmos DB endpoint')
param cosmosDbEndpoint string

@description('The Storage Account blob endpoint')
param storageAccountBlobEndpoint string

@description('The Key Vault URI')
param keyVaultUri string

@description('Tags to apply to resources')
param tags object = {}

// Reference existing Container Apps Environment
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
}

resource cleanupJob 'Microsoft.App/jobs@2024-03-01' = {
  name: cleanupJobName
  location: location
  tags: tags
  properties: {
    environmentId: containerAppsEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      scheduleTriggerConfig: {
        cronExpression: '*/5 * * * *' // Every 5 minutes
        parallelism: 1
        completions: 1
      }
      triggerType: 'Schedule'
      replicaTimeout: 300 // 5 minutes timeout
      replicaRetryLimit: 3
      registries: [
        {
          server: containerRegistryLoginServer
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'cleanup-job'
          image: '${containerRegistryLoginServer}/azure-web-content-share-jobs:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'DOTNET_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsightsConnectionString
            }
            {
              name: 'AzureOptions__Storage__BlobEndpoint'
              value: storageAccountBlobEndpoint
            }
            {
              name: 'AzureOptions__CosmosDb__Endpoint'
              value: cosmosDbEndpoint
            }
            {
              name: 'AzureOptions__KeyVault__Uri'
              value: keyVaultUri
            }
          ]
        }
      ]
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

@description('The resource ID of the Cleanup Job')
output cleanupJobId string = cleanupJob.id

@description('The name of the Cleanup Job')
output cleanupJobName string = cleanupJob.name

@description('The system-assigned managed identity principal ID of the Cleanup Job')
output cleanupJobPrincipalId string = cleanupJob.identity.principalId