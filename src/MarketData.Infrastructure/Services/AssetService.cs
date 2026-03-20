namespace MarketData.Infrastructure.Services;

using MarketData.Application.Interfaces;
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

    public async Task SyncAssetsAsync(CancellationToken ct = default)
    {
        var remoteInstruments = await this.fintaClient.GetInstrumentsAsync(cancellationToken: ct).ConfigureAwait(false);

        foreach (var instrument in remoteInstruments)
        {
            var exists = await this.context.Assets.AnyAsync(a => a.ExternalId == instrument.Id, ct).ConfigureAwait(false);

            if (!exists)
            {
                this.context.Assets.Add(new Asset
                {
                    Id = Guid.NewGuid(),
                    ExternalId = instrument.Id,
                    Symbol = instrument.Symbol,
                    Name = instrument.Description,
                    AssetKind = instrument.Kind,
                    Exchange = instrument.Exchange,
                });
            }
        }

        await this.context.SaveChangesAsync(ct).ConfigureAwait(false);
    }
}