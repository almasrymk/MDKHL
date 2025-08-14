using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.OpeningGate;

public interface IOpenGateFactoryService
{
     Task<IOpeningGateService> GetOpeningGateService(OrganizationDto organizationDto);
}
