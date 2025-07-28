using '../main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'prod'
param githubActionsServicePrincipalObjectId = '11f8da5c-9a23-4911-892a-acd817a4fa7d'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Production'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}
