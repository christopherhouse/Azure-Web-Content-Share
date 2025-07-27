# Azure Web Content Share - Infrastructure

This directory contains the Bicep infrastructure as code templates for deploying the Azure Web Content Share application.

## 🏗️ Architecture Overview

The infrastructure deploys the following Azure services:

- **Azure Container Apps Environment** - Hosting platform for containerized applications
- **Azure Container Apps** - API and Frontend applications 
- **Azure Container Registry** - Container image storage
- **Azure Storage Account** - File storage with blob containers
- **Azure Cosmos DB** - Serverless database for metadata
- **Azure Key Vault** - Secrets and configuration management  
- **Azure Log Analytics** - Centralized logging and monitoring
- **Azure Application Insights** - Application performance monitoring

## 📁 Directory Structure

```
infra/bicep/
├── main.bicep                          # Main deployment template
├── parameters/
│   ├── dev.bicepparam                  # Development parameters
│   ├── staging.bicepparam              # Staging parameters  
│   └── prod.bicepparam                 # Production parameters
└── modules/
    ├── cosmosDb/main.bicep            # Cosmos DB module
    ├── storageAccount/main.bicep      # Storage Account module
    ├── keyVault/main.bicep            # Key Vault module
    ├── containerApps/main.bicep       # Container Apps module
    ├── logAnalytics/main.bicep        # Log Analytics module
    ├── appInsights/main.bicep         # Application Insights module
    └── containerRegistry/main.bicep    # Container Registry module
```

## 🚀 Deployment

### Prerequisites

