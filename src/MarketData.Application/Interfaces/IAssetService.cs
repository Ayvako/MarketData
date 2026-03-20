namespace MarketData.Application.Interfaces;

using MarketData.Domain.Entities;

public interface IAssetService
{
    Task<IEnumerable<Asset>> GetAllAssetsAsync(CancellationToken ct = default);

    Task SyncAssetsAsync(CancellationToken ct = default);
}