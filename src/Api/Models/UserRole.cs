namespace AzureWebContentShare.Api.Models;

/// <summary>
/// Represents the different user roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Administrator with full system access
    /// </summary>
    Administrator,
    
    /// <summary>
    /// Content owner who can upload and share content
    /// </summary>
    ContentOwner,
    
    /// <summary>
    /// Content recipient who can access shared content with a share code
    /// </summary>
    ContentRecipient
}