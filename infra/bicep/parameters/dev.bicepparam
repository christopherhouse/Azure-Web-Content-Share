using '../main.bicep'

param location = 'northcentralus'
param appName = 'awcs'
param environmentSuffix = 'dev'
param tags = {
  Application: 'Azure Web Content Share'
  Environment: 'Development'
  CreatedBy: 'Bicep'
  CostCenter: 'Engineering'
}
