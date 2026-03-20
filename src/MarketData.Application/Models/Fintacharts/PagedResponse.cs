namespace MarketData.Application.Models.Fintacharts;

using System.Text.Json.Serialization;

public class PagedResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = [];
}