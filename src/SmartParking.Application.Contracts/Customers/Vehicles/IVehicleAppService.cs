using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Customers.Vehicles;

public interface IVehicleAppService : ICrudAppService<VehicleDto, int, PagedAndSortedResultRequestDto, CreatedUpdatedVehicleDto>
{
}
