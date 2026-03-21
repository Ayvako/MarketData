namespace MarketData.Infrastructure.Services;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MarketData.Application.Interfaces;
using MarketData.Application.Models.Fintacharts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class FintachartsWebSocketClient(IConfiguration config, IFintachartsAuthService authService, ILogger<FintachartsWebSocketClient> logger) : IDisposable
{
    private readonly IConfiguration config = config;

    private readonly IFintachartsAuthService authService = authService;

    private readonly ILogger<FintachartsWebSocketClient> logger = logger;

    private ClientWebSocket? socket;

    public async Task ConnectAndListen(Action<WsPriceUpdateMessage> onUpdate, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(onUpdate);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                this.socket = new ClientWebSocket();
                var token = await this.authService.GetAccessTokenAsync(ct).ConfigureAwait(false);
                var uri = new Uri($"{this.config["Fintacharts:WssUrl"]}/api/streaming/ws/v1/realtime?token={token}");

                await this.socket.ConnectAsync(uri, ct).ConfigureAwait(false);

                this.logger.LogInformation("WebSocket Connected to Fintacharts!");

                var buffer = new byte[1024 * 4];
                while (this.socket.State == WebSocketState.Open)
                {
                    var result = await this.socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var update = JsonSerializer.Deserialize<WsPriceUpdateMessage>(message);
                    if (update?.Type == "l1-update")
                    {
                        onUpdate(update);
                    }
                }
            }
            catch
            {
                await Task.Delay(5000, ct).ConfigureAwait(false);
            }
        }
    }

    public async Task Subscribe(string instrumentId)
    {
        ArgumentNullException.ThrowIfNull(this.socket);

        var msg = new WsSubscriptionMessage { InstrumentId = instrumentId };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
        await this.socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, default).ConfigureAwait(false);
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
            this.socket?.Dispose();
        }
    }
}