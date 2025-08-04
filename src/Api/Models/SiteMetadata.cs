namespace AzureWebContentShare.Api.Models;

/// <summary>
/// Represents site-wide metadata and configuration
/// </summary>
public class SiteMetadata
{
    /// <summary>
    /// Fixed site metadata ID - there should only be one site metadata record
    /// </summary>
    public string Id { get; set; } = "site";
    
    /// <summary>
    /// Whether the site has been claimed by an administrator
    /// </summary>
    public bool IsClaimed { get; set; } = false;
    
    /// <summary>
    /// User ID of the administrator who claimed the site
    /// </summary>
    public string? ClaimedByUserId { get; set; }
    
    /// <summary>
    /// When the site was claimed
    /// </summary>
    public DateTime? ClaimedAt { get; set; }
    
    /// <summary>
    /// Site name/title (for future use)
    /// </summary>
    public string SiteName { get; set; } = "Azure Web Content Share";
    
    /// <summary>
    /// When this metadata record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this metadata record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Response model for site status
/// </summary>
public class SiteStatusResponse
{
    /// <summary>
    /// Whether the site has been claimed by an administrator
    /// </summary>
    public bool IsClaimed { get; set; }
    
    /// <summary>
    /// Site name
    /// </summary>
    public string SiteName { get; set; } = string.Empty;
}

/// <summary>
/// Response model for site claiming
/// </summary>
public class ClaimSiteResponse
{
    /// <summary>
    /// Whether the claiming was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Message describing the result
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// When the site was claimed
    /// </summary>
    public DateTime ClaimedAt { get; set; }
}