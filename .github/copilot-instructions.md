# GitHub Copilot Instructions - Azure Web Content Share

This document provides comprehensive development guidelines and workflow instructions for the Azure Web Content Share solution. All developers and AI assistants (including GitHub Copilot and coding agents) should follow these standards when contributing to this project.

## 🏗️ Project Architecture Overview

This solution is a secure file sharing web application built with:
- **Backend**: .NET 8 minimal API
- **Frontend**: Vue.js with modern, engaging UI
- **Data Storage**: Azure Blob Storage (files) + Cosmos DB Serverless (metadata)
- **Hosting**: Azure Container Apps
- **Infrastructure**: Bicep templates
- **CI/CD**: GitHub Actions

## 🚀 Development Workflow Standards

### ⚠️ Implementation Standards

#### Complete Implementation Requirement
- **TODO/mock implementation is never successful** - All changes must be complete, working implementations
- **No placeholder code** - Every feature must be fully functional upon delivery
- **If unable to determine implementation** - Stop immediately and ask for guidance rather than providing incomplete solutions
- **All dependencies must be properly configured** - No missing configuration or setup steps
- **Error handling must be comprehensive** - Handle all expected and unexpected scenarios
- **Testing must be thorough** - All code paths must be tested and verified

#### When to Ask for Help
- When requirements are unclear or ambiguous
- When architectural decisions need clarification
- When external API documentation is insufficient
- When security implications are uncertain
- When performance requirements are not specified

### 1. .NET 8 Backend Development

#### API Structure & Patterns
- **Use minimal API approach** with endpoint classes for organization
- **Endpoint Classes**: Create dedicated endpoint classes that can be easily registered in `Program.cs`
- **Example Endpoint Class Pattern**:
```csharp
public static class FileEndpoints
{
    public static void MapFileEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/files").WithTags("Files");
        
        group.MapGet("/", GetFiles);
        group.MapPost("/", UploadFile);
        group.MapDelete("/{id}", DeleteFile);
    }
    
    private static async Task<IResult> GetFiles(IFileService fileService)
    {
        // Implementation
    }
}
```

#### Coding Standards
- **Follow Microsoft .NET coding standards** exactly
- Use **PascalCase** for public members, **camelCase** for private fields
- Apply **async/await** patterns consistently
- Implement **proper error handling** with structured logging
- Use **dependency injection** for all services
- Apply **configuration pattern** with strongly typed options

#### Security Requirements
- **Implement Secure Development Lifecycle (SDL) practices**
- **Input validation** on all endpoints using Data Annotations or FluentValidation
- **Authentication & Authorization** using Azure AD B2C or similar
- **HTTPS only** - reject HTTP requests
- **CORS configuration** - restrict to known origins
- **Rate limiting** implementation
- **SQL injection prevention** (parameterized queries)
- **XSS protection** with proper encoding
- **Secrets management** via Azure Key Vault only

#### Testing Requirements
- **All code except Program.cs must have unit tests**
- Use **xUnit testing framework**
- Use **FluentAssertions** for more readable and expressive assertions
- Use **AutoFixture** for test data generation and object creation
- **Minimum 80% code coverage** for business logic
- **Test naming convention**: `MethodName_Scenario_ExpectedResult`
- **Use test doubles** (mocks/stubs) for external dependencies with Moq
- **Example test structure**:
```csharp
public class FileServiceTests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly Fixture _fixture;
    private readonly FileService _sut;

    public FileServiceTests()
    {
        _mockDependency = new Mock<IDependency>();
        _fixture = new Fixture();
        _sut = new FileService(_mockDependency.Object);
    }

    [Theory]
    [AutoData]
    public async Task GetFileAsync_ValidId_ReturnsFile(string fileId)
    {
        // Arrange
        var expectedFile = _fixture.Create<FileModel>();
        _mockDependency.Setup(x => x.GetAsync(fileId))
                      .ReturnsAsync(expectedFile);

        // Act  
        var result = await _sut.GetFileAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedFile);
    }
}
```

