using AzureWebContentShare.Api.Models;
using AzureWebContentShare.Api.Services;
using AzureWebContentShare.Api.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FluentAssertions;

namespace AzureWebContentShare.Api.Tests.Services;

/// <summary>
/// Unit tests for UserService
/// </summary>
public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly Mock<IFileShareService> _mockFileShareService;
    private readonly Mock<CosmosClient> _mockCosmosClient;
    private readonly Mock<Container> _mockContainer;
    private readonly IOptions<AzureOptions> _azureOptions;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockLogger = new Mock<ILogger<UserService>>();
        _mockFileShareService = new Mock<IFileShareService>();
        _mockCosmosClient = new Mock<CosmosClient>();
        _mockContainer = new Mock<Container>();
        
        _azureOptions = Options.Create(new AzureOptions
        {
            CosmosDb = new CosmosDbOptions { DatabaseName = "test-db" }
        });

        // Setup mock cosmos database and container
        var mockDatabase = new Mock<Database>();
        mockDatabase.Setup(x => x.GetContainer("users")).Returns(_mockContainer.Object);
        _mockCosmosClient.Setup(x => x.GetDatabase("test-db")).Returns(mockDatabase.Object);

        _userService = new UserService(_mockLogger.Object, _mockFileShareService.Object, 
            _mockCosmosClient.Object, _azureOptions);
    }

    [Fact]
    public void HasRole_Administrator_ShouldHaveAllRoles()
    {
        // Arrange
        var adminUser = new CurrentUser { Role = UserRole.Administrator };

        // Act & Assert
        _userService.HasRole(adminUser, UserRole.Administrator).Should().BeTrue();
        _userService.HasRole(adminUser, UserRole.ContentOwner).Should().BeTrue();
        _userService.HasRole(adminUser, UserRole.ContentRecipient).Should().BeTrue();
    }

    [Fact]
    public void HasRole_ContentOwner_ShouldOnlyHaveOwnRole()
    {
        // Arrange
        var contentOwner = new CurrentUser { Role = UserRole.ContentOwner };

        // Act & Assert
        _userService.HasRole(contentOwner, UserRole.ContentOwner).Should().BeTrue();
        _userService.HasRole(contentOwner, UserRole.Administrator).Should().BeFalse();
        _userService.HasRole(contentOwner, UserRole.ContentRecipient).Should().BeFalse();
    }

    [Fact]
    public void HasRole_ContentRecipient_ShouldOnlyHaveOwnRole()
    {
        // Arrange
        var recipient = new CurrentUser { Role = UserRole.ContentRecipient };

        // Act & Assert
        _userService.HasRole(recipient, UserRole.ContentRecipient).Should().BeTrue();
        _userService.HasRole(recipient, UserRole.Administrator).Should().BeFalse();
        _userService.HasRole(recipient, UserRole.ContentOwner).Should().BeFalse();
    }
}