- Azure CLI installed and configured
- Bicep CLI installed (`az bicep install`)
- Appropriate Azure permissions for the target subscription
- Resource group created in Azure
- **GitHub Actions Service Principal object ID** (see [GitHub Actions Service Principal Setup](#github-actions-service-principal-setup))

### GitHub Actions Service Principal Setup

For CI/CD deployment, you need to configure the Service Principal object ID in the parameter files:

1. **Find your Service Principal Object ID**:
   ```bash
   # Using the Client ID from AZURE_CLIENT_ID secret
   az ad sp show --id <AZURE_CLIENT_ID> --query id -o tsv
   ```

2. **Update Parameter Files**:
   Replace `REPLACE_WITH_ACTUAL_SERVICE_PRINCIPAL_OBJECT_ID` in:
   - `infra/bicep/parameters/dev.bicepparam`
   - `infra/bicep/parameters/prod.bicepparam`

   With the actual Service Principal object ID (GUID format).

3. **Why This Is Required**:
   The GitHub Actions workflow pushes Docker images to Azure Container Registry. This requires the `AcrPush` RBAC permission to be assigned to the Service Principal used by GitHub Actions.

### Resource Naming Convention

Resources follow the naming pattern: `[resource-type-abbreviation]-[appName]-[uniqueResourceToken]`

- **Resource Type Abbreviations**:
  - `st` - Storage Account
  - `cosmos` - Cosmos DB Account
  - `kv` - Key Vault
  - `cae` - Container Apps Environment
  - `ca` - Container App
  - `cr` - Container Registry
  - `log` - Log Analytics Workspace
  - `appi` - Application Insights

- **App Name**: `awcs` (Azure Web Content Share)
- **Unique Resource Token**: Generated using `uniqueString()` with resource group as salt

### Manual Deployment

#### 1. Deploy to Development Environment

```bash
# Create resource group
az group create --name rg-awcs-dev --location eastus2

# Deploy infrastructure
az deployment group create \
  --resource-group rg-awcs-dev \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/dev.bicepparam
```

#### 2. Deploy to Staging Environment

```bash
# Create resource group  
az group create --name rg-awcs-staging --location eastus2

# Deploy infrastructure
az deployment group create \
  --resource-group rg-awcs-staging \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/staging.bicepparam
```

#### 3. Deploy to Production Environment

```bash
# Create resource group
az group create --name rg-awcs-prod --location eastus2

# Deploy infrastructure
az deployment group create \
  --resource-group rg-awcs-prod \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/prod.bicepparam
```

### Automated Deployment (GitHub Actions)

The infrastructure is automatically deployed using GitHub Actions workflow (`.github/workflows/deploy-infrastructure.yml`).

#### Workflow Triggers:
- **Push to main branch** with changes to `infra/bicep/**` → Deploy to Development
- **Manual workflow dispatch** → Deploy to selected environment (dev/staging/prod)

#### Required GitHub Secrets:
- `AZURE_CLIENT_ID` - Azure AD App Registration Client ID
- `AZURE_TENANT_ID` - Azure AD Tenant ID  
- `AZURE_SUBSCRIPTION_ID` - Azure Subscription ID

#### Setting up OIDC Authentication:

1. Create an Azure AD App Registration
2. Create a federated credential for GitHub Actions
3. Assign appropriate permissions to the service principal
4. Configure the GitHub secrets

## 🔧 Validation and Testing

### Template Validation

```bash
# Validate main template
az bicep build --file infra/bicep/main.bicep --stdout > /dev/null

# Validate all modules
for module in infra/bicep/modules/*/main.bicep; do
  echo "Validating $module..."
  az bicep build --file "$module" --stdout > /dev/null
done
```

### What-If Deployment

```bash
# Preview changes before deployment
az deployment group what-if \
  --resource-group rg-awcs-dev \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/dev.bicepparam
```

## 🔐 Security Features

### Security Best Practices Implemented:

- **Managed Identity** authentication for Container Apps
- **RBAC assignments** for service-to-service communication
- **Key Vault** integration for secrets management
- **Diagnostic settings** enabled for all supported resources
- **HTTPS only** enforcement on all web services
- **Minimal TLS 1.2** requirement
- **Network access** restricted where possible
- **Soft delete** and **purge protection** on Key Vault
- **No public blob access** on Storage Accounts

### Role Assignments:

- Container Apps → Container Registry (AcrPull)
- **GitHub Actions Service Principal → Container Registry (AcrPush)**
- API Container App → Storage Account (Storage Blob Data Contributor)
- API Container App → Cosmos DB (Cosmos DB Built-in Data Contributor)
- API Container App → Key Vault (Key Vault Secrets User)

## 📊 Monitoring and Observability

### Diagnostic Settings:
All resources export:
- **All available metrics** to Log Analytics
- **All log categories** via diagnostic settings

### Key Metrics Monitored:
- Application performance and availability
- Storage usage and costs
- Database performance and request units
- Container resource utilization
- Security events and access patterns

## 🏷️ Resource Tags

All resources are tagged with:
- `Application`: "Azure Web Content Share"
- `Environment`: Environment name (Development/Staging/Production) 
- `CreatedBy`: "Bicep"
- `CostCenter`: "Engineering"

## 💰 Cost Optimization

### Serverless and Consumption-Based Services:
- **Cosmos DB Serverless** - Pay per operation
- **Container Apps Consumption** - Pay per usage
- **Log Analytics** - Pay per GB ingested
- **Storage Account** - Pay per GB stored

### Resource Tier Selection:
- **Development**: Basic/Standard tiers
- **Staging**: Standard tiers  
- **Production**: Premium tiers where needed

## 🔄 Updates and Maintenance

### Updating Infrastructure:
1. Modify Bicep templates
2. Test changes in development environment
3. Create pull request with changes
4. Deploy through CI/CD pipeline
5. Monitor deployment and validate functionality

### Version Management:
- Use semantic versioning for infrastructure releases
- Tag deployments with version numbers
- Maintain deployment history in Log Analytics

## 📞 Support

For issues or questions regarding the infrastructure:
1. Check Azure resource health and diagnostics
2. Review Log Analytics for error patterns
3. Consult deployment history in Azure Portal
4. Review GitHub Actions workflow logs