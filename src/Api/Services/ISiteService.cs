using AzureWebContentShare.Api.Models;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Service for managing site-wide configuration and metadata
/// </summary>
public interface ISiteService
{
    /// <summary>
    /// Get the current site metadata
    /// </summary>
    /// <returns>Site metadata or null if not found</returns>
    Task<SiteMetadata?> GetSiteMetadataAsync();
    
    /// <summary>
    /// Check if the site has been claimed by an administrator
    /// </summary>
    /// <returns>True if the site has been claimed</returns>
    Task<bool> IsSiteClaimedAsync();
    
    /// <summary>
    /// Claim the site for the current user (making them the first administrator)
    /// </summary>
    /// <param name="userId">User ID of the person claiming the site</param>
    /// <returns>Updated site metadata</returns>
    Task<SiteMetadata> ClaimSiteAsync(string userId);
    
    /// <summary>
    /// Initialize site metadata if it doesn't exist
    /// </summary>
    /// <returns>Site metadata</returns>
    Task<SiteMetadata> InitializeSiteMetadataAsync();
}