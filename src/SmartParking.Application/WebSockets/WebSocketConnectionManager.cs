using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartParking.WebSockets;

public class WebSocketConnectionManager : IWebSocketConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private readonly ILogger<WebSocketConnectionManager> _logger;

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
        _logger = logger;
        _logger.LogInformation("WebSocketConnectionManager constructor called");
    }

    public Task AddConnection(string connectionId, WebSocket socket)
    {
        _logger.LogInformation("Adding connection (ConnId: {ConnId})", connectionId);

        _connections[connectionId] = socket;

        _logger.LogInformation("Total connections: {Count}", _connections.Count);
        return Task.CompletedTask;
    }

    public Task<WebSocket?> GetConnection(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var socket);
        return Task.FromResult(socket);
    }

    public Task<bool> TryGetConnection(string connectionId, out WebSocket socket)
    {
        return Task.FromResult(_connections.TryGetValue(connectionId, out socket));
    }

    public ConcurrentDictionary<string, WebSocket> GetAllConnections()
    {
        return _connections;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public async Task CloseAllConnectionsAsync()
    {
        foreach (var connection in _connections)
        {
            try
            {
                if (connection.Value.State == WebSocketState.Open)
                {
                    await connection.Value.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Server shutting down",
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing connection {ConnectionId}", connection.Key);
            }
        }
    }
}
