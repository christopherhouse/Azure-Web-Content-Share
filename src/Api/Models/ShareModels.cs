namespace AzureWebContentShare.Api.Models;

/// <summary>
/// Request model for uploading and sharing a file
/// </summary>
public class ShareFileRequest
{
    /// <summary>
    /// Email address of the recipient
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of hours until the share expires (default: 24 hours)
    /// </summary>
    public int ExpirationHours { get; set; } = 24;
}

/// <summary>
/// Response model for file sharing
/// </summary>
public class ShareFileResponse
{
    /// <summary>
    /// Unique share ID
    /// </summary>
    public string ShareId { get; set; } = string.Empty;
    
    /// <summary>
    /// Share code for the recipient
    /// </summary>
    public string ShareCode { get; set; } = string.Empty;
    
    /// <summary>
    /// When the share expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Filename that was shared
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Request model for accessing a shared file
/// </summary>
public class AccessFileRequest
{
    /// <summary>
    /// Share code provided to the recipient
    /// </summary>
    public string ShareCode { get; set; } = string.Empty;
}