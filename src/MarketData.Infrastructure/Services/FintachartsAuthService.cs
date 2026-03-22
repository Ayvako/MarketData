namespace MarketData.Infrastructure.Services;

using System.Net.Http;
using System.Net.Http.Json;
using MarketData.Application.Interfaces;
using MarketData.Application.Models;
using Microsoft.Extensions.Configuration;

public class FintachartsAuthService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IFintachartsAuthService, IDisposable
{
    private readonly SemaphoreSlim semaphore = new(1, 1);

    private readonly IHttpClientFactory httpClientFactory = httpClientFactory;

    private readonly IConfiguration configuration = configuration;

    private string cachedToken = string.Empty;

    private DateTime tokenExpiryTime = DateTime.MinValue;

    private bool disposed;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (this.IsTokenValid())
        {
            return this.cachedToken;
        }

        await this.semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (this.IsTokenValid())
            {
                return this.cachedToken;
            }

            var httpClient = this.httpClientFactory.CreateClient("FintachartsAuth");
            var tokenUrl = this.configuration["Fintacharts:TokenUrl"]!;
            var requestData = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", this.configuration["Fintacharts:ClientId"]! },
                { "username", this.configuration["Fintacharts:Username"]! },
                { "password", this.configuration["Fintacharts:Password"]! },
            };

            using var content = new FormUrlEncodedContent(requestData);
            var response = await httpClient
                .PostAsync(tokenUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var tokenResult = await response.Content
                .ReadFromJsonAsync<TokenResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

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
        if (!this.disposed)
        {
            if (disposing)
            {
                this.semaphore.Dispose();
            }

            this.disposed = true;
        }
    }

    private bool IsTokenValid() =>
        !string.IsNullOrEmpty(this.cachedToken) && this.tokenExpiryTime > DateTime.UtcNow.AddSeconds(30);
}