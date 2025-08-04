using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Services;
using AzureWebContentShare.Api.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;
using AutoFixture;

namespace AzureWebContentShare.Api.Tests.Services;

/// <summary>
/// Unit tests for SiteService
/// </summary>
public class SiteServiceTests
{
    private readonly Mock<ILogger<SiteService>> _mockLogger;
    private readonly Mock<CosmosClient> _mockCosmosClient;
    private readonly Mock<Container> _mockContainer;
    private readonly Mock<IUserService> _mockUserService;
    private readonly IOptions<AzureOptions> _azureOptions;
    private readonly Fixture _fixture;
    private readonly SiteService _siteService;

    public SiteServiceTests()
    {
        _mockLogger = new Mock<ILogger<SiteService>>();
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockContainer = new Mock<Container>();
        _mockUserService = new Mock<IUserService>();
        _fixture = new Fixture();
        
        _azureOptions = Options.Create(new AzureOptions
        {
            CosmosDb = new CosmosDbOptions { DatabaseName = "test-db" }
        });

        // Setup mock cosmos database and container
        var mockDatabase = new Mock<Database>();
        mockDatabase.Setup(x => x.GetContainer("site")).Returns(_mockContainer.Object);
        _mockCosmosClient.Setup(x => x.GetDatabase("test-db")).Returns(mockDatabase.Object);

        _siteService = new SiteService(_mockLogger.Object, _mockCosmosClient.Object, 
            _azureOptions, _mockUserService.Object);
    }

