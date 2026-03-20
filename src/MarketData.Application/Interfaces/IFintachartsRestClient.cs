namespace MarketData.Application.Interfaces;

using MarketData.Application.Models.Fintacharts;

public interface IFintachartsRestClient
{
    Task<List<InstrumentDto>> GetInstrumentsAsync(string provider = "oanda", string kind = "forex", CancellationToken cancellationToken = default);
}