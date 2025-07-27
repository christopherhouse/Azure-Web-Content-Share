using AzureWebContentShare.Api.Models;

namespace AzureWebContentShare.Api.Tests.Models;

/// <summary>
/// Unit tests for ShareModels demonstrating FluentAssertions and AutoFixture usage
/// </summary>
public class ShareModelsTests
{
    private readonly Fixture _fixture;

    public ShareModelsTests()
    {
        _fixture = new Fixture();
    }

    [Theory]
    [AutoData]
    public void ShareFileRequest_WithAutoData_ShouldHaveValidProperties(
        ShareFileRequest request)
    {
        // Assert
        request.Should().NotBeNull();
        request.RecipientEmail.Should().NotBeNullOrEmpty();
        request.ExpirationHours.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShareFileRequest_WithFixtureData_ShouldAllowCustomization()
    {
        // Arrange
        var request = _fixture.Build<ShareFileRequest>()
            .With(x => x.RecipientEmail, "test@example.com")
            .With(x => x.ExpirationHours, 24)
            .Create();

        // Assert
        request.RecipientEmail.Should().Be("test@example.com");
        request.ExpirationHours.Should().Be(24);
    }

    [Theory]
    [AutoData]
    public void ShareFileResponse_WithAutoData_ShouldHaveValidStructure(
        ShareFileResponse response)
    {
        // Assert
        response.Should().NotBeNull();
        response.ShareId.Should().NotBeNullOrEmpty();
        response.ShareCode.Should().NotBeNullOrEmpty();
        response.FileName.Should().NotBeNullOrEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.MinValue);
    }

    [Fact]
    public void ShareFileResponse_WithExpiredDate_ShouldBeInPast()
    {
        // Arrange
        var response = _fixture.Build<ShareFileResponse>()
            .With(x => x.ExpiresAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Assert
        response.ExpiresAt.Should().BeBefore(DateTime.UtcNow);
    }

    [Theory]
    [AutoData]
    public void AccessFileRequest_WithValidShareCode_ShouldNotBeEmpty(
        AccessFileRequest request)
    {
        // Assert
        request.Should().NotBeNull();
        request.ShareCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FileShareMetadata_CollectionOperations_ShouldWorkWithFluentAssertions()
    {
        // Arrange
        var fileShares = _fixture.CreateMany<FileShareMetadata>(5).ToList();
        
        // Act
        var activeShares = fileShares.Where(x => !x.IsDeleted).ToList();
        var expiredShares = fileShares.Where(x => x.ExpiresAt < DateTime.UtcNow).ToList();

        // Assert
        fileShares.Should().HaveCount(5);
        fileShares.Should().AllSatisfy(share => 
        {
            share.Id.Should().NotBeNullOrEmpty();
            share.FileName.Should().NotBeNullOrEmpty();
            share.BlobPath.Should().NotBeNullOrEmpty();
        });
        
        activeShares.Should().AllSatisfy(share => share.IsDeleted.Should().BeFalse());
    }

    [Fact]
    public void FileShareMetadata_TimeComparisons_ShouldUseFluentAssertions()
    {
        // Arrange
        var metadata = _fixture.Build<FileShareMetadata>()
            .With(x => x.CreatedAt, DateTime.UtcNow.AddHours(-2))
            .With(x => x.UpdatedAt, DateTime.UtcNow.AddHours(-1))
            .With(x => x.ExpiresAt, DateTime.UtcNow.AddHours(22))
            .Create();

        // Assert
        metadata.CreatedAt.Should().BeBefore(metadata.UpdatedAt);
        metadata.UpdatedAt.Should().BeBefore(metadata.ExpiresAt);
        metadata.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        
        // Time span assertions
        var timeUntilExpiration = metadata.ExpiresAt - DateTime.UtcNow;
        timeUntilExpiration.Should().BeGreaterThan(TimeSpan.FromHours(20));
    }
}