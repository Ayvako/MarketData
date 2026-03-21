namespace MarketData.Infrastructure.Services;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MarketData.Application.Interfaces;
using MarketData.Application.Models.Fintacharts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public partial class FintachartsWebSocketClient(IConfiguration config, IFintachartsAuthService authService, ILogger<FintachartsWebSocketClient> logger) : IDisposable
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
                this.socket?.Dispose();
                this.socket = new ClientWebSocket();

                var token = await this.authService.GetAccessTokenAsync(ct).ConfigureAwait(false);
                var uri = new Uri($"{this.config["Fintacharts:WssUrl"]}/api/streaming/ws/v1/realtime?token={token}");

                await this.socket.ConnectAsync(uri, ct).ConfigureAwait(false);
                LogConnected(this.logger);

                var buffer = new byte[1024 * 4];
                while (this.socket.State == WebSocketState.Open)
                {
                    var result = await this.socket.ReceiveAsync(new ArraySegment<byte>(buffer), ct).ConfigureAwait(false);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var update = JsonSerializer.Deserialize<WsPriceUpdateMessage>(message);

                    if (update?.Type == "l1-update")
                    {
                        onUpdate(update);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                LogConnectionCancelled(this.logger);
            }
            catch (Exception ex)
            {
                LogConnectionError(this.logger, ex.Message, ex);
                await Task.Delay(5000, ct).ConfigureAwait(false);
            }
        }
    }

    public async Task Subscribe(string instrumentId)
    {
        if (this.socket == null || this.socket.State != WebSocketState.Open)
        {
            LogSubscriptionWarning(this.logger, instrumentId);
            return;
        }

        var msg = new WsSubscriptionMessage { InstrumentId = instrumentId };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));

        try
        {
            await this.socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, default).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogSubscriptionError(this.logger, instrumentId, ex);
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
            this.socket?.Dispose();
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "WebSocket Connected to Fintacharts!")]
    static partial void LogConnected(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "WebSocket connection was cancelled.")]
    static partial void LogConnectionCancelled(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "WebSocket error: {Message}. Attempting to reconnect...")]
    static partial void LogConnectionError(ILogger logger, string message, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cannot subscribe to {InstrumentId}: WebSocket is not connected.")]
    static partial void LogSubscriptionWarning(ILogger logger, string instrumentId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error while subscribing to {InstrumentId}.")]
    static partial void LogSubscriptionError(ILogger logger, string instrumentId, Exception ex);
}