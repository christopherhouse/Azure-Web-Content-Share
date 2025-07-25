#!/bin/bash

# Azure Web Content Share - Infrastructure Deployment Script
# This script demonstrates how to deploy the infrastructure manually

set -e

# Configuration
SUBSCRIPTION_ID="your-subscription-id"
LOCATION="eastus2"
ENVIRONMENT=${1:-dev}
RESOURCE_GROUP_NAME="rg-awcs-${ENVIRONMENT}"

echo "🚀 Deploying Azure Web Content Share Infrastructure"
echo "Environment: $ENVIRONMENT"
echo "Resource Group: $RESOURCE_GROUP_NAME"
echo "Location: $LOCATION"
echo

# Check if Azure CLI is installed and user is logged in
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed. Please install it first."
    exit 1
fi

if ! az account show &> /dev/null; then
    echo "❌ You are not logged in to Azure CLI. Please run 'az login' first."
    exit 1
fi

echo "✅ Azure CLI is available and you are logged in"

# Set the subscription
if [ "$SUBSCRIPTION_ID" != "your-subscription-id" ]; then
    echo "Setting subscription to $SUBSCRIPTION_ID..."
    az account set --subscription "$SUBSCRIPTION_ID"
fi

echo "📋 Current subscription: $(az account show --query name -o tsv)"
echo

# Create resource group if it doesn't exist
echo "🏗️ Creating resource group '$RESOURCE_GROUP_NAME' in '$LOCATION'..."
az group create \
    --name "$RESOURCE_GROUP_NAME" \
    --location "$LOCATION" \
    --output table

echo

# Validate the deployment first
echo "🔍 Validating deployment..."
az deployment group validate \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --template-file "infra/bicep/main.bicep" \
    --parameters "@infra/bicep/parameters/${ENVIRONMENT}.bicepparam"

if [ $? -eq 0 ]; then
    echo "✅ Deployment validation successful"
else
    echo "❌ Deployment validation failed"
    exit 1
fi

echo

# Show what-if changes
echo "🔍 Showing what-if changes..."
az deployment group what-if \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --template-file "infra/bicep/main.bicep" \
    --parameters "@infra/bicep/parameters/${ENVIRONMENT}.bicepparam"

echo
read -p "Do you want to proceed with the deployment? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Deployment cancelled by user"
    exit 1
fi

# Deploy the infrastructure
echo "🚀 Deploying infrastructure..."
DEPLOYMENT_NAME="infra-${ENVIRONMENT}-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --template-file "infra/bicep/main.bicep" \
    --parameters "@infra/bicep/parameters/${ENVIRONMENT}.bicepparam" \
    --name "$DEPLOYMENT_NAME" \
    --output table

if [ $? -eq 0 ]; then
    echo
    echo "✅ Deployment completed successfully!"
    echo
    
    # Get deployment outputs
    echo "📊 Deployment outputs:"
    az deployment group show \
        --resource-group "$RESOURCE_GROUP_NAME" \
        --name "$DEPLOYMENT_NAME" \
        --query properties.outputs \
        --output table
        
    echo
    echo "🎉 Infrastructure deployment complete!"
    echo "Environment: $ENVIRONMENT"
    echo "Resource Group: $RESOURCE_GROUP_NAME"
    echo "Deployment Name: $DEPLOYMENT_NAME"
else
    echo "❌ Deployment failed"
    exit 1
fi