using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Mobile.Vehicles;

public interface IVehicleService 
{
    Task<Result<VehicleDto>> CreateAsync(CreateUpdateVehicleDto input);
    Task<Result<PagedResultDto<VehicleDto>>> GetListAsync(PagedAndSortedResultRequestDto input);
}
