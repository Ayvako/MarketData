namespace MarketData.Application.Models.Fintacharts;

using System.Text.Json.Serialization;

public class BarsResponse
{
    [JsonPropertyName("data")]
    public List<BarDto> Data { get; set; } = [];
}