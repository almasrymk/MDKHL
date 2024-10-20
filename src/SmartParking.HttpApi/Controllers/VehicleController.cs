using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Invoices;
using SmartParking.Mobile.Vehicles;
using SmartParking.Results;
using SmartParking.Vehicles;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/mobile/Vehicle")]
[ApiController]
public class VehicleController : AbpController
{
    private readonly IVehicleAppService _vehicleAppService;
    public VehicleController(IVehicleAppService vehicleAppService)
    {
        _vehicleAppService = vehicleAppService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Result<VehicleDto>> CreateVehicle(CreateUpdateVehicleDto input)
    {
        return await _vehicleAppService.CreateAsync(input);
    }
    [HttpGet]
    [Authorize]
    public async Task<Result<PagedResultDto<VehicleDto>>> GetAllVehicles([FromQuery] PagedAndSortedResultRequestDto input)
    {
        return await _vehicleAppService.GetListAsync(input);
    }
}
