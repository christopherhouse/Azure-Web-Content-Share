using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AzureWebContentShare.Api.Tests;

/// <summary>
/// Integration tests for health endpoints
/// </summary>
public class HealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task GetReadiness_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/health/ready");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Ready", content);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}