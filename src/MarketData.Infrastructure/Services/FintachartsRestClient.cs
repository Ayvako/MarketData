namespace MarketData.Infrastructure.Services;

using System.Net.Http.Json;
using MarketData.Application.Interfaces;
using MarketData.Application.Models.Fintacharts;
using Microsoft.Extensions.Configuration;

public class FintachartsRestClient : IFintachartsRestClient
{
    private readonly HttpClient httpClient;

    public FintachartsRestClient(HttpClient httpClient, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri(configuration["Fintacharts:BaseUrl"]!);
    }

    public async Task<List<InstrumentDto>> GetInstrumentsAsync(string provider = "oanda", string kind = "forex", CancellationToken cancellationToken = default)
    {
        var url = $"/api/instruments/v1/instruments?provider={provider}&kind={kind}";

        // Делаем GET запрос. О токене не думаем — его добавит FintachartsAuthHandler!
        var response = await this.httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<InstrumentDto>>(cancellationToken: cancellationToken).ConfigureAwait(false);

        return result?.Data ?? [];
    }
}