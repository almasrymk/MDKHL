using Castle.MicroKernel;
using SmartParking.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SmartParking.BarrierAppService;

public class BarrierAppService : SmartParkingAppService, IBarrierAppService
{
    private readonly IWebSocketConnectionManager _websocketConnectionManager;

    public BarrierAppService(IWebSocketConnectionManager websocketConnectionManager)
    {
        _websocketConnectionManager = websocketConnectionManager;
    }

    public async Task<bool> CallingBarrier(int organizationId, BarrierRequest request)
    {
        try
        {
            var socket = await _websocketConnectionManager.GetConnection(organizationId.ToString());

            if (socket.State == WebSocketState.Open)
            {
                string json = JsonSerializer.Serialize(request);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }
}
