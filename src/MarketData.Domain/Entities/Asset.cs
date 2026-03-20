namespace MarketData.Domain.Entities;

public class Asset
{
    public Guid Id { get; set; }

    public string ExternalId { get; set; } = string.Empty;

    public string Symbol { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string AssetKind { get; set; } = string.Empty;

    public string Exchange { get; set; } = string.Empty;

    public decimal? LastPrice { get; set; }

    public DateTimeOffset? LastUpdated { get; set; }
}