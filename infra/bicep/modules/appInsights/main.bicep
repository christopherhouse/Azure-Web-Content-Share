@description('The location where the Application Insights resource will be deployed')
param location string = resourceGroup().location

@description('The name of the Application Insights resource')
param applicationInsightsName string

@description('The resource ID of the Log Analytics workspace to associate with Application Insights')
param logAnalyticsWorkspaceId string

@description('Application type for Application Insights')
@allowed([
  'web'
  'other'
])
param applicationType string = 'web'

@description('Tags to apply to the Application Insights resource')
param tags object = {}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: applicationType
    WorkspaceResourceId: logAnalyticsWorkspaceId
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('The resource ID of the Application Insights resource')
output applicationInsightsId string = applicationInsights.id

@description('The name of the Application Insights resource')
output applicationInsightsName string = applicationInsights.name

@description('The instrumentation key of the Application Insights resource')
output instrumentationKey string = applicationInsights.properties.InstrumentationKey

@description('The connection string of the Application Insights resource')
output connectionString string = applicationInsights.properties.ConnectionString