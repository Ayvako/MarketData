namespace MarketData.Application.Interfaces;

using MarketData.Application.Models.Fintacharts;

public interface IFintachartsRestClient
{
    Task<List<InstrumentDto>> GetInstrumentsAsync(string provider = "oanda", string kind = "forex", CancellationToken cancellationToken = default);

    Task<List<BarDto>> GetHistoricalBarsAsync(string instrumentId, string provider = "oanda", int interval = 1, string periodicity = "minute", int barsCount = 10, CancellationToken ct = default);
}