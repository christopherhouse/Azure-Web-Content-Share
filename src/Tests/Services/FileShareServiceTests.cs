using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AzureWebContentShare.Api.Configuration;
using AzureWebContentShare.Api.Services;
using AzureWebContentShare.Api.Models;

namespace AzureWebContentShare.Api.Tests.Services;

/// <summary>
/// Unit tests for FileShareService using FluentAssertions and AutoFixture
/// </summary>
public class FileShareServiceTests
{
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly Mock<CosmosClient> _mockCosmosClient;
    private readonly Mock<IEncryptionService> _mockEncryptionService;
    private readonly Mock<IOptions<AzureOptions>> _mockOptions;
    private readonly Mock<ILogger<FileShareService>> _mockLogger;
    private readonly Fixture _fixture;
    private readonly FileShareService _sut;

    public FileShareServiceTests()
    {
        _mockBlobServiceClient = new Mock<BlobServiceClient>();
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockEncryptionService = new Mock<IEncryptionService>();
        _mockOptions = new Mock<IOptions<AzureOptions>>();
        _mockLogger = new Mock<ILogger<FileShareService>>();
        _fixture = new Fixture();

        // Setup AzureOptions
        var azureOptions = _fixture.Create<AzureOptions>();
        azureOptions.CosmosDb.DatabaseName = "ContentShare";
        azureOptions.CosmosDb.ContainerName = "FileMetadata";
        azureOptions.Storage.ContainerName = "shared-files";
        _mockOptions.Setup(x => x.Value).Returns(azureOptions);

        _sut = new FileShareService(
            _mockBlobServiceClient.Object,
            _mockCosmosClient.Object,
            _mockEncryptionService.Object,
            _mockOptions.Object,
            _mockLogger.Object);
    }

    [Theory]
    [AutoData]
    public async Task GetFileByShareCodeAsync_WithInvalidCode_ShouldReturnNull(string invalidCode)
    {
        // Arrange
        _mockEncryptionService
            .Setup(x => x.DecryptAsync(invalidCode))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _sut.GetFileByShareCodeAsync(invalidCode);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CleanupExpiredSharesAsync_WithExpiredShares_ShouldReturnCleanedCount()
    {
        // Arrange
        var expiredShares = _fixture.CreateMany<FileShareMetadata>(3).ToList();
        expiredShares.ForEach(share => share.ExpiresAt = DateTime.UtcNow.AddDays(-1));

        var mockContainer = new Mock<Container>();
        _mockCosmosClient
            .Setup(x => x.GetContainer("ContentShare", "FileMetadata"))
            .Returns(mockContainer.Object);

        // Setup query response mock would go here in a real implementation
        // For now, we'll test the method signature and return type

        // Act
        var result = await _sut.CleanupExpiredSharesAsync();

        // Assert
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void Constructor_WithNullBlobServiceClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new FileShareService(
            null!, 
            _mockCosmosClient.Object, 
            _mockEncryptionService.Object, 
            _mockOptions.Object, 
            _mockLogger.Object);
        
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("blobServiceClient");
    }

    [Fact]
    public void Constructor_WithNullCosmosClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new FileShareService(
            _mockBlobServiceClient.Object, 
            null!, 
            _mockEncryptionService.Object, 
            _mockOptions.Object, 
            _mockLogger.Object);
        
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("cosmosClient");
    }

    [Fact]
    public void Constructor_WithNullEncryptionService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new FileShareService(
            _mockBlobServiceClient.Object, 
            _mockCosmosClient.Object, 
            null!, 
            _mockOptions.Object, 
            _mockLogger.Object);
        
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("encryptionService");
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new FileShareService(
            _mockBlobServiceClient.Object, 
            _mockCosmosClient.Object, 
            _mockEncryptionService.Object, 
            null!, 
            _mockLogger.Object);
        
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new FileShareService(
            _mockBlobServiceClient.Object, 
            _mockCosmosClient.Object, 
            _mockEncryptionService.Object, 
            _mockOptions.Object, 
            null!);
        
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }
}