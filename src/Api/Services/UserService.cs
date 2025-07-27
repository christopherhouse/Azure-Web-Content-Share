using AzureWebContentShare.Api.Models;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Service for user authentication and authorization
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get the current user from the request context
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Current user information or null if not authenticated</returns>
    Task<CurrentUser?> GetCurrentUserAsync(HttpContext context);
    
    /// <summary>
    /// Validate if user has the required role
    /// </summary>
    /// <param name="user">Current user</param>
    /// <param name="requiredRole">Required role</param>
    /// <returns>True if user has the required role</returns>
    bool HasRole(CurrentUser user, UserRole requiredRole);
    
    /// <summary>
    /// Validate share code access (for content recipients)
    /// </summary>
    /// <param name="shareCode">Share code to validate</param>
    /// <returns>True if share code is valid and not expired</returns>
    Task<bool> ValidateShareCodeAccessAsync(string shareCode);
}

/// <summary>
/// Implementation of user service with Entra ID integration
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IFileShareService _fileShareService;

    public UserService(ILogger<UserService> logger, IFileShareService fileShareService)
    {
        _logger = logger;
        _fileShareService = fileShareService;
    }

    /// <inheritdoc/>
    public async Task<CurrentUser?> GetCurrentUserAsync(HttpContext context)
    {
        try
        {
            // For now, return a placeholder user until authentication is fully implemented
            // TODO: Implement proper Entra ID token parsing
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                return new CurrentUser
                {
                    Id = "temp-user-id",
                    Email = "user@example.com",
                    Name = "Test User",
                    Role = UserRole.ContentPublisher,
                    TenantId = "temp-tenant-id"
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user from context");
            return null;
        }
    }

    /// <inheritdoc/>
    public bool HasRole(CurrentUser user, UserRole requiredRole)
    {
        return user.Role == requiredRole || user.Role == UserRole.Administrator;
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateShareCodeAccessAsync(string shareCode)
    {
        try
        {
            var metadata = await _fileShareService.GetFileByShareCodeAsync(shareCode);
            return metadata != null && !metadata.IsDeleted && metadata.ExpiresAt > DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate share code access");
            return false;
        }
    }
}