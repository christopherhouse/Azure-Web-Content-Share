targetScope = 'resourceGroup'

@description('The location where all resources will be deployed')
param location string = 'eastus2'

@description('The name of the application')
param appName string = 'awcs'

@description('The environment suffix (dev, staging, prod)')
param environmentSuffix string = 'dev'

@description('Tags to apply to all resources')
param tags object = {
  Application: 'Azure Web Content Share'
  Environment: environmentSuffix
  CreatedBy: 'Bicep'
}

// Generate unique resource token using resource group name as salt
var uniqueResourceToken = take(uniqueString(resourceGroup().id, appName), 8)

// Resource names following naming conventions
var logAnalyticsWorkspaceName = 'log-${appName}-${uniqueResourceToken}'
var applicationInsightsName = 'appi-${appName}-${uniqueResourceToken}'
var keyVaultName = 'kv-${appName}-${uniqueResourceToken}'
var storageAccountName = 'st${appName}${uniqueResourceToken}'
var cosmosDbAccountName = 'cosmos-${appName}-${uniqueResourceToken}'
var containerRegistryName = 'cr${appName}${uniqueResourceToken}'
var containerAppsEnvironmentName = 'cae-${appName}-${uniqueResourceToken}'
var apiContainerAppName = 'ca-${appName}-api-${uniqueResourceToken}'
var frontendContainerAppName = 'ca-${appName}-frontend-${uniqueResourceToken}'

// Deploy Log Analytics workspace first (foundation for other services)
module logAnalytics 'modules/logAnalytics/main.bicep' = {
  name: 'logAnalytics-deployment'
  params: {
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    sku: 'PerGB2018'
    retentionInDays: 30
    tags: tags
  }
}

// Deploy Application Insights (depends on Log Analytics)
module appInsights 'modules/appInsights/main.bicep' = {
  name: 'appInsights-deployment'
  params: {
    location: location
    applicationInsightsName: applicationInsightsName
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    applicationType: 'web'
    tags: tags
  }
}

// Deploy Key Vault
module keyVault 'modules/keyVault/main.bicep' = {
  name: 'keyVault-deployment'
  params: {
    location: location
    keyVaultName: keyVaultName
    sku: 'standard'
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: true
    enabledForTemplateDeployment: true
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    tags: tags
  }
}

// Deploy Storage Account
module storageAccount 'modules/storageAccount/main.bicep' = {
  name: 'storageAccount-deployment'
  params: {
    location: location
    storageAccountName: storageAccountName
    sku: 'Standard_LRS'
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    tags: tags
  }
}

// Deploy Cosmos DB
module cosmosDb 'modules/cosmosDb/main.bicep' = {
  name: 'cosmosDb-deployment'
  params: {
    location: location
    cosmosDbAccountName: cosmosDbAccountName
    databaseName: 'ContentShare'
    containerName: 'FileMetadata'
    partitionKeyPath: '/userId'
    consistencyLevel: 'Session'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    enableServerless: true
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    tags: tags
  }
}

// Deploy Container Registry
module containerRegistry 'modules/containerRegistry/main.bicep' = {
  name: 'containerRegistry-deployment'
  params: {
    location: location
    containerRegistryName: containerRegistryName
    sku: 'Basic'
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    tags: tags
  }
}

// Deploy Container Apps (depends on most other services)
module containerApps 'modules/containerApps/main.bicep' = {
  name: 'containerApps-deployment'
  params: {
    location: location
    containerAppsEnvironmentName: containerAppsEnvironmentName
    apiContainerAppName: apiContainerAppName
    frontendContainerAppName: frontendContainerAppName
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    logAnalyticsWorkspaceCustomerId: logAnalytics.outputs.logAnalyticsWorkspaceCustomerId
    containerRegistryLoginServer: containerRegistry.outputs.loginServer
    applicationInsightsConnectionString: appInsights.outputs.connectionString
    cosmosDbEndpoint: cosmosDb.outputs.cosmosDbEndpoint
    storageAccountBlobEndpoint: storageAccount.outputs.blobEndpoint
    keyVaultUri: keyVault.outputs.keyVaultUri
    tags: tags
  }
}

// Role assignments for Container Apps to access other services

// Grant Container Registry pull access to Container Apps
resource containerRegistryPullRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d' // AcrPull role
}

resource apiContainerAppAcrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistryName, apiContainerAppName, containerRegistryPullRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: containerRegistryPullRoleDefinition.id
    principalId: containerApps.outputs.apiContainerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource frontendContainerAppAcrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistryName, frontendContainerAppName, containerRegistryPullRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: containerRegistryPullRoleDefinition.id
    principalId: containerApps.outputs.frontendContainerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Storage Blob Data Contributor access to API Container App
resource storageBlobDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor role
}

resource apiContainerAppStorageAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountName, apiContainerAppName, storageBlobDataContributorRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: storageBlobDataContributorRoleDefinition.id
    principalId: containerApps.outputs.apiContainerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Cosmos DB Data Contributor access to API Container App
resource cosmosDbDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '00000000-0000-0000-0000-000000000002' // Cosmos DB Built-in Data Contributor role
}

resource apiContainerAppCosmosDbAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cosmosDbAccountName, apiContainerAppName, cosmosDbDataContributorRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: cosmosDbDataContributorRoleDefinition.id
    principalId: containerApps.outputs.apiContainerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Key Vault Secrets User access to API Container App
resource keyVaultSecretsUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User role
}

resource apiContainerAppKeyVaultAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVaultName, apiContainerAppName, keyVaultSecretsUserRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: keyVaultSecretsUserRoleDefinition.id
    principalId: containerApps.outputs.apiContainerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
@description('The URL of the frontend application')
output frontendUrl string = 'https://${containerApps.outputs.frontendContainerAppFqdn}'

@description('The URL of the API')
output apiUrl string = 'https://${containerApps.outputs.apiContainerAppFqdn}'

@description('The name of the resource group')
output resourceGroupName string = resourceGroup().name

@description('The location where resources were deployed')
output location string = location

@description('The unique resource token used for naming')
output uniqueResourceToken string = uniqueResourceToken