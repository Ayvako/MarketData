namespace MarketData.Api.Workers;

using MarketData.Application.Interfaces;

public partial class AssetSyncWorker(IServiceProvider serviceProvider, ILogger<AssetSyncWorker> logger) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

    private readonly ILogger<AssetSyncWorker> logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogSyncStarted(this.logger);

        using var scope = this.serviceProvider.CreateScope();
        var assetService = scope.ServiceProvider.GetRequiredService<IAssetService>();

        try
        {
            await assetService.SyncAssetsAsync(stoppingToken).ConfigureAwait(false);
            LogSyncCompleted(this.logger);
        }
        catch (OperationCanceledException)
        {
            LogSyncCanceled(this.logger);
        }
        catch (HttpRequestException ex)
        {
            LogNetworkError(this.logger, ex);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Asset synchronization started...")]
    static partial void LogSyncStarted(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Asset synchronization completed successfully.")]
    static partial void LogSyncCompleted(ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Asset synchronization was canceled.")]
    static partial void LogSyncCanceled(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Network error during asset synchronization.")]
    static partial void LogNetworkError(ILogger logger, Exception ex);

    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "Fatal error occurred while syncing assets during background execution.")]
    static partial void LogFatalError(ILogger logger, Exception ex);
}