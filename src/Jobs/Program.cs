using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AzureWebContentShare.Api.Configuration;
using AzureWebContentShare.Api.Services;

namespace AzureWebContentShare.Jobs;

/// <summary>
/// Console application for running scheduled cleanup jobs
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Starting expired share cleanup job");
            
            var fileShareService = scope.ServiceProvider.GetRequiredService<IFileShareService>();
            var cleanupJobStateService = scope.ServiceProvider.GetRequiredService<ICleanupJobStateService>();
            var cleanedCount = await fileShareService.CleanupExpiredSharesAsync(cleanupJobStateService);
            
            logger.LogInformation("Cleanup job completed successfully. Cleaned up {Count} expired shares", cleanedCount);
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cleanup job failed with error");
            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                var configuration = context.Configuration;
                
                // Configure options
                services.Configure<AzureOptions>(configuration.GetSection(AzureOptions.SectionName));
                
                // Configure Azure services using Managed Identity
                var credential = new DefaultAzureCredential();
                
                // Configure Azure Blob Storage
                services.AddSingleton(serviceProvider =>
                {
                    var azureOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOptions>>().Value;
                    return new BlobServiceClient(new Uri(azureOptions.Storage.BlobEndpoint), credential);
                });
                
                // Configure Azure Cosmos DB
                services.AddSingleton(serviceProvider =>
                {
                    var azureOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOptions>>().Value;
                    return new CosmosClient(azureOptions.CosmosDb.Endpoint, credential);
                });
                
                // Configure Azure Key Vault
                services.AddSingleton(serviceProvider =>
                {
                    var azureOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureOptions>>().Value;
                    return new SecretClient(new Uri(azureOptions.KeyVault.Uri), credential);
                });
                
                // Register application services
                services.AddScoped<IEncryptionService, EncryptionService>();
                services.AddScoped<IFileShareService, FileShareService>();
                services.AddScoped<ICleanupJobStateService, CleanupJobStateService>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddApplicationInsights();
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));
            });
}