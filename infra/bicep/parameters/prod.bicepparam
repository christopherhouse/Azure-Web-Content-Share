using '../main.bicep'

param location = 'northcentralus'
param appName = 'awcs'
param environmentSuffix = 'prod'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Production'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}
