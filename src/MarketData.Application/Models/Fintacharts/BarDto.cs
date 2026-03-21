namespace MarketData.Application.Models.Fintacharts;

using System.Text.Json.Serialization;

public class BarDto
{
    [JsonPropertyName("t")] // Timestamp
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("o")] // Open
    public decimal Open { get; set; }

    [JsonPropertyName("h")] // High
    public decimal High { get; set; }

    [JsonPropertyName("l")] // Low
    public decimal Low { get; set; }

    [JsonPropertyName("c")] // Close
    public decimal Close { get; set; }

    [JsonPropertyName("v")] // Volume
    public decimal Volume { get; set; }
}