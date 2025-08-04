using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Services;
using AzureWebContentShare.Api.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureWebContentShare.Api.Endpoints;

/// <summary>
/// Endpoints for site management and claiming
/// </summary>
public static class SiteEndpoints
{
    /// <summary>
    /// Maps site management endpoints
    /// </summary>
    /// <param name="routes">Endpoint route builder</param>
    public static void MapSiteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/site")
            .WithTags("Site Management")
            .WithOpenApi();

        // Get site status (anonymous access allowed)
        group.MapGet("/status", GetSiteStatusAsync)
            .WithName("GetSiteStatus")
            .WithSummary("Get site status")
            .WithDescription("Get site claiming status and basic information (anonymous access allowed)")
            .Produces<SiteStatusResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .AllowAnonymous();

        // Claim site (authenticated users only)
        group.MapPost("/claim", ClaimSiteAsync)
            .WithName("ClaimSite")
            .WithSummary("Claim site")
            .WithDescription("Claim the site as the first administrator (authenticated users only)")
            .Produces<ClaimSiteResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser);
    }

    /// <summary>
    /// Get site status
    /// </summary>
    private static async Task<IResult> GetSiteStatusAsync(ISiteService siteService)
    {
        try
        {
            var metadata = await siteService.GetSiteMetadataAsync();
            if (metadata == null)
            {
                // Initialize metadata if it doesn't exist
                metadata = await siteService.InitializeSiteMetadataAsync();
            }

            var response = new SiteStatusResponse
            {
                IsClaimed = metadata.IsClaimed,
                SiteName = metadata.SiteName
            };

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to get site status",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Claim site
    /// </summary>
    private static async Task<IResult> ClaimSiteAsync(
        ISiteService siteService,
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

            // Check if site is already claimed
            var isClaimed = await siteService.IsSiteClaimedAsync();
            if (isClaimed)
            {
                return Results.Conflict(new ProblemDetails
                {
                    Title = "Site already claimed",
                    Detail = "The site has already been claimed by an administrator",
                    Status = StatusCodes.Status409Conflict
                });
            }

            // Claim the site
            var metadata = await siteService.ClaimSiteAsync(currentUser.Id);

            var response = new ClaimSiteResponse
            {
                Success = true,
                Message = "Site successfully claimed. You are now the administrator.",
                ClaimedAt = metadata.ClaimedAt ?? DateTime.UtcNow
            };

            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("already been claimed"))
            {
                return Results.Conflict(new ProblemDetails
                {
                    Title = "Site already claimed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict
                });
            }

            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to claim site",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}