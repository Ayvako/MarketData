namespace MarketData.Application.Models;

using System;
using System.Collections.Generic;

public class AssetPriceResponse
{
    public Guid AssetId { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public decimal? LastPrice { get; set; }

    public DateTimeOffset? LastUpdated { get; set; }

    public List<Fintacharts.BarDto> History { get; set; } = [];
}