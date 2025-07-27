namespace AzureWebContentShare.Api.Models;

/// <summary>
/// Represents the current authenticated user
/// </summary>
public class CurrentUser
{
    /// <summary>
    /// Unique user identifier
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
    /// Azure AD tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// When the user was last authenticated
    /// </summary>
    public DateTime AuthenticatedAt { get; set; } = DateTime.UtcNow;
}