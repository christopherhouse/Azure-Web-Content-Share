using '../main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'prod'
param cleanupJobCronExpression = '0 */4 * * *' // Every 4 hours for production (cost optimization)
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Production'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}