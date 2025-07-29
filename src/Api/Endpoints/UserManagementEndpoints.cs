using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Services;
using AzureWebContentShare.Api.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureWebContentShare.Api.Endpoints;

/// <summary>
/// Endpoints for user management (Administrator only)
/// </summary>
public static class UserManagementEndpoints
{
    /// <summary>
    /// Maps user management endpoints
    /// </summary>
    /// <param name="routes">Endpoint route builder</param>
    public static void MapUserManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/users")
            .WithTags("User Management")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.AdministratorOnly);

        // Get all users
        group.MapGet("/", GetAllUsersAsync)
            .WithName("GetAllUsers")
            .WithSummary("Get all users")
            .WithDescription("Get a list of all users in the system (Administrator only)")
            .Produces<IEnumerable<UserProfileResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);

        // Get user by ID
        group.MapGet("/{userId}", GetUserByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Get user by ID")
            .WithDescription("Get a specific user by their ID (Administrator only)")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // Update user role
        group.MapPut("/{userId}/role", UpdateUserRoleAsync)
            .WithName("UpdateUserRole")
            .WithSummary("Update user role")
            .WithDescription("Update a user's role (Administrator only)")
            .Accepts<UpdateUserRoleRequest>("application/json")
            .Produces<UserProfileResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // Delete user (soft delete)
        group.MapDelete("/{userId}", DeleteUserAsync)
            .WithName("DeleteUser")
            .WithSummary("Delete user")
            .WithDescription("Soft delete a user (Administrator only)")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        // Get all shared content (Administrator view)
        group.MapGet("/content", GetAllSharedContentAsync)
            .WithName("GetAllSharedContent")
            .WithSummary("Get all shared content")
            .WithDescription("Get all shared content across all users (Administrator only)")
            .Produces<IEnumerable<FileShareMetadata>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Get all users
    /// </summary>
    private static async Task<IResult> GetAllUsersAsync(
        IUserService userService,
        HttpContext context)
    {
        try
        {
            var currentUser = await userService.GetCurrentUserAsync(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }

            if (!userService.HasRole(currentUser, UserRole.Administrator))
            {
                return Results.Forbid();
            }

            var users = await userService.GetAllUsersAsync();
            var userProfiles = users.Select(u => new UserProfileResponse
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                Role = u.Role,
                AuthenticatedAt = u.AuthenticatedAt
            });

            return Results.Ok(userProfiles);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to get users",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    private static async Task<IResult> GetUserByIdAsync(
        string userId,
        IUserService userService,
        HttpContext context)
    {
        try
        {
            var currentUser = await userService.GetCurrentUserAsync(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }

            if (!userService.HasRole(currentUser, UserRole.Administrator))
            {
                return Results.Forbid();
            }

            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID {userId} was not found"
                });
            }

            var userProfile = new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                AuthenticatedAt = user.AuthenticatedAt
            };

            return Results.Ok(userProfile);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to get user",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Update user role
    /// </summary>
    private static async Task<IResult> UpdateUserRoleAsync(
        string userId,
        UpdateUserRoleRequest request,
        IUserService userService,
        HttpContext context)
    {
        try
        {
            var currentUser = await userService.GetCurrentUserAsync(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }

            if (!userService.HasRole(currentUser, UserRole.Administrator))
            {
                return Results.Forbid();
            }

            // Validate role
            if (!Enum.IsDefined(typeof(UserRole), request.Role))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid role",
                    Detail = "The specified role is not valid"
                });
            }

            // Prevent removing the last administrator
            if (request.Role != UserRole.Administrator && currentUser.Id == userId)
            {
                var adminCount = await userService.GetAdministratorCountAsync();
                if (adminCount <= 1)
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Cannot remove last administrator",
                        Detail = "Cannot remove administrator role from the last administrator in the system"
                    });
                }
            }

            var user = await userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID {userId} was not found"
                });
            }

            user.Role = request.Role;
            var updatedUser = await userService.CreateOrUpdateUserAsync(user);

            var userProfile = new UserProfileResponse
            {
                Id = updatedUser.Id,
                Email = updatedUser.Email,
                Name = updatedUser.Name,
                Role = updatedUser.Role,
                AuthenticatedAt = updatedUser.AuthenticatedAt
            };

            return Results.Ok(userProfile);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to update user role",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    private static async Task<IResult> DeleteUserAsync(
        string userId,
        IUserService userService,
        HttpContext context)
    {
        try
        {
            var currentUser = await userService.GetCurrentUserAsync(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }

            if (!userService.HasRole(currentUser, UserRole.Administrator))
            {
                return Results.Forbid();
            }

            // Prevent deleting self
            if (currentUser.Id == userId)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Cannot delete self",
                    Detail = "Administrators cannot delete their own account"
                });
            }

            // Prevent deleting the last administrator
            var user = await userService.GetUserByIdAsync(userId);
            if (user?.Role == UserRole.Administrator)
            {
                var adminCount = await userService.GetAdministratorCountAsync();
                if (adminCount <= 1)
                {
                    return Results.BadRequest(new ProblemDetails
                    {
                        Title = "Cannot delete last administrator",
                        Detail = "Cannot delete the last administrator in the system"
                    });
                }
            }

            var success = await userService.DeleteUserAsync(userId);
            if (!success)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "User not found",
                    Detail = $"User with ID {userId} was not found"
                });
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to delete user",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get all shared content (Administrator view)
    /// </summary>
    private static async Task<IResult> GetAllSharedContentAsync(
        IUserService userService,
        IFileShareService fileShareService,
        HttpContext context)
    {
        try
        {
            var currentUser = await userService.GetCurrentUserAsync(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }

            if (!userService.HasRole(currentUser, UserRole.Administrator))
            {
                return Results.Forbid();
            }

            var allShares = await fileShareService.GetAllSharesAsync();
            return Results.Ok(allShares);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to get shared content",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}