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

    Task AddConnection(string organizationId, string connectionId, WebSocket socket);
    Task<WebSocket?> GetConnection(string organizationId, string connectionId);
    Task<IEnumerable<WebSocket>> GetConnectionsByOrganization(string organizationId);
    Task RemoveConnectionAsync(string organizationId, string connectionId);
    Task CloseAllConnectionsAsync();
    ConcurrentDictionary<string, ConcurrentDictionary<string, WebSocket>> GetAllConnections();

}