    [Fact]
    public async Task GetSiteMetadataAsync_WhenMetadataExists_ShouldReturnMetadata()
    {
        // Arrange
        var expectedMetadata = _fixture.Create<SiteMetadata>();
        var mockResponse = new Mock<ItemResponse<SiteMetadata>>();
        mockResponse.Setup(x => x.Resource).Returns(expectedMetadata);
        
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _siteService.GetSiteMetadataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedMetadata);
    }

    [Fact]
    public async Task GetSiteMetadataAsync_WhenMetadataNotFound_ShouldReturnNull()
    {
        // Arrange
        var cosmosException = new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ThrowsAsync(cosmosException);

        // Act
        var result = await _siteService.GetSiteMetadataAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsSiteClaimedAsync_WhenMetadataNotExists_ShouldReturnFalse()
    {
        // Arrange
        var cosmosException = new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ThrowsAsync(cosmosException);

        // Act
        var result = await _siteService.IsSiteClaimedAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSiteClaimedAsync_WhenMetadataExistsAndClaimed_ShouldReturnTrue()
    {
        // Arrange
        var metadata = _fixture.Build<SiteMetadata>()
                              .With(x => x.IsClaimed, true)
                              .Create();
        var mockResponse = new Mock<ItemResponse<SiteMetadata>>();
        mockResponse.Setup(x => x.Resource).Returns(metadata);
        
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _siteService.IsSiteClaimedAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSiteClaimedAsync_WhenMetadataExistsAndNotClaimed_ShouldReturnFalse()
    {
        // Arrange
        var metadata = _fixture.Build<SiteMetadata>()
                              .With(x => x.IsClaimed, false)
                              .Create();
        var mockResponse = new Mock<ItemResponse<SiteMetadata>>();
        mockResponse.Setup(x => x.Resource).Returns(metadata);
        
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(mockResponse.Object);

        // Act
        var result = await _siteService.IsSiteClaimedAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ClaimSiteAsync_WhenSiteNotClaimed_ShouldClaimSuccessfully()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var user = _fixture.Build<CurrentUser>()
                          .With(x => x.Id, userId)
                          .With(x => x.Role, UserRole.ContentOwner)
                          .Create();
        var metadata = _fixture.Build<SiteMetadata>()
                              .With(x => x.IsClaimed, false)
                              .With(x => x.ClaimedByUserId, (string?)null)
                              .With(x => x.ClaimedAt, (DateTime?)null)
                              .Create();

        // Setup mocks
        var readResponse = new Mock<ItemResponse<SiteMetadata>>();
        readResponse.Setup(x => x.Resource).Returns(metadata);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(readResponse.Object);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                       .ReturnsAsync(user);

        var updatedUser = _fixture.Build<CurrentUser>()
                                 .With(x => x.Id, userId)
                                 .With(x => x.Role, UserRole.Administrator)
                                 .Create();
        _mockUserService.Setup(x => x.CreateOrUpdateUserAsync(It.IsAny<CurrentUser>()))
                       .ReturnsAsync(updatedUser);

        var upsertResponse = new Mock<ItemResponse<SiteMetadata>>();
        var claimedMetadata = new SiteMetadata
        {
            Id = metadata.Id,
            IsClaimed = true,
            ClaimedByUserId = userId,
            ClaimedAt = DateTime.UtcNow,
            SiteName = metadata.SiteName,
            CreatedAt = metadata.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
        upsertResponse.Setup(x => x.Resource).Returns(claimedMetadata);
        _mockContainer.Setup(x => x.UpsertItemAsync(It.IsAny<SiteMetadata>(), new PartitionKey("site"), null, default))
                     .ReturnsAsync(upsertResponse.Object);

        // Act
        var result = await _siteService.ClaimSiteAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsClaimed.Should().BeTrue();
        result.ClaimedByUserId.Should().Be(userId);
        result.ClaimedAt.Should().NotBeNull();

        // Verify user was promoted to Administrator
        _mockUserService.Verify(x => x.CreateOrUpdateUserAsync(It.Is<CurrentUser>(u => 
            u.Id == userId && u.Role == UserRole.Administrator)), Times.Once);
    }

    [Fact]
    public async Task ClaimSiteAsync_WhenSiteAlreadyClaimed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var metadata = _fixture.Build<SiteMetadata>()
                              .With(x => x.IsClaimed, true)
                              .Create();

        var readResponse = new Mock<ItemResponse<SiteMetadata>>();
        readResponse.Setup(x => x.Resource).Returns(metadata);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(readResponse.Object);

        // Act & Assert
        await _siteService.Invoking(x => x.ClaimSiteAsync(userId))
                         .Should().ThrowAsync<InvalidOperationException>()
                         .WithMessage("Site has already been claimed");
    }

    [Fact]
    public async Task ClaimSiteAsync_WhenUserNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = _fixture.Create<string>();
        var metadata = _fixture.Build<SiteMetadata>()
                              .With(x => x.IsClaimed, false)
                              .Create();

        var readResponse = new Mock<ItemResponse<SiteMetadata>>();
        readResponse.Setup(x => x.Resource).Returns(metadata);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(readResponse.Object);

        _mockUserService.Setup(x => x.GetUserByIdAsync(userId))
                       .ReturnsAsync((CurrentUser?)null);

        // Act & Assert
        await _siteService.Invoking(x => x.ClaimSiteAsync(userId))
                         .Should().ThrowAsync<InvalidOperationException>()
                         .WithMessage("User not found");
    }

    [Fact]
    public async Task InitializeSiteMetadataAsync_WhenMetadataNotExists_ShouldCreateNewMetadata()
    {
        // Arrange
        var cosmosException = new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ThrowsAsync(cosmosException);

        var createdMetadata = new SiteMetadata
        {
            Id = "site",
            IsClaimed = false,
            SiteName = "Azure Web Content Share"
        };
        var createResponse = new Mock<ItemResponse<SiteMetadata>>();
        createResponse.Setup(x => x.Resource).Returns(createdMetadata);
        _mockContainer.Setup(x => x.CreateItemAsync(It.IsAny<SiteMetadata>(), new PartitionKey("site"), null, default))
                     .ReturnsAsync(createResponse.Object);

        // Act
        var result = await _siteService.InitializeSiteMetadataAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("site");
        result.IsClaimed.Should().BeFalse();
        result.SiteName.Should().Be("Azure Web Content Share");
    }

    [Fact]
    public async Task InitializeSiteMetadataAsync_WhenMetadataExists_ShouldReturnExisting()
    {
        // Arrange
        var existingMetadata = _fixture.Create<SiteMetadata>();
        var readResponse = new Mock<ItemResponse<SiteMetadata>>();
        readResponse.Setup(x => x.Resource).Returns(existingMetadata);
        _mockContainer.Setup(x => x.ReadItemAsync<SiteMetadata>("site", new PartitionKey("site"), null, default))
                     .ReturnsAsync(readResponse.Object);

        // Act
        var result = await _siteService.InitializeSiteMetadataAsync();

        // Assert
        result.Should().BeEquivalentTo(existingMetadata);
        _mockContainer.Verify(x => x.CreateItemAsync(It.IsAny<SiteMetadata>(), It.IsAny<PartitionKey>(), null, default), Times.Never);
    }
}