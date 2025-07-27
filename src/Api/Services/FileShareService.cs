using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using AzureWebContentShare.Api.Configuration;
using AzureWebContentShare.Api.Models;
using Microsoft.Extensions.Options;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Implementation of file share service using Azure Blob Storage and Cosmos DB
/// </summary>
public class FileShareService : IFileShareService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly CosmosClient _cosmosClient;
    private readonly IEncryptionService _encryptionService;
    private readonly AzureOptions _azureOptions;
    private readonly ILogger<FileShareService> _logger;
    private Container? _cosmosContainer;

    public FileShareService(
        BlobServiceClient blobServiceClient,
        CosmosClient cosmosClient,
        IEncryptionService encryptionService,
        IOptions<AzureOptions> azureOptions,
        ILogger<FileShareService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _cosmosClient = cosmosClient;
        _encryptionService = encryptionService;
        _azureOptions = azureOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ShareFileResponse> ShareFileAsync(IFormFile file, ShareFileRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("Starting file share process for user {UserId}, file {FileName}", userId, file.FileName);

            // Generate unique identifiers
            var shareId = Guid.NewGuid().ToString();
            var shareCode = _encryptionService.GenerateShareCode();
            var blobName = $"{userId}/{shareId}/{file.FileName}";

            // Upload file to blob storage
            var blobContainer = _blobServiceClient.GetBlobContainerClient(_azureOptions.Storage.ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            var blobClient = blobContainer.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            // Create metadata record
            var metadata = new FileShareMetadata
            {
                Id = shareId,
                UserId = userId,
                FileName = file.FileName,
                BlobPath = blobName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                RecipientEmail = request.RecipientEmail,
                EncryptedShareCode = await _encryptionService.EncryptAsync(shareCode),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(request.ExpirationHours)
            };

            // Store metadata in Cosmos DB
            var container = await GetCosmosContainerAsync();
            await container.CreateItemAsync(metadata, new PartitionKey(userId));

            _logger.LogInformation("File share created successfully. ShareId: {ShareId}", shareId);

            return new ShareFileResponse
            {
                ShareId = shareId,
                ShareCode = shareCode,
                ExpiresAt = metadata.ExpiresAt,
                FileName = file.FileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create file share for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<FileShareMetadata?> GetFileByShareCodeAsync(string shareCode)
    {
        try
        {
            var container = await GetCosmosContainerAsync();
            var encryptedShareCode = await _encryptionService.EncryptAsync(shareCode);

            // Query by encrypted share code
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.encryptedShareCode = @shareCode AND c.expiresAt > @now AND c.isDeleted = false")
                .WithParameter("@shareCode", encryptedShareCode)
                .WithParameter("@now", DateTime.UtcNow);

            var iterator = container.GetItemQueryIterator<FileShareMetadata>(query);
            var results = await iterator.ReadNextAsync();

            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file by share code");
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<(Stream? stream, string? fileName, string? contentType)> DownloadFileAsync(string shareCode)
    {
        try
        {
            var metadata = await GetFileByShareCodeAsync(shareCode);
            if (metadata == null)
            {
                return (null, null, null);
            }

            var blobContainer = _blobServiceClient.GetBlobContainerClient(_azureOptions.Storage.ContainerName);
            var blobClient = blobContainer.GetBlobClient(metadata.BlobPath);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found for share {ShareId}", metadata.Id);
                return (null, null, null);
            }

            var stream = await blobClient.OpenReadAsync();
            return (stream, metadata.FileName, metadata.ContentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file for share code");
            return (null, null, null);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FileShareMetadata>> GetUserSharesAsync(string userId)
    {
        try
        {
            var container = await GetCosmosContainerAsync();
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId AND c.isDeleted = false ORDER BY c.createdAt DESC")
                .WithParameter("@userId", userId);

            var iterator = container.GetItemQueryIterator<FileShareMetadata>(query);
            var results = new List<FileShareMetadata>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user shares for user {UserId}", userId);
            return Enumerable.Empty<FileShareMetadata>();
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteShareAsync(string shareId, string userId)
    {
        try
        {
            var container = await GetCosmosContainerAsync();
            var metadata = await container.ReadItemAsync<FileShareMetadata>(shareId, new PartitionKey(userId));

            // Soft delete the metadata
            metadata.Resource.IsDeleted = true;
            metadata.Resource.UpdatedAt = DateTime.UtcNow;
            metadata.Resource.Ttl = 180 * 24 * 60 * 60; // 180 days in seconds

            await container.ReplaceItemAsync(metadata.Resource, shareId, new PartitionKey(userId));

            // Hard delete the blob
            var blobContainer = _blobServiceClient.GetBlobContainerClient(_azureOptions.Storage.ContainerName);
            var blobClient = blobContainer.GetBlobClient(metadata.Resource.BlobPath);
            await blobClient.DeleteIfExistsAsync();

            _logger.LogInformation("Share {ShareId} deleted successfully", shareId);
            return true;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Share {ShareId} not found for user {UserId}", shareId, userId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete share {ShareId}", shareId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupExpiredSharesAsync(ICleanupJobStateService cleanupJobStateService)
    {
        try
        {
            // Get the current state to determine high water mark
            var state = await cleanupJobStateService.GetStateAsync();
            var highWaterMark = state.LastProcessedTimestamp;
            var currentTimestamp = DateTimeOffset.UtcNow;
            
            _logger.LogInformation("Starting cleanup job. High water mark: {HighWaterMark}, Current time: {CurrentTime}", 
                highWaterMark, currentTimestamp);

            var container = await GetCosmosContainerAsync();
            
            // Query for shares that are expired AND have been updated since the last run
            // This ensures we only process shares that have changed or expired since last run
            var query = new QueryDefinition(@"
                SELECT * FROM c 
                WHERE c.expiresAt < @now 
                  AND c.isDeleted = false 
                  AND (c._ts > @highWaterMarkUnix OR c.expiresAt > @highWaterMark)
                ORDER BY c._ts ASC")
                .WithParameter("@now", DateTime.UtcNow)
                .WithParameter("@highWaterMark", highWaterMark.DateTime)
                .WithParameter("@highWaterMarkUnix", ((DateTimeOffset)highWaterMark).ToUnixTimeSeconds());

            var iterator = container.GetItemQueryIterator<FileShareMetadata>(query);
            var cleanedCount = 0;
            var lastProcessedTimestamp = highWaterMark;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var metadata in response)
                {
                    try
                    {
                        // Soft delete metadata
                        metadata.IsDeleted = true;
                        metadata.UpdatedAt = DateTime.UtcNow;
                        metadata.Ttl = 180 * 24 * 60 * 60; // 180 days

                        await container.ReplaceItemAsync(metadata, metadata.Id, new PartitionKey(metadata.UserId));

                        // Hard delete blob
                        var blobContainer = _blobServiceClient.GetBlobContainerClient(_azureOptions.Storage.ContainerName);
                        var blobClient = blobContainer.GetBlobClient(metadata.BlobPath);
                        await blobClient.DeleteIfExistsAsync();

                        cleanedCount++;
                        
                        // Update high water mark to the latest processed timestamp
                        // Use the Cosmos DB timestamp (_ts) converted to DateTimeOffset
                        if (metadata.UpdatedAt > lastProcessedTimestamp.DateTime)
                        {
                            lastProcessedTimestamp = new DateTimeOffset(metadata.UpdatedAt, TimeSpan.Zero);
                        }
                        
                        _logger.LogDebug("Cleaned up expired share {ShareId} for user {UserId}", 
                            metadata.Id, metadata.UserId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to cleanup share {ShareId} for user {UserId}", 
                            metadata.Id, metadata.UserId);
                        // Continue processing other shares even if one fails
                    }
                }
            }

            // Update the state with new high water mark and statistics
            state.LastProcessedTimestamp = lastProcessedTimestamp;
            state.LastRunProcessedCount = cleanedCount;
            state.LastRunTimestamp = currentTimestamp;
            await cleanupJobStateService.UpdateStateAsync(state);

            _logger.LogInformation("Cleanup job completed. Cleaned up {Count} expired shares. New high water mark: {NewHighWaterMark}", 
                cleanedCount, lastProcessedTimestamp);
            
            return cleanedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired shares");
            return 0;
        }
    }

    private async Task<Container> GetCosmosContainerAsync()
    {
        if (_cosmosContainer != null)
            return _cosmosContainer;

        var database = _cosmosClient.GetDatabase(_azureOptions.CosmosDb.DatabaseName);
        _cosmosContainer = database.GetContainer(_azureOptions.CosmosDb.ContainerName);
        return _cosmosContainer;
    }
}