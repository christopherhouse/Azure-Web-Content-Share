targetScope = 'resourceGroup'

@description('The location where all resources will be deployed')
param location string = 'eastus2'

@description('The name of the application')
param appName string = 'awcs'

@description('The environment suffix (dev, staging, prod)')
param environmentSuffix string = 'dev'

@description('The GitHub Actions build/run ID for unique deployment naming')
param buildUniqueId string = newGuid()

@description('Tags to apply to all resources')
param tags object = {
  Application: 'Azure Web Content Share'
  Environment: environmentSuffix
  CreatedBy: 'Bicep'
}

// Generate unique resource token using resource group name as salt
var uniqueResourceToken = take(uniqueString(resourceGroup().id, appName), 8)

// Generate sanitized build ID from the unique build identifier
var buildId = uniqueString(buildUniqueId)

// Resource names following naming conventions
var logAnalyticsWorkspaceName = 'log-${appName}-${uniqueResourceToken}'
var applicationInsightsName = 'appi-${appName}-${uniqueResourceToken}'
var keyVaultName = 'kv-${appName}-${uniqueResourceToken}'
var storageAccountName = 'st${appName}${uniqueResourceToken}'
var cosmosDbAccountName = 'cosmos-${appName}-${uniqueResourceToken}'
var containerRegistryName = 'cr${appName}${uniqueResourceToken}'
var containerAppsEnvironmentName = 'cae-${appName}-${uniqueResourceToken}'
var apiGatewayContainerAppName = 'ca-gateway-${appName}-${uniqueResourceToken}'
var apiContainerAppName = 'ca-api-${appName}-${uniqueResourceToken}'
var frontendContainerAppName = 'ca-frontend-${appName}-${uniqueResourceToken}'
var userAssignedManagedIdentityName = 'uami-${appName}-${uniqueResourceToken}'

// Deploy Log Analytics workspace first (foundation for other services)
module logAnalytics 'modules/logAnalytics/main.bicep' = {
  name: 'logAnalytics-${buildId}'
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
  name: 'appInsights-${buildId}'
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
  name: 'keyVault-${buildId}'
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
  name: 'storageAccount-${buildId}'
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
  name: 'cosmosDb-${buildId}'
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
  name: 'containerRegistry-${buildId}'
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

// Deploy User Assigned Managed Identity for Container Apps ACR access
module userAssignedManagedIdentity 'modules/userAssignedManagedIdentity/main.bicep' = {
  name: 'userAssignedManagedIdentity-${buildId}'
  params: {
    location: location
    userAssignedManagedIdentityName: userAssignedManagedIdentityName
    tags: tags
  }
}

// Deploy Container Apps Environment (without the apps themselves)
module containerAppsEnvironment 'modules/containerAppsEnvironment/main.bicep' = {
  name: 'containerAppsEnvironment-${buildId}'
  params: {
    location: location
    containerAppsEnvironmentName: containerAppsEnvironmentName
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    logAnalyticsWorkspaceCustomerId: logAnalytics.outputs.logAnalyticsWorkspaceCustomerId
    tags: tags
  }
}

// Deploy Container Apps (API Gateway, API, and Frontend)
module containerApps 'modules/containerApps/main.bicep' = {
  name: 'containerApps-${buildId}'
  params: {
    location: location
    containerAppsEnvironmentName: containerAppsEnvironment.outputs.containerAppsEnvironmentName
    apiGatewayContainerAppName: apiGatewayContainerAppName
    apiContainerAppName: apiContainerAppName
    frontendContainerAppName: frontendContainerAppName
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
    logAnalyticsWorkspaceCustomerId: logAnalytics.outputs.logAnalyticsWorkspaceCustomerId
    containerRegistryLoginServer: containerRegistry.outputs.loginServer
    applicationInsightsConnectionString: applicationInsights.outputs.connectionString
    cosmosDbEndpoint: cosmosDb.outputs.endpoint
    storageAccountBlobEndpoint: storageAccount.outputs.blobEndpoint
    keyVaultUri: keyVault.outputs.keyVaultUri
    tags: tags
  }
  dependsOn: [
    containerAppsEnvironment
    containerRegistry
    applicationInsights
    cosmosDb
    storageAccount
    keyVault
  ]
}

// Role assignments for services to access other resources

// Role definitions (defined once and reused)
resource containerRegistryPullRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '7f951dda-4ed3-4680-a7ca-43fe172d538d' // AcrPull role
}

resource storageBlobDataContributorRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe' // Storage Blob Data Contributor role
}

resource keyVaultSecretsUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6' // Key Vault Secrets User role
}

// Grant Container Registry pull access to User Assigned Managed Identity (for Container Apps)
resource uamiAcrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(containerRegistryName, userAssignedManagedIdentityName, containerRegistryPullRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: containerRegistryPullRoleDefinition.id
    principalId: userAssignedManagedIdentity.outputs.userAssignedManagedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Storage Blob Data Contributor access to UAMI (for Container Apps)
resource uamiStorageAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountName, userAssignedManagedIdentityName, storageBlobDataContributorRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: storageBlobDataContributorRoleDefinition.id
    principalId: userAssignedManagedIdentity.outputs.userAssignedManagedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Grant Key Vault Secrets User access to UAMI (for Container Apps)
resource uamiKeyVaultAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVaultName, userAssignedManagedIdentityName, keyVaultSecretsUserRoleDefinition.id)
  scope: resourceGroup()
  properties: {
    roleDefinitionId: keyVaultSecretsUserRoleDefinition.id
    principalId: userAssignedManagedIdentity.outputs.userAssignedManagedIdentityPrincipalId
    principalType: 'ServicePrincipal'
  }
}

// Outputs
@description('The name of the resource group')
output resourceGroupName string = resourceGroup().name

@description('The location where resources were deployed')
output location string = location

@description('The unique resource token used for naming')
output uniqueResourceToken string = uniqueResourceToken

@description('The name of the Container Apps Environment')
output containerAppsEnvironmentName string = containerAppsEnvironment.outputs.containerAppsEnvironmentName

@description('The name of the Container Registry')
output containerRegistryName string = containerRegistry.outputs.containerRegistryName

@description('The login server of the Container Registry')
output containerRegistryLoginServer string = containerRegistry.outputs.loginServer

@description('The resource ID of the User Assigned Managed Identity')
output userAssignedManagedIdentityId string = userAssignedManagedIdentity.outputs.userAssignedManagedIdentityId

@description('The FQDN of the API Gateway Container App')
output apiGatewayContainerAppFqdn string = containerApps.outputs.apiGatewayContainerAppFqdn

@description('The FQDN of the API Container App (internal)')
output apiContainerAppFqdn string = containerApps.outputs.apiContainerAppFqdn

@description('The FQDN of the Frontend Container App')
output frontendContainerAppFqdn string = containerApps.outputs.frontendContainerAppFqdn

@description('The client ID of the User Assigned Managed Identity')
output userAssignedManagedIdentityClientId string = userAssignedManagedIdentity.outputs.userAssignedManagedIdentityClientId

@description('The Application Insights connection string')
output applicationInsightsConnectionString string = appInsights.outputs.connectionString

@description('The Cosmos DB endpoint')
output cosmosDbEndpoint string = cosmosDb.outputs.cosmosDbEndpoint

@description('The Cosmos DB account name')
output cosmosDbAccountName string = cosmosDb.outputs.cosmosDbAccountName

@description('The Storage Account blob endpoint')
output storageAccountBlobEndpoint string = storageAccount.outputs.blobEndpoint

@description('The Key Vault URI')
output keyVaultUri string = keyVault.outputs.keyVaultUri
