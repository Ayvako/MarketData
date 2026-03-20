namespace MarketData.Application.Interfaces;

public interface IFintachartsAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}