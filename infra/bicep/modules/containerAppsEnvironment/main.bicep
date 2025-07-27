@description('The location where the Container Apps Environment will be deployed')
param location string = resourceGroup().location

@description('The name of the Container Apps Environment')
param containerAppsEnvironmentName string

@description('The resource ID of the Log Analytics workspace')
param logAnalyticsWorkspaceId string

@description('The customer ID of the Log Analytics workspace')
param logAnalyticsWorkspaceCustomerId string

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

@description('The resource ID of the Container Apps Environment')
output containerAppsEnvironmentId string = containerAppsEnvironment.id

@description('The name of the Container Apps Environment')
output containerAppsEnvironmentName string = containerAppsEnvironment.name