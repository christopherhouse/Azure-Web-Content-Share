using System.Security.Cryptography;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using AzureWebContentShare.Api.Configuration;
using Microsoft.Extensions.Options;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Implementation of encryption service using AES encryption with keys from Key Vault
/// </summary>
public class EncryptionService : IEncryptionService
{
    private readonly SecretClient _secretClient;
    private readonly AzureOptions _azureOptions;
    private readonly ILogger<EncryptionService> _logger;
    private string? _cachedKey;

    public EncryptionService(
        SecretClient secretClient, 
        IOptions<AzureOptions> azureOptions,
        ILogger<EncryptionService> logger)
    {
        _secretClient = secretClient;
        _azureOptions = azureOptions.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> EncryptAsync(string plainText)
    {
        try
        {
            var key = await GetEncryptionKeyAsync();
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Combine IV and encrypted data
            var result = new byte[aes.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt data");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<string> DecryptAsync(string encryptedText)
    {
        try
        {
            var key = await GetEncryptionKeyAsync();
            var encryptedData = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(key);

            // Extract IV and encrypted data
            var iv = new byte[16]; // AES block size
            var encrypted = new byte[encryptedData.Length - 16];
            Buffer.BlockCopy(encryptedData, 0, iv, 0, 16);
            Buffer.BlockCopy(encryptedData, 16, encrypted, 0, encrypted.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt data");
            throw;
        }
    }

    /// <inheritdoc/>
    public string GenerateShareCode()
    {
        // Generate a 12-character alphanumeric code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new StringBuilder(12);
        
        for (int i = 0; i < 12; i++)
        {
            code.Append(chars[random.Next(chars.Length)]);
        }
        
        return code.ToString();
    }

    private async Task<string> GetEncryptionKeyAsync()
    {
        if (_cachedKey != null)
            return _cachedKey;

        try
        {
            var response = await _secretClient.GetSecretAsync(_azureOptions.KeyVault.EncryptionKeySecretName);
            _cachedKey = response.Value.Value;
            return _cachedKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve encryption key from Key Vault");
            throw;
        }
    }
}