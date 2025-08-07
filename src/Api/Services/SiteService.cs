using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Implementation of site service with Cosmos DB integration
/// </summary>
public class SiteService : ISiteService
{
    private readonly ILogger<SiteService> _logger;
    private readonly CosmosClient _cosmosClient;
    private readonly AzureOptions _azureOptions;
    private readonly Container _siteContainer;
    private readonly IUserService _userService;

    /// <summary>
    /// Constructor for SiteService
    /// </summary>
    public SiteService(
        ILogger<SiteService> logger,
        CosmosClient cosmosClient,
        IOptions<AzureOptions> azureOptions,
        IUserService userService)
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
        _azureOptions = azureOptions.Value;
        _userService = userService;
        
        var database = _cosmosClient.GetDatabase(_azureOptions.CosmosDb.DatabaseName);
        _siteContainer = database.GetContainer("Site");
    }

    /// <inheritdoc/>
    public async Task<SiteMetadata?> GetSiteMetadataAsync()
    {
        try
        {
            var response = await _siteContainer.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get site metadata");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsSiteClaimedAsync()
    {
        try
        {
            var metadata = await GetSiteMetadataAsync();
            return metadata?.IsClaimed ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if site is claimed");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SiteMetadata> ClaimSiteAsync(string userId)
    {
        try
        {
            var metadata = await GetSiteMetadataAsync();
            if (metadata == null)
            {
                metadata = await InitializeSiteMetadataAsync();
            }

            if (metadata.IsClaimed)
            {
                throw new InvalidOperationException("Site has already been claimed");
            }

            // Update the user to Administrator role
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.Role = UserRole.Administrator;
            await _userService.CreateOrUpdateUserAsync(user);

            // Mark site as claimed
            metadata.IsClaimed = true;
            metadata.ClaimedByUserId = userId;
            metadata.ClaimedAt = DateTime.UtcNow;
            metadata.UpdatedAt = DateTime.UtcNow;

            var response = await _siteContainer.UpsertItemAsync(metadata, new PartitionKey("site"));
            
            _logger.LogInformation("Site claimed by user {UserId} at {ClaimedAt}", userId, metadata.ClaimedAt);
            
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to claim site for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<SiteMetadata> InitializeSiteMetadataAsync()
    {
        try
        {
            var existingMetadata = await GetSiteMetadataAsync();
            if (existingMetadata != null)
            {
                return existingMetadata;
            }

            var metadata = new SiteMetadata
            {
                Id = "site",
                IsClaimed = false,
                SiteName = "Azure Web Content Share",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var response = await _siteContainer.CreateItemAsync(metadata, new PartitionKey("site"));
            
            _logger.LogInformation("Site metadata initialized");
            
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize site metadata");
            throw;
        }
    }
}