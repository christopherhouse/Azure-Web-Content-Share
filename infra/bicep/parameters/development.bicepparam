using '../main.bicep'

param location = 'eastus2'
param appName = 'awcs'
param environmentSuffix = 'development'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Development'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}