#### Package Management
- **Open source NuGet packages only** if they are:
  - Popular (>1M downloads)
  - Well maintained (recent updates)
  - Have good documentation
  - Compatible with .NET 8

### 2. Vue.js Frontend Development

#### UI/UX Standards
- **Clean, modern, engaging design**
- **Consistent styling** throughout the application
- **Responsive design** for mobile and desktop
- **Accessibility compliance** (WCAG 2.1 AA)
- **Progressive Web App (PWA)** features where applicable

#### Development Patterns
- **Vue 3 Composition API** preferred over Options API
- **TypeScript** for type safety
- **Component-based architecture** with reusable components
- **State management** with Pinia for complex state
- **Vue Router** for navigation
- **Axios** for HTTP client with proper error handling

#### Testing Requirements
- **Unit tests** for all components using Vitest
- **UI tests** using Playwright for end-to-end scenarios
- **Test coverage minimum 70%** for components
- **Component testing** with Vue Test Utils
- **Example test structure**:
```javascript
describe('FileUpload.vue', () => {
  it('should upload file successfully', async () => {
    // Test implementation
  });
});
```

### 3. Azure Services Integration

#### Storage Strategy
- **Azure Blob Storage**: File storage with proper container organization
- **Cosmos DB Serverless**: Metadata storage with efficient querying
- **Partition key strategy** for optimal Cosmos DB performance

#### Configuration Management
- **Azure App Configuration Service** for all configuration
- **Feature flags** for progressive rollouts
- **Environment-specific configurations**

#### Security & Secrets
- **Azure Key Vault** for all secrets (connection strings, API keys)
- **Managed Identity** for service-to-service authentication
- **No secrets in code or configuration files**

#### Observability Requirements
- **Application Insights** for telemetry and performance monitoring
- **Log Analytics** for centralized logging
- **Custom metrics** for business KPIs
- **Structured logging** with correlation IDs
- **All Azure resources must export**:
  - All available metrics to Log Analytics
  - All log categories via diagnostic settings

### 4. Infrastructure as Code (Bicep)

#### Organization Structure
```
/infra/bicep/
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

#### Bicep Best Practices
- **Never return secrets as output variables**
- **Use parameter files** for environment-specific values
- **Modular design** - one module per Azure resource type
- **Consistent naming conventions** across all resources
- **Resource tagging** for cost management and organization
- **RBAC assignments** as part of infrastructure deployment
- **Diagnostic settings** enabled for all supported resources

#### Module Standards
- **Each module should**:
  - Have clear input parameters with descriptions
  - Include resource tags as parameters
  - Configure diagnostic settings where supported
  - Use latest API versions
  - Include outputs for dependent resources only (no secrets)

### 5. Container Strategy

#### Docker Configuration
- **Multi-stage builds** for optimized image sizes
- **Non-root user** execution for security
- **Minimal base images** (Alpine or Distroless)
- **Health checks** in Dockerfile
- **Azure Container Registry** for image storage

#### Container Apps Deployment
- **Separate container apps** for API and frontend
- **Auto-scaling configuration** based on HTTP traffic
- **Environment variables** from App Configuration and Key Vault
- **Ingress configuration** with proper routing

### 6. GitHub Actions CI/CD

#### Workflow Structure
```yaml
# Required stages:
1. Build Stage:
   - Compile/package code
   - Run unit tests
   - Publish test results
   - Build and push container images
   - Create deployment artifacts

2. Deploy Stage:
   - Deploy infrastructure (Bicep)
   - Deploy applications to Container Apps
   - Run smoke tests
   - Update monitoring dashboards
