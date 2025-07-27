namespace AzureWebContentShare.Api.Configuration;

/// <summary>
/// Configuration options for Azure services
/// </summary>
public class AzureOptions
{
    public const string SectionName = "Azure";
    
    /// <summary>
    /// Storage account configuration
    /// </summary>
    public StorageOptions Storage { get; set; } = new();
    
    /// <summary>
    /// Cosmos DB configuration
    /// </summary>
    public CosmosDbOptions CosmosDb { get; set; } = new();
    
    /// <summary>
    /// Key Vault configuration
    /// </summary>
    public KeyVaultOptions KeyVault { get; set; } = new();
}

/// <summary>
/// Storage account configuration
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// Blob service endpoint
    /// </summary>
    public string BlobEndpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Container name for storing shared files
    /// </summary>
    public string ContainerName { get; set; } = "shared-files";
}

/// <summary>
/// Cosmos DB configuration
/// </summary>
public class CosmosDbOptions
{
    /// <summary>
    /// Cosmos DB endpoint
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;
    
    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = "ContentShare";
    
    /// <summary>
    /// Container name for file metadata
    /// </summary>
    public string ContainerName { get; set; } = "FileMetadata";
}

/// <summary>
/// Key Vault configuration
/// </summary>
public class KeyVaultOptions
{
    /// <summary>
    /// Key Vault URI
    /// </summary>
    public string Uri { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the secret containing the encryption key for share codes
    /// </summary>
    public string EncryptionKeySecretName { get; set; } = "ShareCodeEncryptionKey";
}