#!/bin/bash
# Azure Web Content Share - GitHub Actions Service Principal Validation Script
# This script helps validate that the Service Principal has the required AcrPush permissions

set -e

echo "🔍 Azure Web Content Share - Service Principal Validation"
echo "========================================================"

# Check if required parameters are provided
if [ -z "$1" ]; then
    echo "❌ Error: Please provide the Azure Client ID (from AZURE_CLIENT_ID secret)"
    echo "Usage: $0 <AZURE_CLIENT_ID> [environment]"
    echo "Example: $0 12345678-1234-1234-1234-123456789012 dev"
    exit 1
fi

CLIENT_ID="$1"
ENVIRONMENT="${2:-dev}"

echo "📋 Configuration:"
echo "   Client ID: $CLIENT_ID"
echo "   Environment: $ENVIRONMENT"
echo ""

# Get Service Principal Object ID
echo "🔍 Finding Service Principal Object ID..."
OBJECT_ID=$(az ad sp show --id "$CLIENT_ID" --query id -o tsv 2>/dev/null || echo "")

if [ -z "$OBJECT_ID" ]; then
    echo "❌ Error: Could not find Service Principal with Client ID: $CLIENT_ID"
    echo "   Please verify the Client ID is correct and you have permissions to query it."
    exit 1
fi

echo "✅ Service Principal Object ID: $OBJECT_ID"
echo ""

# Check resource group
RESOURCE_GROUP="rg-awcs-$ENVIRONMENT"
echo "🔍 Checking resource group: $RESOURCE_GROUP"

if ! az group show --name "$RESOURCE_GROUP" >/dev/null 2>&1; then
    echo "❌ Error: Resource group '$RESOURCE_GROUP' not found"
    echo "   Please ensure the infrastructure has been deployed to this environment."
    exit 1
fi

echo "✅ Resource group exists"
echo ""

# Get Container Registry name
echo "🔍 Finding Container Registry..."
ACR_NAME=$(az acr list --resource-group "$RESOURCE_GROUP" --query "[0].name" -o tsv 2>/dev/null || echo "")

if [ -z "$ACR_NAME" ]; then
    echo "❌ Error: Could not find Container Registry in resource group: $RESOURCE_GROUP"
    echo "   Please ensure the infrastructure has been deployed properly."
    exit 1
fi

echo "✅ Container Registry: $ACR_NAME"
echo ""

# Check if AcrPush role assignment exists
echo "🔍 Checking AcrPush role assignment..."
ACR_PUSH_ROLE_ID="8311e382-0749-4cb8-b61a-304f252e45ec"

ASSIGNMENT_EXISTS=$(az role assignment list \
    --assignee "$OBJECT_ID" \
    --role "$ACR_PUSH_ROLE_ID" \
    --resource-group "$RESOURCE_GROUP" \
    --query "length(@)" -o tsv 2>/dev/null || echo "0")

if [ "$ASSIGNMENT_EXISTS" -eq "0" ]; then
    echo "❌ Missing AcrPush role assignment for Service Principal"
    echo "   Object ID: $OBJECT_ID"
    echo "   Role: AcrPush ($ACR_PUSH_ROLE_ID)"
    echo ""
    echo "🛠️  To fix this issue:"
    echo "   1. Update the parameter file: infra/bicep/parameters/$ENVIRONMENT.bicepparam"
    echo "   2. Replace 'REPLACE_WITH_ACTUAL_SERVICE_PRINCIPAL_OBJECT_ID' with: $OBJECT_ID"
    echo "   3. Redeploy the infrastructure"
    echo ""
    exit 1
fi

echo "✅ AcrPush role assignment exists"
echo ""

# Test ACR login (if possible)
echo "🧪 Testing ACR access..."
if az acr login --name "$ACR_NAME" --expose-token >/dev/null 2>&1; then
    echo "✅ ACR login successful"
else
    echo "⚠️  Warning: Could not test ACR login (may require different authentication context)"
fi

echo ""
echo "🎉 Validation Complete!"
echo "   Service Principal $CLIENT_ID ($OBJECT_ID) is properly configured for AcrPush access."
echo "   Container Registry: $ACR_NAME"
echo "   Environment: $ENVIRONMENT"