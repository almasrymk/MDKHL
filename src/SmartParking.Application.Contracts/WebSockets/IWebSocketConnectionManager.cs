using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.WebSockets;

public interface IWebSocketConnectionManager
{
    Task AddConnection(string connectionId, WebSocket socket);
    Task<WebSocket?> GetConnection(string connectionId);
    Task<bool> TryGetConnection(string connectionId, out WebSocket socket);
    ConcurrentDictionary<string, WebSocket> GetAllConnections();
    Task RemoveConnectionAsync(string connectionId);
    Task CloseAllConnectionsAsync();
}


