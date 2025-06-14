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
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> _connections = new();

    private readonly ILogger<WebSocketConnectionManager> _logger;

    public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
    {
        _logger = logger;
        _logger.LogInformation("WebSocketConnectionManager constructor called");

    }

    public Task<WebSocket?> GetConnection(string organizationId, string connectionId)
    {
        if (_connections.TryGetValue(organizationId, out var orgConnections))
        {
            if (orgConnections.TryRemove(connectionId, out var socket))
            {
                return Task.FromResult<WebSocket?>(socket);
            }
        }
        return Task.FromResult<WebSocket?>(null);

    }

    public Task<IEnumerable<WebSocket>> GetConnectionsByOrganization(string organizationId)
    {
        if (_connections.TryGetValue(organizationId, out var orgConnections))
        {
            return Task.FromResult<IEnumerable<WebSocket>>(orgConnections.Values);
        }

        return Task.FromResult<IEnumerable<WebSocket>>(Enumerable.Empty<WebSocket>());
    }

    public IEnumerable<string> GetAllOrganizationIds()
    {
        return _connections.Keys;
    }

    public bool TryGetConnection(string organizationId, string connectionId, out WebSocket socket)
    {
        socket = null;
        return _connections.TryGetValue(organizationId, out var orgConnections) &&
               orgConnections.TryGetValue(connectionId, out socket);
    }

    public async Task CloseAllConnectionsAsync()
    {
        foreach (var org in _connections)
        {
            foreach (var connection in org.Value)
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
                    _logger.LogError(ex, "Error closing connection {ConnectionId} for organization {OrganizationId}",
                        connection.Key, org.Key);
                }
            }
        }
    }
    public Task RemoveConnectionAsync(string organizationId, string connectionId)
    {
        if (_connections.TryGetValue(organizationId, out var connections))
        {
            connections.TryRemove(connectionId, out _);

            if (connections.IsEmpty)
            {
                _connections.TryRemove(organizationId, out _);
            }
        }

        return Task.CompletedTask;
    }

    public Task AddConnection(string organizationId, string connectionId, WebSocket socket)
    {
        _logger.LogInformation("Adding connection (Org: {OrgId}, Conn: {ConnId})", organizationId, connectionId);

        var orgConnections = _connections.GetOrAdd(
            organizationId,
            _ => new ConcurrentDictionary<string, WebSocket>()
        );

        orgConnections.AddOrUpdate(connectionId, socket, (_, _) => socket);

        _logger.LogInformation("Total connections for {OrgId}: {Count}",
            organizationId, orgConnections.Count);

        _logger.LogInformation("Total subscribers : {Count}",
    _connections.Count);

        return Task.CompletedTask;
    }

    public ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> GetAllConnections()
    {
        return _connections;
    }
}
