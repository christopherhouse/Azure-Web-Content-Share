using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureWebContentShare.Api.Configuration;
using AzureWebContentShare.Api.Services;

namespace AzureWebContentShare.Api.Tests.Services;

/// <summary>
/// Unit tests for EncryptionService using FluentAssertions and AutoFixture
/// </summary>
public class EncryptionServiceTests
{
    private readonly Mock<SecretClient> _mockSecretClient;
    private readonly Mock<IOptions<AzureOptions>> _mockOptions;
    private readonly Mock<ILogger<EncryptionService>> _mockLogger;
    private readonly Fixture _fixture;
    private readonly EncryptionService _sut;

    public EncryptionServiceTests()
    {
        _mockSecretClient = new Mock<SecretClient>();
        _mockOptions = new Mock<IOptions<AzureOptions>>();
        _mockLogger = new Mock<ILogger<EncryptionService>>();
        _fixture = new Fixture();

        // Setup AzureOptions
        var azureOptions = _fixture.Create<AzureOptions>();
        azureOptions.KeyVault.EncryptionKeySecretName = "file-share-encryption-key";
        _mockOptions.Setup(x => x.Value).Returns(azureOptions);

        _sut = new EncryptionService(
            _mockSecretClient.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EncryptAsync_WithEmptyOrWhitespacePlaintext_ShouldThrowArgumentException(string invalidPlaintext)
    {
        // Act & Assert
        var action = async () => await _sut.EncryptAsync(invalidPlaintext);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EncryptAsync_WithNullPlaintext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = async () => await _sut.EncryptAsync(null!);
        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("plainText");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DecryptAsync_WithEmptyOrWhitespaceCiphertext_ShouldThrowArgumentException(string invalidCiphertext)
    {
        // Act & Assert
        var action = async () => await _sut.DecryptAsync(invalidCiphertext);
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DecryptAsync_WithNullCiphertext_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = async () => await _sut.DecryptAsync(null!);
        await action.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("cipherText");
    }

    [Fact]
    public void Constructor_WithNullParameters_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action1 = () => new EncryptionService(null!, _mockOptions.Object, _mockLogger.Object);
        action1.Should().Throw<ArgumentNullException>();

        var action2 = () => new EncryptionService(_mockSecretClient.Object, null!, _mockLogger.Object);
        action2.Should().Throw<ArgumentNullException>();

        var action3 = () => new EncryptionService(_mockSecretClient.Object, _mockOptions.Object, null!);
        action3.Should().Throw<ArgumentNullException>();
    }
}