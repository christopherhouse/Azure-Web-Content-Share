using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AzureWebContentShare.Api.Services;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Azure.Security.KeyVault.Secrets;
using Moq;
using Microsoft.Extensions.Configuration;

namespace AzureWebContentShare.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory for testing that replaces Azure services with mocks
/// </summary>
public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Add test configuration to prevent Azure service initialization
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Azure:Storage:BlobEndpoint"] = "https://test.blob.core.windows.net/",
                ["Azure:CosmosDb:Endpoint"] = "https://test.documents.azure.com:443/",
                ["Azure:KeyVault:Uri"] = "https://test.vault.azure.net/"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove Azure service registrations
            services.RemoveAll<BlobServiceClient>();
            services.RemoveAll<CosmosClient>();
            services.RemoveAll<SecretClient>();

            // Replace with mocks
            var mockBlobServiceClient = new Mock<BlobServiceClient>();
            var mockCosmosClient = new Mock<CosmosClient>();
            var mockSecretClient = new Mock<SecretClient>();

            services.AddSingleton(mockBlobServiceClient.Object);
            services.AddSingleton(mockCosmosClient.Object);
            services.AddSingleton(mockSecretClient.Object);

            // Replace application services with mocks
            services.RemoveAll<IEncryptionService>();
            services.RemoveAll<IFileShareService>();
            services.RemoveAll<IUserService>();

            var mockEncryptionService = new Mock<IEncryptionService>();
            var mockFileShareService = new Mock<IFileShareService>();
            var mockUserService = new Mock<IUserService>();

            services.AddSingleton(mockEncryptionService.Object);
            services.AddSingleton(mockFileShareService.Object);
            services.AddSingleton(mockUserService.Object);
        });
    }
}