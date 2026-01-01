using SmartParking.OpeningGate;
using SmartParking.Organizations;
using SmartParking.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.OpenGate;

public class OpenGateFactoryService : IOpenGateFactoryService
{
    private readonly IEnumerable<IOpeningGateService> _services;
    private readonly IWebSocketConnectionManager _websocketConnectionManager;


    public OpenGateFactoryService(IEnumerable<IOpeningGateService> services, IWebSocketConnectionManager websocketConnectionManager)
    {
        _services = services;
        _websocketConnectionManager = websocketConnectionManager;
    }

    public async Task<IOpeningGateService> GetOpeningGateService(OrganizationDto organizationDto)
    {
        var websocket =  _websocketConnectionManager.GetConnection(organizationDto.Id.ToString());
        if (websocket != null)
        {
            IOpeningGateService service = (organizationDto.DispenserTransaction) switch
            {
                (true) => _services.FirstOrDefault(ser => ser.Name.Equals("Barrier", StringComparison.OrdinalIgnoreCase))!,
                (false) => _services.FirstOrDefault(ser => ser.Name.Equals("Camera", StringComparison.OrdinalIgnoreCase))!
            };
            return service;

        }
        return null;

    }


}
