namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Service for encrypting and decrypting share codes
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypt a share code
    /// </summary>
    /// <param name="plainText">Plain text to encrypt</param>
    /// <returns>Encrypted text</returns>
    Task<string> EncryptAsync(string plainText);
    
    /// <summary>
    /// Decrypt a share code
    /// </summary>
    /// <param name="encryptedText">Encrypted text to decrypt</param>
    /// <returns>Plain text</returns>
    Task<string> DecryptAsync(string encryptedText);
    
    /// <summary>
    /// Generate a random share code
    /// </summary>
    /// <returns>Random share code</returns>
    string GenerateShareCode();
}