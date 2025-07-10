using SmartParking.OpeningGate;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.OpenGate;

public class OpenGateFactoryService : IOpenGateFactoryService
{
    private readonly IEnumerable<IOpeningGateService> _services;

    public OpenGateFactoryService(IEnumerable<IOpeningGateService> services)
    {
        _services = services;
    }

    public IOpeningGateService GetOpeningGateService(OrganizationDto organizationDto)
    {
        IOpeningGateService service = (organizationDto.DispenserTransaction) switch
        {
            (true) => _services.FirstOrDefault(ser => ser.Name.Equals("Barrier", StringComparison.OrdinalIgnoreCase)),
            (false) => _services.FirstOrDefault(ser => ser.Name.Equals("Camera", StringComparison.OrdinalIgnoreCase))
        };
        return service;
    }
}
