@description('The location where the Key Vault will be deployed')
param location string = resourceGroup().location

@description('The name of the Key Vault')
param keyVaultName string

@description('The Azure Active Directory tenant ID for the Key Vault')
param tenantId string = tenant().tenantId

@description('The pricing tier of the Key Vault')
@allowed([
  'standard'
  'premium'
])
param sku string = 'standard'

@description('Whether to enable soft delete for the Key Vault')
param enableSoftDelete bool = true

@description('The number of days to retain soft deleted keys')
@minValue(7)
@maxValue(90)
param softDeleteRetentionInDays int = 90

@description('Whether to enable purge protection for the Key Vault')
param enablePurgeProtection bool = true

@description('Whether to enable Azure Resource Manager template deployment from the Key Vault')
param enabledForTemplateDeployment bool = true

@description('Whether to enable Azure Disk Encryption from the Key Vault')
param enabledForDiskEncryption bool = false

@description('Whether to enable Azure Virtual Machines deployment from the Key Vault')
param enabledForDeployment bool = false

@description('The resource ID of the Log Analytics workspace for diagnostic settings')
param logAnalyticsWorkspaceId string

@description('Tags to apply to the Key Vault')
param tags object = {}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    tenantId: tenantId
    sku: {
      family: 'A'
      name: sku
    }
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection ? true : null
    enabledForTemplateDeployment: enabledForTemplateDeployment
    enabledForDiskEncryption: enabledForDiskEncryption
    enabledForDeployment: enabledForDeployment
    accessPolicies: []
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
    publicNetworkAccess: 'Enabled'
  }
}

resource keyVaultDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${keyVaultName}-diagnostics'
  scope: keyVault
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      {
        categoryGroup: 'allLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

@description('The resource ID of the Key Vault')
output keyVaultId string = keyVault.id

@description('The name of the Key Vault')
output keyVaultName string = keyVault.name

@description('The URI of the Key Vault')
output keyVaultUri string = keyVault.properties.vaultUri