using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace SmartParking.WebSockets;


public class WebSocketHeartbeatWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IWebSocketConnectionManager _webSocketConnectionManager;
    private readonly ILogger<WebSocketHeartbeatWorker> _logger;

    public WebSocketHeartbeatWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IWebSocketConnectionManager webSocketConnectionManager,
        ILogger<WebSocketHeartbeatWorker> logger // ✅ injected properly
    ) : base(timer, serviceScopeFactory)
    {
        _webSocketConnectionManager = webSocketConnectionManager;
        _logger = logger;
        _logger.LogInformation("WebSocketHeartbeatWorker constructor called.");

        Timer.Period = 5000;
    }
    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        _logger.LogInformation("Heartbeat worker triggered at {Time}", DateTime.Now);

        var allConnections = _webSocketConnectionManager.GetAllConnections(); // Dictionary<string, WebSocket>

        foreach (var connection in allConnections)
        {
            var connectionId = connection.Key;
            var socket = connection.Value;

            if (socket == null)
            {
                _logger.LogWarning("Socket not found for connectionId: {ConnectionId}", connectionId);
                continue;
            }

            try
            {
                if (socket.State != WebSocketState.Open)
                {
                    socket.Dispose();
                    _logger.LogInformation("Disposed closed socket for connectionId: {ConnectionId}", connectionId);
                    continue;
                }

                await SendHeartbeatAsync(socket, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending heartbeat to connectionId: {ConnectionId}", connectionId);
            }
        }
    }

    private async Task SendHeartbeatAsync(WebSocket socket, CancellationToken ct)
    {
        var message = $"Worker heartbeat at {DateTime.UtcNow:O}";
        var buffer = Encoding.UTF8.GetBytes(message);

        try
        {
            await socket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                ct);

            _logger.LogInformation("Heartbeat sent: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send heartbeat.");
        }
    }
}