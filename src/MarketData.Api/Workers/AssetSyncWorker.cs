namespace MarketData.Api.Workers;

using MarketData.Application.Interfaces;

public class AssetSyncWorker(IServiceProvider serviceProvider, ILogger<AssetSyncWorker> logger) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    private readonly ILogger<AssetSyncWorker> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Asset synchronization started...");

        using var scope = this.serviceProvider.CreateScope();
        var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();

        try
        {
            await assetService.SyncAssetsAsync(stoppingToken).ConfigureAwait(false);
            this.logger.LogInformation("Asset synchronization completed successfully.");
        }
        catch (OperationCanceledException)
        {
            this.logger.LogWarning("Asset synchronization was canceled.");
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, "Network error during asset synchronization.");
        }
        catch (Exception ex)
        {
            var message = "Fatal error occurred while syncing assets during background execution.";
            this.logger.LogCritical(ex, message);
            throw new InvalidOperationException(message, ex);
        }
    }
}