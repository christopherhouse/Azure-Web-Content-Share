# Azure Web Content Share 🌐

A secure, modern web application for sharing files built with .NET 8 and Vue.js, deployed on Azure Container Apps.

## 🏗️ Architecture

This solution provides a complete cloud-native file sharing platform with:

### Backend
- **.NET 8 Minimal API** - High-performance REST API
- **Azure Container Apps** - Serverless container hosting
- **Azure Cosmos DB Serverless** - Metadata storage
- **Azure Blob Storage** - Secure file storage
- **Azure Key Vault** - Secrets management

### Frontend  
- **Vue.js 3** - Modern, engaging user interface
- **TypeScript** - Type-safe development
- **Progressive Web App** - Mobile-friendly experience

### Infrastructure
- **Bicep Templates** - Infrastructure as Code
- **GitHub Actions** - CI/CD automation
- **Azure Container Registry** - Container image storage
- **Application Insights** - Performance monitoring

## 🚀 Quick Start

### Prerequisites
- Azure CLI installed and configured
- Docker Desktop (for local development)
- .NET 8 SDK
- Node.js 18+ LTS

### Deploy Infrastructure

```bash
# Manual deployment
./deploy-infrastructure.sh dev

# Or using Azure CLI directly
az group create --name rg-awcs-dev --location eastus2
az deployment group create \
  --resource-group rg-awcs-dev \
  --template-file infra/bicep/main.bicep \
  --parameters @infra/bicep/parameters/dev.bicepparam
```

### Local Development

```bash
# Backend
dotnet run --project src/Api

# Frontend  
cd src/Frontend
npm install
npm run dev
```

## 📁 Project Structure

```
Azure-Web-Content-Share/
├── infra/                          # Infrastructure as Code
│   ├── bicep/                      # Bicep templates
│   │   ├── main.bicep             # Main deployment template
│   │   ├── modules/               # Individual resource modules
│   │   └── parameters/            # Environment-specific parameters
│   └── README.md                   # Infrastructure documentation
├── src/                            # Application source code (future)
│   ├── Api/                       # .NET 8 backend API
│   └── Frontend/                  # Vue.js frontend
├── .github/workflows/              # GitHub Actions CI/CD
└── docs/                          # Documentation (future)
```

## 🔧 Infrastructure Details

The infrastructure consists of modular Bicep templates following Azure Well-Architected Framework principles:

### Core Services
- **Container Apps Environment** - Serverless container hosting
- **Container Registry** - Secure image storage  
- **Storage Account** - File storage with private blob containers
- **Cosmos DB** - Serverless NoSQL database for metadata
- **Key Vault** - Secure secrets and configuration management

### Observability Stack
- **Log Analytics Workspace** - Centralized logging
- **Application Insights** - APM and user analytics
- **Diagnostic Settings** - Comprehensive monitoring

### Security Features
- **Managed Identity** authentication
- **RBAC role assignments** for service-to-service access
- **HTTPS-only** enforcement
- **Private networking** where applicable
- **Comprehensive audit logging**

## 🔐 Security

Security is built into every layer:

- **Zero-trust architecture** with managed identities
- **Encrypted storage** at rest and in transit
- **Network isolation** and private endpoints
- **Comprehensive audit trails**
- **Automated vulnerability scanning**

## 📊 Monitoring

Comprehensive observability with:

- **Application performance monitoring**
- **Infrastructure health tracking** 
- **Security event monitoring**
- **Cost optimization insights**
- **Custom business metrics**

## 🚀 Deployment

### Automated (Recommended)
Infrastructure deploys automatically via GitHub Actions:
- **Push to main** → Deploy to Development
- **Manual dispatch** → Deploy to any environment

### Manual Deployment
Use the provided deployment script:
```bash
./deploy-infrastructure.sh [environment]
```

## 🏷️ Resource Naming

Resources follow Azure naming best practices:
- Format: `[type]-[app]-[unique-token]`
- Example: `st-awcs-a1b2c3d4` (Storage Account)
- Unique tokens prevent naming conflicts

## 💰 Cost Optimization

Designed for cost efficiency:
- **Serverless services** - Pay only for usage
- **Consumption-based** Container Apps
- **Serverless Cosmos DB** - No idle costs
- **Lifecycle policies** on storage

## 🔄 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📞 Support

For issues and questions:
- Check the [Infrastructure Documentation](infra/README.md)
- Review Azure resource health
- Consult deployment logs
- Create a GitHub issue

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Built with ❤️ using Azure, .NET 8, and Vue.js**
