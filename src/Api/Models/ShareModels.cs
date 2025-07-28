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
    
    /// <summary>
    /// Optional message to include with the share
    /// </summary>
    public string? Message { get; set; }
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
    
    /// <summary>
    /// Download URL for the recipient
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;
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

/// <summary>
/// Response model for user profile information
/// </summary>
public class UserProfileResponse
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// User's role in the system
    /// </summary>
    public UserRole Role { get; set; }
    
    /// <summary>
    /// When the user was last authenticated
    /// </summary>
    public DateTime AuthenticatedAt { get; set; }
}