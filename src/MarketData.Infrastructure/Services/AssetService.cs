namespace MarketData.Infrastructure.Services;

using MarketData.Application.Interfaces;
using MarketData.Application.Models;
using MarketData.Domain.Entities;
using MarketData.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class AssetService(ApplicationDbContext context, IFintachartsRestClient fintaClient) : IAssetService
{
    private readonly ApplicationDbContext context = context;

    private readonly IFintachartsRestClient fintaClient = fintaClient;

    public async Task<IEnumerable<Asset>> GetAllAssetsAsync(CancellationToken ct = default)
    {
        return await this.context.Assets.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<AssetPriceResponse?> GetAssetPriceInfoAsync(Guid assetId, CancellationToken ct = default)
    {
        var asset = await this.context.Assets
                .FirstOrDefaultAsync(a => a.Id == assetId || a.ExternalId == assetId.ToString(), ct)
                .ConfigureAwait(false);
        if (asset == null)
        {
            return null;
        }

        var history = await this.fintaClient.GetHistoricalBarsAsync(
            instrumentId: asset.ExternalId,
            provider: "oanda",
            ct: ct).ConfigureAwait(false);

        var response = new AssetPriceResponse
        {
            AssetId = asset.Id,
            Symbol = asset.Symbol,
            LastPrice = asset.LastPrice,
            LastUpdated = asset.LastUpdated,
            History = history,
        };
        if (response.LastPrice == null && response.History != null && response.History.Count == 0)
        {
            var latestCandle = response.History[0];

            response.LastPrice = latestCandle.Close;
            response.LastUpdated = latestCandle.Timestamp;
        }

        return response;
    }

    public async Task SyncAssetsAsync(CancellationToken ct = default)
    {
        var remoteInstruments = await this.fintaClient.GetInstrumentsAsync(cancellationToken: ct).ConfigureAwait(false);

        foreach (var instrument in remoteInstruments)
        {
            var asset = await this.context.Assets
                .FirstOrDefaultAsync(a => a.ExternalId == instrument.Id, ct)
                .ConfigureAwait(false);

            bool isNew = false;
            if (asset == null)
            {
                isNew = true;
                asset = new Asset
                {
                    Id = Guid.NewGuid(),
                    ExternalId = instrument.Id,
                    Symbol = instrument.Symbol,
                    Name = instrument.Description,
                    AssetKind = instrument.Kind,
                    Exchange = instrument.Exchange,
                };
            }

            var bars = await this.fintaClient.GetHistoricalBarsAsync(
                instrument.Id,
                "oanda",
                1,
                "minute",
                1,
                ct).ConfigureAwait(false);

            var lastBar = bars?.FirstOrDefault();
            if (lastBar != null)
            {
                asset.LastPrice = lastBar.Close;
                asset.LastUpdated = lastBar.Timestamp;
            }

            if (isNew)
            {
                this.context.Assets.Add(asset);
            }
        }

        await this.context.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}