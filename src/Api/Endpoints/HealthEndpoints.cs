namespace AzureWebContentShare.Api.Endpoints;

/// <summary>
/// Health check endpoints
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps health check endpoints
    /// </summary>
    /// <param name="routes">Endpoint route builder</param>
    public static void MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/health")
            .WithTags("Health")
            .WithOpenApi();

        group.MapGet("/", GetHealthAsync)
            .WithName("GetHealth")
            .WithSummary("Get application health status")
            .WithDescription("Returns the health status of the application and its dependencies")
            .Produces<object>(StatusCodes.Status200OK);

        group.MapGet("/ready", GetReadinessAsync)
            .WithName("GetReadiness")
            .WithSummary("Get application readiness status")
            .WithDescription("Returns whether the application is ready to serve requests")
            .Produces<object>(StatusCodes.Status200OK);
    }

    /// <summary>
    /// Get application health status
    /// </summary>
    private static async Task<IResult> GetHealthAsync()
    {
        await Task.CompletedTask; // Placeholder for future health checks

        return Results.Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "Unknown",
            Dependencies = new
            {
                BlobStorage = "Healthy", // TODO: Implement actual health checks
                CosmosDb = "Healthy",
                KeyVault = "Healthy"
            }
        });
    }

    /// <summary>
    /// Get application readiness status
    /// </summary>
    private static async Task<IResult> GetReadinessAsync()
    {
        await Task.CompletedTask; // Placeholder for future readiness checks

        return Results.Ok(new
        {
            Status = "Ready",
            Timestamp = DateTime.UtcNow
        });
    }
}