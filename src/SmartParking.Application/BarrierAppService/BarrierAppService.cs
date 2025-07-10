using Castle.MicroKernel;
using SmartParking.GarageTransactions;
using SmartParking.OpeningGate;
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

public class BarrierAppService : SmartParkingAppService, IBarrierAppService , IOpeningGateService
{
    private readonly IWebSocketConnectionManager _websocketConnectionManager;
    public string Name => "Barrier";

    public BarrierAppService(IWebSocketConnectionManager websocketConnectionManager)
    {
        _websocketConnectionManager = websocketConnectionManager;
    }

    public async Task<bool> CallingBarrier(int organizationId, BarrierRequest request)
    {
        try
        {
            var socket = await _websocketConnectionManager.GetConnection(organizationId.ToString());

            if (socket != null && socket.State == WebSocketState.Open)
            {
                string json = JsonSerializer.Serialize(request);
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                return true;
            }
            return false;

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public async Task<bool> OpenGateAsync(GarageTransactionDto garageTransactionDto)
    {
        return await CallingBarrier(garageTransactionDto.OrganizationID, new BarrierRequest { carNum = garageTransactionDto.PlateNumber, laneNO = garageTransactionDto.LaneNO, operatorType = 5 });
    }
}
