namespace MarketData.Application.Models.Fintacharts;

public class WsPriceUpdateMessage
{
    public string Type { get; set; } = string.Empty;

    public string InstrumentId { get; set; } = string.Empty;

    public TickData? Last { get; set; }
}