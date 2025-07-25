@description('The location where the Container Apps will be deployed')
param location string = resourceGroup().location

@description('The name of the Container Apps Environment')
param containerAppsEnvironmentName string

@description('The name of the API Container App')
param apiContainerAppName string

@description('The name of the Frontend Container App')
param frontendContainerAppName string

@description('The resource ID of the Log Analytics workspace')
param logAnalyticsWorkspaceId string

@description('The customer ID of the Log Analytics workspace')
param logAnalyticsWorkspaceCustomerId string

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

// Reference to Log Analytics workspace for shared key
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: split(logAnalyticsWorkspaceId, '/')[8]
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppsEnvironmentName
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspaceCustomerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

resource apiContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: apiContainerAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        allowInsecure: false
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
      }
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
          name: 'api'
          image: '${containerRegistryLoginServer}/api:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: applicationInsightsConnectionString
            }
            {
              name: 'CosmosDb__Endpoint'
              value: cosmosDbEndpoint
            }
            {
              name: 'Storage__BlobEndpoint'
              value: storageAccountBlobEndpoint
            }
            {
              name: 'KeyVault__VaultUri'
              value: keyVaultUri
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaler'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

resource frontendContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: frontendContainerAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    workloadProfileName: 'Consumption'
    configuration: {
      ingress: {
        external: true
        targetPort: 80
        allowInsecure: false
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
      }
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
          name: 'frontend'
          image: '${containerRegistryLoginServer}/frontend:latest'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'VITE_API_BASE_URL'
              value: 'https://${apiContainerApp.properties.configuration.ingress.fqdn}'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaler'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

@description('The resource ID of the Container Apps Environment')
output containerAppsEnvironmentId string = containerAppsEnvironment.id

@description('The name of the Container Apps Environment')
output containerAppsEnvironmentName string = containerAppsEnvironment.name

@description('The resource ID of the API Container App')
output apiContainerAppId string = apiContainerApp.id

@description('The name of the API Container App')
output apiContainerAppName string = apiContainerApp.name

@description('The FQDN of the API Container App')
output apiContainerAppFqdn string = apiContainerApp.properties.configuration.ingress.fqdn

@description('The system-assigned managed identity principal ID of the API Container App')
output apiContainerAppPrincipalId string = apiContainerApp.identity.principalId

@description('The resource ID of the Frontend Container App')
output frontendContainerAppId string = frontendContainerApp.id

@description('The name of the Frontend Container App')
output frontendContainerAppName string = frontendContainerApp.name

@description('The FQDN of the Frontend Container App')
output frontendContainerAppFqdn string = frontendContainerApp.properties.configuration.ingress.fqdn

@description('The system-assigned managed identity principal ID of the Frontend Container App')
output frontendContainerAppPrincipalId string = frontendContainerApp.identity.principalId