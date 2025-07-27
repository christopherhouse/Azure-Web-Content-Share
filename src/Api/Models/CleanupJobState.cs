using System.Text.Json.Serialization;

namespace AzureWebContentShare.Api.Models;

/// <summary>
/// Represents the state of the cleanup job, including the high water mark
/// </summary>
public class CleanupJobState
{
    /// <summary>
    /// Well-known ID for the cleanup job state document
    /// </summary>
    public const string DocumentId = "cleanup-job-state";
    
    /// <summary>
    /// The unique identifier for this document (always "cleanup-job-state")
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = DocumentId;
    
    /// <summary>
    /// The partition key - using a fixed value for efficient reads
    /// </summary>
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "system";
    
    /// <summary>
    /// The timestamp of the last processed share (high water mark)
    /// </summary>
    [JsonPropertyName("lastProcessedTimestamp")]
    public DateTimeOffset LastProcessedTimestamp { get; set; }
    
    /// <summary>
    /// The timestamp when this state was last updated
    /// </summary>
    [JsonPropertyName("lastUpdated")]
    public DateTimeOffset LastUpdated { get; set; }
    
    /// <summary>
    /// The number of shares processed in the last run
    /// </summary>
    [JsonPropertyName("lastRunProcessedCount")]
    public int LastRunProcessedCount { get; set; }
    
    /// <summary>
    /// The timestamp when the job last ran
    /// </summary>
    [JsonPropertyName("lastRunTimestamp")]
    public DateTimeOffset LastRunTimestamp { get; set; }
}