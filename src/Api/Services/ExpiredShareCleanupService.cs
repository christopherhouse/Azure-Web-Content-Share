using AzureWebContentShare.Api.Services;

namespace AzureWebContentShare.Api.Services;

/// <summary>
/// Background service that cleans up expired file shares
/// </summary>
public class ExpiredShareCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredShareCleanupService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Run every 5 minutes

    public ExpiredShareCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredShareCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired share cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredSharesAsync();
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up expired shares");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Expired share cleanup service stopped");
    }

    private async Task CleanupExpiredSharesAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var fileShareService = scope.ServiceProvider.GetRequiredService<IFileShareService>();

        try
        {
            var cleanedCount = await fileShareService.CleanupExpiredSharesAsync();
            if (cleanedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} expired shares", cleanedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired shares");
        }
    }
}