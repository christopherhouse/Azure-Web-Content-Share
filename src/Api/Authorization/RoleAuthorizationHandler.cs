using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace AzureWebContentShare.Api.Authorization;

/// <summary>
/// Custom authorization requirement for role-based access control
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Required user role
    /// </summary>
    public UserRole RequiredRole { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="requiredRole">Required role</param>
    public RoleRequirement(UserRole requiredRole)
    {
        RequiredRole = requiredRole;
    }
}

/// <summary>
/// Authorization handler for role requirements
/// </summary>
public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
{
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RoleAuthorizationHandler> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    public RoleAuthorizationHandler(
        IUserService userService, 
        IHttpContextAccessor httpContextAccessor,
        ILogger<RoleAuthorizationHandler> logger)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Handle the authorization requirement
    /// </summary>
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RoleRequirement requirement)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HTTP context is null during authorization");
                context.Fail();
                return;
            }

            var currentUser = await _userService.GetCurrentUserAsync(httpContext);
            if (currentUser == null)
            {
                _logger.LogWarning("No current user found for authorization");
                context.Fail();
                return;
            }

            if (_userService.HasRole(currentUser, requirement.RequiredRole))
            {
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("User {UserId} does not have required role {RequiredRole}. User role: {UserRole}", 
                    currentUser.Id, requirement.RequiredRole, currentUser.Role);
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during role authorization");
            context.Fail();
        }
    }
}

/// <summary>
/// Authorization policies constants
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Policy for administrators only
    /// </summary>
    public const string AdministratorOnly = "AdministratorOnly";
    
    /// <summary>
    /// Policy for content owners (includes administrators)
    /// </summary>
    public const string ContentOwnerOnly = "ContentOwnerOnly";
    
    /// <summary>
    /// Policy for authenticated users (any role)
    /// </summary>
    public const string AuthenticatedUser = "AuthenticatedUser";
}