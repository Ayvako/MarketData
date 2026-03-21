namespace MarketData.Api.Workers;

using MarketData.Infrastructure.Persistence;
using MarketData.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public class PriceUpdateWorker(IServiceProvider serviceProvider, FintachartsWebSocketClient wsClient) : BackgroundService
{
    private readonly IServiceProvider serviceProvider = serviceProvider;

#pragma warning disable CA2213

    private readonly FintachartsWebSocketClient wsClient = wsClient;

#pragma warning restore CA2213

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = this.wsClient.ConnectAndListen(
            async update =>
        {
            using var scope = this.serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var asset = await db.Assets.FirstOrDefaultAsync(a => a.ExternalId == update.InstrumentId).ConfigureAwait(false);
            if (asset != null)
            {
                ArgumentNullException.ThrowIfNull(update.Last);
                asset.LastPrice = update.Last.Price;
                asset.LastUpdated = update.Last.Timestamp;
                await db.SaveChangesAsync().ConfigureAwait(false);
            }
        },
            stoppingToken);

        await Task.Delay(5000, stoppingToken).ConfigureAwait(false);
        using var scope = this.serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var ids = await db.Assets.Select(a => a.ExternalId).ToListAsync(cancellationToken: stoppingToken).ConfigureAwait(false);

        foreach (var id in ids)
        {
            await this.wsClient.Subscribe(id).ConfigureAwait(false);
        }
    }
}