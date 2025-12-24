using Castle.MicroKernel;
using SmartParking.Cameras;
using SmartParking.Entities.Cameras.Watchers;
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
using Volo.Abp;

namespace SmartParking.BarrierAppService;

public class BarrierAppService : SmartParkingAppService, IBarrierAppService , IOpeningGateService
{
    private readonly IWebSocketConnectionManager _websocketConnectionManager;
    private readonly ICamerasAppService _cameraAppService;

    public string Name => "Barrier";

    public BarrierAppService(IWebSocketConnectionManager websocketConnectionManager, ICamerasAppService cameraAppService)
    {
        _websocketConnectionManager = websocketConnectionManager;
        _cameraAppService = cameraAppService;
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
    [RemoteService(IsEnabled = false)]

    public async Task<bool> OpenGateAsync(OpenGateDto openGateDto)
    { 
        return await CallingBarrier(openGateDto.OrganizationId, new BarrierRequest
        {
            laneNO = openGateDto.WatcherCode,
            operatorType = 5
        });
    }
}
