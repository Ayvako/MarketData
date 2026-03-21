namespace MarketData.Application.Models.Fintacharts;

public class WsSubscriptionMessage
{
    public string Type { get; set; } = "l1-subscription";

    public string Id { get; set; } = "1";

    public string InstrumentId { get; set; } = string.Empty;

    public string Provider { get; set; } = "oanda";

    public bool Subscribe { get; set; } = true;
}