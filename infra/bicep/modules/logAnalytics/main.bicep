@description('The location where the Log Analytics workspace will be deployed')
param location string = resourceGroup().location

@description('The name of the Log Analytics workspace')
param logAnalyticsWorkspaceName string

@description('The pricing tier of the Log Analytics workspace')
@allowed([
  'Free'
  'Standard'
  'Premium'
  'Standalone'
  'PerNode'
  'PerGB2018'
  'CapacityReservation'
])
param sku string = 'PerGB2018'

@description('The number of days to retain data in the Log Analytics workspace')
@minValue(30)
@maxValue(730)
param retentionInDays int = 30

@description('Tags to apply to the Log Analytics workspace')
param tags object = {}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: sku
    }
    retentionInDays: retentionInDays
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
    workspaceCapping: {
      dailyQuotaGb: -1
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('The resource ID of the Log Analytics workspace')
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id

@description('The name of the Log Analytics workspace')
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name

@description('The workspace ID (customer ID) of the Log Analytics workspace')
output logAnalyticsWorkspaceCustomerId string = logAnalyticsWorkspace.properties.customerId