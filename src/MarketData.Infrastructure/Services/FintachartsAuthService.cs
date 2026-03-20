namespace MarketData.Infrastructure.Services;

using System.Net.Http.Json;
using MarketData.Application.Interfaces;
using MarketData.Application.Models;
using Microsoft.Extensions.Configuration;

public class FintachartsAuthService(HttpClient httpClient, IConfiguration configuration) : IFintachartsAuthService, IDisposable
{
#pragma warning disable CA2213

    private readonly HttpClient httpClient = httpClient;

#pragma warning restore CA2213

    private readonly IConfiguration configuration = configuration;

    private readonly SemaphoreSlim semaphore = new(1, 1);

    private string cachedToken = string.Empty;

    private DateTime tokenExpiryTime = DateTime.MinValue;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(this.cachedToken) && this.tokenExpiryTime > DateTime.UtcNow.AddSeconds(30))
        {
            return this.cachedToken;
        }

        await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrEmpty(this.cachedToken) && this.tokenExpiryTime > DateTime.UtcNow.AddSeconds(30))
            {
                return this.cachedToken;
            }

            var tokenUrl = this.configuration["Fintacharts:TokenUrl"]!;

            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", this.configuration["Fintacharts:ClientId"]! },
                { "username", this.configuration["Fintacharts:Username"]! },
                { "password", this.configuration["Fintacharts:Password"]! },
            };

            using var content = new FormUrlEncodedContent(requestData);

            var response = await this.httpClient
                .PostAsync(tokenUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.AccessToken))
            {
                throw new InvalidOperationException("Fintacharts API returned an empty or null access token.");
            }

            this.cachedToken = tokenResult.AccessToken;
            this.tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResult.ExpiresIn);

            return this.cachedToken;
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.semaphore.Dispose();
        }
    }
}