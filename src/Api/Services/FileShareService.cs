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
    public async Task<int> CleanupExpiredSharesAsync()
    {
        try
        {
            var container = await GetCosmosContainerAsync();
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.expiresAt < @now AND c.isDeleted = false")
                .WithParameter("@now", DateTime.UtcNow);

            var iterator = container.GetItemQueryIterator<FileShareMetadata>(query);
            var cleanedCount = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var metadata in response)
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
                }
            }

            _logger.LogInformation("Cleaned up {Count} expired shares", cleanedCount);
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