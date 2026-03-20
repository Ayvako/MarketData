namespace MarketData.Infrastructure.Services;

using System.Net.Http.Headers;
using MarketData.Application.Interfaces;

public class FintachartsAuthHandler(IFintachartsAuthService authService) : DelegatingHandler
{
    private readonly IFintachartsAuthService authService = authService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = await this.authService.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}