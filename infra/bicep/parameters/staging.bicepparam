using 'main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'staging'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Staging'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}