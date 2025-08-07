using AzureWebContentShare.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureWebContentShare.Api.Configuration;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Implementation of cleanup job state service using Cosmos DB
/// </summary>
public class CleanupJobStateService : ICleanupJobStateService
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<CleanupJobStateService> _logger;
    private readonly AzureOptions _azureOptions;
    private Container? _container;

    public CleanupJobStateService(
        CosmosClient cosmosClient,
        ILogger<CleanupJobStateService> logger,
        IOptions<AzureOptions> azureOptions)
    {
        _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _azureOptions = azureOptions?.Value ?? throw new ArgumentNullException(nameof(azureOptions));
    }

    /// <summary>
    /// Gets the Cosmos DB container for job state storage
    /// </summary>
    private async Task<Container> GetContainerAsync()
    {
        if (_container != null)
        {
            return _container;
        }

        try
        {
            // Get the database (should already exist from infrastructure deployment)
            var database = _cosmosClient.GetDatabase(_azureOptions.CosmosDb.DatabaseName);

            // Get the JobState container (should already exist from infrastructure deployment)
            _container = database.GetContainer("JobState");
            
            return _container;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Cosmos DB container for job state. Ensure the JobState container exists in database {DatabaseName}", 
                _azureOptions.CosmosDb.DatabaseName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<CleanupJobState> GetStateAsync()
    {
        try
        {
            var container = await GetContainerAsync();
            
            // Use point read for maximum efficiency
            var response = await container.ReadItemAsync<CleanupJobState>(
                CleanupJobState.DocumentId, 
                new PartitionKey("system"));
            
            _logger.LogDebug("Retrieved cleanup job state with last processed timestamp: {Timestamp}", 
                response.Resource.LastProcessedTimestamp);
            
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Return initial state if document doesn't exist
            var initialState = new CleanupJobState
            {
                LastProcessedTimestamp = DateTimeOffset.UtcNow.AddDays(-1), // Start from yesterday
                LastUpdated = DateTimeOffset.UtcNow,
                LastRunProcessedCount = 0,
                LastRunTimestamp = DateTimeOffset.UtcNow
            };
            
            _logger.LogInformation("No existing cleanup job state found, returning initial state with timestamp: {Timestamp}", 
                initialState.LastProcessedTimestamp);
            
            return initialState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get cleanup job state");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateStateAsync(CleanupJobState state)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        try
        {
            var container = await GetContainerAsync();
            
            // Update the last updated timestamp
            state.LastUpdated = DateTimeOffset.UtcNow;
            
            // Use upsert for efficiency - creates if not exists, updates if exists
            await container.UpsertItemAsync(
                state, 
                new PartitionKey(state.PartitionKey));
            
            _logger.LogDebug("Updated cleanup job state with last processed timestamp: {Timestamp}, processed count: {Count}", 
                state.LastProcessedTimestamp, state.LastRunProcessedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update cleanup job state");
            throw;
        }
    }
}