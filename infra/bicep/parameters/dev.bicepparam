using '../main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'dev'
param cleanupJobCronExpression = '0 */1 * * *' // Every hour for dev environment
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Development'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}