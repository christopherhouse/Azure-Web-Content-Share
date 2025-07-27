namespace AzureWebContentShare.Api.Models;

/// <summary>
/// Represents metadata about a shared file
/// </summary>
public class FileShareMetadata
{
    /// <summary>
    /// Unique identifier for the file share
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID of the person who shared the file (partition key)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Original filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Blob storage path/container where the file is stored
    /// </summary>
    public string BlobPath { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME type of the file
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Email address of the content recipient
    /// </summary>
    public string RecipientEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted share code for accessing the file
    /// </summary>
    public string EncryptedShareCode { get; set; } = string.Empty;
    
    /// <summary>
    /// When the share was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the share was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// When the share expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Whether the share has been soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Cosmos DB TTL (time to live) in seconds
    /// </summary>
    public int Ttl { get; set; } = -1; // -1 means no TTL by default
}