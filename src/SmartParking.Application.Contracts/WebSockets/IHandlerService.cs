using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.WebSockets;

public interface IHandlerService
{
    Task<string> ReceiveAsync(int organizationId, WebSocket webSocket);
    Task<TResponse> SendMessageAsync<TResponse>(int orgId, WebSocket webSocket, object data);
    Task<FindMyCarResponse> FindMyCarAsync(int orgId, string action, WebSocket webSocket, string licensePlate);

}
