namespace AzureWebContentShare.Api.Configuration;

/// <summary>
/// Configuration options for Entra ID authentication
/// </summary>
public class EntraIdOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "EntraId";
    
    /// <summary>
    /// Azure AD tenant ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// API client ID (application ID)
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// API client secret (for confidential client flows)
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Authority URL for token validation
    /// </summary>
    public string Authority => $"https://login.microsoftonline.com/{TenantId}";
    
    /// <summary>
    /// Valid audience for JWT tokens
    /// </summary>
    public string Audience => $"api://{ClientId}";
    
    /// <summary>
    /// Valid issuer for JWT tokens
    /// </summary>
    public string Issuer => $"https://sts.windows.net/{TenantId}/";
    
    /// <summary>
    /// Frontend client ID (for CORS and token validation)
    /// </summary>
    public string FrontendClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Frontend redirect URI
    /// </summary>
    public string FrontendRedirectUri { get; set; } = string.Empty;
}