```

#### Best Practices
- **Environment separation** (dev, staging, prod)
- **Manual approval** for production deployments
- **Rollback capability** for failed deployments
- **Secrets management** via GitHub secrets and Azure Key Vault
- **Parallel execution** where possible to reduce deployment time
- **Status badges** in README for build status

### 7. Testing Strategy

#### Backend Testing
- **Unit Tests**: xUnit with Moq for mocking
- **Integration Tests**: Test with real Azure services (emulated)
- **API Tests**: Test all endpoints with various scenarios
- **Performance Tests**: Basic load testing for critical paths

#### Frontend Testing  
- **Unit Tests**: Vitest for component logic
- **Component Tests**: Vue Test Utils for component behavior
- **E2E Tests**: Playwright for complete user workflows
- **Visual Regression**: Capture and compare UI screenshots

#### Test Data Management
- **Test data isolation** between test runs
- **Cleanup routines** for integration tests
- **Mock external dependencies** in unit tests
- **Test environment provisioning** via infrastructure code

### 8. Documentation Standards

#### Code Documentation
- **XML documentation** for all public APIs
- **README files** in each major directory
- **Architecture Decision Records (ADRs)** for significant decisions
- **API documentation** generated from OpenAPI specifications

#### Repository Documentation
- **Clear, engaging README.md** with emojis and visual elements
- **Complete description** of purpose, contents, and usage
- **Setup instructions** for local development
- **Contributing guidelines** for external contributors

### 9. Security & Compliance

#### Security Checklist
- [ ] All secrets stored in Azure Key Vault
- [ ] HTTPS enforced everywhere
- [ ] Authentication required for all operations
- [ ] Input validation on all endpoints
- [ ] SQL injection prevention
- [ ] XSS protection implemented
- [ ] CORS properly configured
- [ ] Rate limiting implemented
- [ ] Security headers configured
- [ ] Dependencies scanned for vulnerabilities

#### Compliance Requirements
- **Data privacy** considerations for file sharing
- **Audit logging** for all file operations
- **Data retention policies** implementation
- **Geographic data residency** compliance

### 10. Development Environment

#### Required Tools
- **.NET 8 SDK**
- **Node.js LTS** for frontend development
- **Azure CLI** for Azure resource management
- **Docker Desktop** for containerization
- **Visual Studio Code** with recommended extensions

#### Recommended Extensions
- **C# Dev Kit** for .NET development
- **Vue Language Features** for Vue.js development
- **Azure Tools** extension pack
- **Bicep** extension for infrastructure code
- **GitHub Actions** extension

### 11. Code Review Guidelines

#### Review Checklist
- [ ] Code follows established patterns and standards
- [ ] Security requirements are met
- [ ] Tests are comprehensive and passing
- [ ] Documentation is updated
- [ ] No secrets are committed
- [ ] Performance implications considered
- [ ] Error handling is appropriate
- [ ] Logging is structured and meaningful

#### Pull Request Requirements
- **Clear description** of changes and rationale
- **Link to related issues** or user stories
- **Screenshots** for UI changes
- **Test results** included or linked
- **No merge** without passing CI/CD pipeline

### 12. Monitoring & Alerting

#### Key Metrics to Monitor
- **Application performance** (response times, throughput)
- **Error rates** and exception patterns
- **Storage usage** and costs
- **User engagement** metrics
- **Security events** and anomalies

#### Alerting Strategy
- **Critical alerts** for system outages
- **Warning alerts** for performance degradation
- **Info alerts** for capacity planning
- **Alert fatigue prevention** with proper thresholds

---

## 🎯 Quick Reference Commands

### Local Development
```bash
# Backend
dotnet run --project src/Api
dotnet test src/Tests

# Frontend  
npm run dev
npm run test
npm run e2e

# Infrastructure
az deployment group create --resource-group rg-name --template-file infra/bicep/main.bicep --parameters @infra/bicep/parameters/dev.bicepparam
```

### Container Operations
```bash
# Build images
docker build -t api:latest -f src/Api/Dockerfile .
docker build -t frontend:latest -f src/Frontend/Dockerfile .

# Run locally
docker-compose up -d
```

---

*This document should be updated as the project evolves. All team members are responsible for keeping these guidelines current and relevant.*