using SmartParking.Lookups.VehicleBrand;
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

public interface IVehicleAppService : IApplicationService
{
    Task<Result<VehicleDto>> CreateAsync(CreateUpdateVehicleMobileDto input);
    Task<Result<VehicleDto>> EditAsync(int vehicleId,CreateUpdateVehicleMobileDto input);
    Task<Result<VehicleDto>> DeleteAsync(int vehicleId);
    Task<Result<List<VehicleDto>>> GetListAsync();


}
