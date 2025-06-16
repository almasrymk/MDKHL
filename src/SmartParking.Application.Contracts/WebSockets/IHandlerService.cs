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

}
