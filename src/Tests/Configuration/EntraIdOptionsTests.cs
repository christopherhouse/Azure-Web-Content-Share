using AzureWebContentShare.Api.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace AzureWebContentShare.Api.Tests.Configuration;

/// <summary>
/// Tests for EntraId configuration from environment variables
/// </summary>
public class EntraIdOptionsTests
{
    [Fact]
    public void EntraIdOptions_WhenEnvironmentVariablesAreSet_ShouldBindCorrectly()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["EntraId:TenantId"] = "12345678-1234-1234-1234-123456789012",
                ["EntraId:ClientId"] = "87654321-4321-4321-4321-210987654321",
                ["EntraId:ClientSecret"] = "super-secret-client-secret-value",
                ["EntraId:FrontendClientId"] = "abcdef01-2345-6789-abcd-ef0123456789",
                ["EntraId:FrontendRedirectUri"] = "https://localhost:5173/auth/callback"
            })
            .Build();

        var services = new ServiceCollection();
        services.Configure<EntraIdOptions>(configuration.GetSection(EntraIdOptions.SectionName));
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<EntraIdOptions>>().Value;

        // Assert
        options.TenantId.Should().Be("12345678-1234-1234-1234-123456789012");
        options.ClientId.Should().Be("87654321-4321-4321-4321-210987654321");
        options.ClientSecret.Should().Be("super-secret-client-secret-value");
        options.FrontendClientId.Should().Be("abcdef01-2345-6789-abcd-ef0123456789");
        options.FrontendRedirectUri.Should().Be("https://localhost:5173/auth/callback");
    }

    [Fact]
    public void EntraIdOptions_Authority_ShouldBeGeneratedCorrectly()
    {
        // Arrange
        var tenantId = "12345678-1234-1234-1234-123456789012";
        var options = new EntraIdOptions
        {
            TenantId = tenantId
        };

        // Act & Assert
        options.Authority.Should().Be($"https://login.microsoftonline.com/{tenantId}");
    }

    [Fact]
    public void EntraIdOptions_Audience_ShouldBeGeneratedCorrectly()
    {
        // Arrange
        var clientId = "87654321-4321-4321-4321-210987654321";
        var options = new EntraIdOptions
        {
            ClientId = clientId
        };

        // Act & Assert
        options.Audience.Should().Be($"api://{clientId}");
    }

    [Fact]
    public void EntraIdOptions_Issuer_ShouldBeGeneratedCorrectly()
    {
        // Arrange
        var tenantId = "12345678-1234-1234-1234-123456789012";
        var options = new EntraIdOptions
        {
            TenantId = tenantId
        };

        // Act & Assert
        options.Issuer.Should().Be($"https://sts.windows.net/{tenantId}/");
    }

    [Fact]
    public void EntraIdOptions_FromEnvironmentVariables_ShouldBindWithDoubleUnderscoreConvention()
    {
        // Arrange - This simulates environment variables as they would appear in container
        Environment.SetEnvironmentVariable("EntraId__TenantId", "env-tenant-id-12345");
        Environment.SetEnvironmentVariable("EntraId__ClientId", "env-client-id-67890");
        Environment.SetEnvironmentVariable("EntraId__ClientSecret", "env-client-secret-abcdef");

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.Configure<EntraIdOptions>(configuration.GetSection(EntraIdOptions.SectionName));
            var serviceProvider = services.BuildServiceProvider();

            // Act
            var options = serviceProvider.GetRequiredService<IOptions<EntraIdOptions>>().Value;

            // Assert
            options.TenantId.Should().Be("env-tenant-id-12345");
            options.ClientId.Should().Be("env-client-id-67890");
            options.ClientSecret.Should().Be("env-client-secret-abcdef");
            
            // Verify computed properties work correctly
            options.Authority.Should().Be("https://login.microsoftonline.com/env-tenant-id-12345");
            options.Audience.Should().Be("api://env-client-id-67890");
            options.Issuer.Should().Be("https://sts.windows.net/env-tenant-id-12345/");
        }
        finally
        {
            // Clean up environment variables
            Environment.SetEnvironmentVariable("EntraId__TenantId", null);
            Environment.SetEnvironmentVariable("EntraId__ClientId", null);
            Environment.SetEnvironmentVariable("EntraId__ClientSecret", null);
        }
    }
}