using AzureWebContentShare.Api.Models;
using Microsoft.Azure.Cosmos;
using System.Security.Claims;
using AzureWebContentShare.Api.Configuration;
using Microsoft.Extensions.Options;

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
    
    /// <summary>
    /// Create or update user in the database
    /// </summary>
    /// <param name="user">User to create or update</param>
    /// <returns>The created or updated user</returns>
    Task<CurrentUser> CreateOrUpdateUserAsync(CurrentUser user);
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User or null if not found</returns>
    Task<CurrentUser?> GetUserByIdAsync(string userId);
}

/// <summary>
/// Implementation of user service with Entra ID integration
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IFileShareService _fileShareService;
    private readonly CosmosClient _cosmosClient;
    private readonly AzureOptions _azureOptions;
    private readonly Container _usersContainer;

    /// <summary>
    /// Constructor for UserService
    /// </summary>
    public UserService(
        ILogger<UserService> logger, 
        IFileShareService fileShareService,
        CosmosClient cosmosClient,
        IOptions<AzureOptions> azureOptions)
    {
        _logger = logger;
        _fileShareService = fileShareService;
        _cosmosClient = cosmosClient;
        _azureOptions = azureOptions.Value;
        
        var database = _cosmosClient.GetDatabase(_azureOptions.CosmosDb.DatabaseName);
        _usersContainer = database.GetContainer("users");
    }

    /// <inheritdoc/>
    public async Task<CurrentUser?> GetCurrentUserAsync(HttpContext context)
    {
        try
        {
            var user = context.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                return null;
            }

            // Extract claims from JWT token
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                        user.FindFirst("oid")?.Value ?? 
                        user.FindFirst("sub")?.Value;
                        
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? 
                       user.FindFirst("email")?.Value ?? 
                       user.FindFirst("preferred_username")?.Value;
                       
            var name = user.FindFirst(ClaimTypes.Name)?.Value ?? 
                      user.FindFirst("name")?.Value ?? 
                      email;
                      
            var tenantId = user.FindFirst("tid")?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("JWT token missing required claims (userId or email)");
                return null;
            }

            // Try to get user from database first
            var existingUser = await GetUserByIdAsync(userId);
            if (existingUser != null)
            {
                // Update last login time
                existingUser.AuthenticatedAt = DateTime.UtcNow;
                await CreateOrUpdateUserAsync(existingUser);
                return existingUser;
            }

            // Create new user with default ContentOwner role
            var newUser = new CurrentUser
            {
                Id = userId,
                Email = email,
                Name = name,
                Role = UserRole.ContentOwner, // Default role - admins need to be promoted
                TenantId = tenantId,
                AuthenticatedAt = DateTime.UtcNow
            };

            return await CreateOrUpdateUserAsync(newUser);
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
        // Administrator has access to everything
        if (user.Role == UserRole.Administrator)
            return true;
            
        return user.Role == requiredRole;
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
            _logger.LogError(ex, "Failed to validate share code access for {ShareCode}", shareCode);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<CurrentUser> CreateOrUpdateUserAsync(CurrentUser user)
    {
        try
        {
            var response = await _usersContainer.UpsertItemAsync(user, new Microsoft.Azure.Cosmos.PartitionKey(user.Id));
            return response.Resource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or update user {UserId}", user.Id);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<CurrentUser?> GetUserByIdAsync(string userId)
    {
        try
        {
            var response = await _usersContainer.ReadItemAsync<CurrentUser>(userId, new Microsoft.Azure.Cosmos.PartitionKey(userId));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by ID {UserId}", userId);
            throw;
        }
    }
}