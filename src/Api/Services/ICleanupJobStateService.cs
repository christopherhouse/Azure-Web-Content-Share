using AzureWebContentShare.Api.Models;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Service for managing cleanup job state and high water mark
/// </summary>
public interface ICleanupJobStateService
{
    /// <summary>
    /// Gets the current cleanup job state
    /// </summary>
    /// <returns>The current state or a new initial state if none exists</returns>
    Task<CleanupJobState> GetStateAsync();
    
    /// <summary>
    /// Updates the cleanup job state with new high water mark
    /// </summary>
    /// <param name="state">The updated state to save</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task UpdateStateAsync(CleanupJobState state);
}