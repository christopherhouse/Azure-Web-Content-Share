using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AzureWebContentShare.Api.Endpoints;

/// <summary>
/// Endpoints for file sharing operations
/// </summary>
public static class FileShareEndpoints
{
    /// <summary>
    /// Maps file share endpoints
    /// </summary>
    /// <param name="routes">Endpoint route builder</param>
    public static void MapFileShareEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/files")
            .WithTags("File Sharing")
            .WithOpenApi();

        group.MapPost("/share", ShareFileAsync)
            .WithName("ShareFile")
            .WithSummary("Upload and share a file")
            .WithDescription("Upload a file and create a share code for the specified recipient")
            .Accepts<ShareFileRequest>("application/json")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ShareFileResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapGet("/download/{shareCode}", DownloadFileAsync)
            .WithName("DownloadFile")
            .WithSummary("Download a shared file")
            .WithDescription("Download a file using the share code")
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status410Gone);

        group.MapGet("/metadata/{shareCode}", GetFileMetadataAsync)
            .WithName("GetFileMetadata")
            .WithSummary("Get file metadata by share code")
            .WithDescription("Get information about a shared file without downloading it")
            .Produces<FileShareMetadata>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/my-shares", GetMySharesAsync)
            .WithName("GetMyShares")
            .WithSummary("Get user's file shares")
            .WithDescription("Get all file shares created by the authenticated user")
            .RequireAuthorization()
            .Produces<IEnumerable<FileShareMetadata>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{shareId}", DeleteShareAsync)
            .WithName("DeleteShare")
            .WithSummary("Delete a file share")
            .WithDescription("Delete a file share (soft delete)")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Upload and share a file
    /// </summary>
    private static async Task<IResult> ShareFileAsync(
        IFormFile file,
        [FromForm] ShareFileRequest request,
        IFileShareService fileShareService,
        IUserService userService,
        HttpContext context)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid file",
                    Detail = "No file was uploaded or file is empty"
                });
            }

            if (string.IsNullOrWhiteSpace(request.RecipientEmail))
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "Invalid recipient",
                    Detail = "Recipient email is required"
                });
            }

            var currentUser = await userService.GetCurrentUserAsync(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }

            if (!userService.HasRole(currentUser, UserRole.ContentPublisher))
            {
                return Results.Forbid();
            }

            var response = await fileShareService.ShareFileAsync(file, request, currentUser.Id);
            return Results.Created($"/api/files/metadata/{response.ShareCode}", response);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "File share failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Download a shared file
    /// </summary>
    private static async Task<IResult> DownloadFileAsync(
        string shareCode,
        IFileShareService fileShareService)
    {
        try
        {
            var (stream, fileName, contentType) = await fileShareService.DownloadFileAsync(shareCode);

            if (stream == null)
            {
                return Results.Problem(
                    title: "File not found",
                    detail: "The requested file was not found or has expired",
                    statusCode: StatusCodes.Status410Gone);
            }

            return Results.Stream(stream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Download failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get file metadata by share code
    /// </summary>
    private static async Task<IResult> GetFileMetadataAsync(
        string shareCode,
        IFileShareService fileShareService)
    {
        try
        {
            var metadata = await fileShareService.GetFileByShareCodeAsync(shareCode);

            if (metadata == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "File not found",
                    Detail = "The requested file was not found or has expired"
                });
            }

            // Don't return sensitive information
            return Results.Ok(new
            {
                metadata.Id,
                metadata.FileName,
                metadata.ContentType,
                metadata.FileSize,
                metadata.CreatedAt,
                metadata.ExpiresAt
            });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to get file metadata",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Get user's file shares
    /// </summary>
    private static async Task<IResult> GetMySharesAsync(
        IFileShareService fileShareService,
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

            if (!userService.HasRole(currentUser, UserRole.ContentPublisher))
            {
                return Results.Forbid();
            }

            var shares = await fileShareService.GetUserSharesAsync(currentUser.Id);
            return Results.Ok(shares);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to get user shares",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Delete a file share
    /// </summary>
    private static async Task<IResult> DeleteShareAsync(
        string shareId,
        IFileShareService fileShareService,
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

            if (!userService.HasRole(currentUser, UserRole.ContentPublisher))
            {
                return Results.Forbid();
            }

            var success = await fileShareService.DeleteShareAsync(shareId, currentUser.Id);

            if (!success)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Share not found",
                    Detail = "The requested share was not found"
                });
            }

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to delete share",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}