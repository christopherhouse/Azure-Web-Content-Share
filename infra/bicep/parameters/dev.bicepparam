using '../main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'dev'
param cleanupJobCronExpression = '0 */1 * * *' // Every hour for dev environment
param githubActionsServicePrincipalObjectId = '11f8da5c-9a23-4911-892a-acd817a4fa7d'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Development'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}
