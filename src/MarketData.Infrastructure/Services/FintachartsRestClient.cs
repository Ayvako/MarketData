namespace MarketData.Infrastructure.Services;

using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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

        var response = await this.httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<InstrumentDto>>(cancellationToken: cancellationToken).ConfigureAwait(false);

        return result?.Data ?? [];
    }

    public async Task<List<BarDto>> GetHistoricalBarsAsync(string instrumentId, string provider = "oanda", int interval = 1, string periodicity = "minute", int barsCount = 10, CancellationToken ct = default)
    {
        var url = $"/api/bars/v1/bars/count-back?instrumentId={instrumentId}&provider={provider}&interval={interval}&periodicity={periodicity}&barsCount={barsCount}";

        var response = await this.httpClient.GetAsync(url, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BarsResponse>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken: ct).ConfigureAwait(false);

        return result?.Data ?? [];
    }
}