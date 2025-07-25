using 'main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'prod'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Production'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}