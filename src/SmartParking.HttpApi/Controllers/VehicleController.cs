using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartParking.Invoices;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Mobile.Vehicles;
using SmartParking.Results;
using SmartParking.Vehicles;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace SmartParking.Controllers;

[Route("api/mobile/Vehicle")]
[ApiController]
public class VehicleController : AbpController
{
    private readonly IVehicleAppService _vehicleAppService;
    private readonly IVehicleBrandsAppService _vehicleBrandsAppService;
    private readonly IVehicleModelsAppService _vehicleModelsAppService;

    public VehicleController(IVehicleAppService vehicleAppService,IVehicleBrandsAppService vehicleBrandsAppService,IVehicleModelsAppService vehicleModelsAppService)
    {
        _vehicleAppService = vehicleAppService;
        _vehicleBrandsAppService = vehicleBrandsAppService;
        _vehicleModelsAppService = vehicleModelsAppService;
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
    [HttpGet("getbrands")]
    [Authorize]
    public async Task<Result<List<VehicleBrandDto>>> GetBrands()
    {
        return await _vehicleBrandsAppService.GetbrandsAsync();
    }    
    [HttpGet("modelsbybrand")]
    [Authorize]
    public async Task<Result<List<VehicleModelDto>>> GetModelsByBrand(int brandId)
    {
        return await _vehicleModelsAppService.GetModelsByBrandAsync(brandId);
    }
}
