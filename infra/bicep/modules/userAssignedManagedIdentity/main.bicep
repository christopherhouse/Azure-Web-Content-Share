@description('The location where the User Assigned Managed Identity will be deployed')
param location string = resourceGroup().location

@description('The name of the User Assigned Managed Identity')
param userAssignedManagedIdentityName string

@description('Tags to apply to resources')
param tags object = {}

resource userAssignedManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: userAssignedManagedIdentityName
  location: location
  tags: tags
}

@description('The resource ID of the User Assigned Managed Identity')
output userAssignedManagedIdentityId string = userAssignedManagedIdentity.id

@description('The name of the User Assigned Managed Identity')
output userAssignedManagedIdentityName string = userAssignedManagedIdentity.name

@description('The principal ID of the User Assigned Managed Identity')
output userAssignedManagedIdentityPrincipalId string = userAssignedManagedIdentity.properties.principalId

@description('The client ID of the User Assigned Managed Identity')
output userAssignedManagedIdentityClientId string = userAssignedManagedIdentity.properties.clientId