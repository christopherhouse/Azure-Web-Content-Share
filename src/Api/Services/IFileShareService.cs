using AzureWebContentShare.Api.Models;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Service for managing file shares
/// </summary>
public interface IFileShareService
{
    /// <summary>
    /// Upload a file and create a share
    /// </summary>
    /// <param name="file">File to upload</param>
    /// <param name="request">Share request details</param>
    /// <param name="userId">ID of the user creating the share</param>
    /// <returns>Share response with share code</returns>
    Task<ShareFileResponse> ShareFileAsync(IFormFile file, ShareFileRequest request, string userId);
    
    /// <summary>
    /// Get file metadata by share code
    /// </summary>
    /// <param name="shareCode">Share code</param>
    /// <returns>File metadata or null if not found/expired</returns>
    Task<FileShareMetadata?> GetFileByShareCodeAsync(string shareCode);
    
    /// <summary>
    /// Download file content by share code
    /// </summary>
    /// <param name="shareCode">Share code</param>
    /// <returns>File stream or null if not found/expired</returns>
    Task<(Stream? stream, string? fileName, string? contentType)> DownloadFileAsync(string shareCode);
    
    /// <summary>
    /// Get all shares created by a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of file share metadata</returns>
    Task<IEnumerable<FileShareMetadata>> GetUserSharesAsync(string userId);
    
    /// <summary>
    /// Delete a share (mark as deleted)
    /// </summary>
    /// <param name="shareId">Share ID</param>
    /// <param name="userId">User ID (for authorization)</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteShareAsync(string shareId, string userId);
    
    /// <summary>
    /// Clean up expired shares (background job)
    /// </summary>
    /// <returns>Number of shares cleaned up</returns>
    Task<int> CleanupExpiredSharesAsync();
}