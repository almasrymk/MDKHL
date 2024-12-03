
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SmartParking.Lookups.LicenceType;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Lookups.VehicleModel;
using SmartParking.Mobile.Vehicles;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly ILicenceTypesAppService _licenceTypesAppService;

    public VehicleController(IVehicleAppService vehicleAppService,IVehicleBrandsAppService vehicleBrandsAppService,IVehicleModelsAppService vehicleModelsAppService,ILicenceTypesAppService licenceTypesAppService)
    {
        _vehicleAppService = vehicleAppService;
        _vehicleBrandsAppService = vehicleBrandsAppService;
        _vehicleModelsAppService = vehicleModelsAppService;
        _licenceTypesAppService = licenceTypesAppService;
    }

    [HttpPost]
    [Authorize]
    public async Task<Result<VehicleDto>> CreateVehicle(CreateUpdateVehicleMobileDto input)
    {
        return await _vehicleAppService.CreateAsync(input);
    }    
    [HttpPut]
    [Authorize]
    public async Task<Result<VehicleDto>> EditVehicle(int vehicleId, CreateUpdateVehicleMobileDto input)
    {
        return await _vehicleAppService.EditAsync(vehicleId,input);
    }    
    [HttpDelete]
    [Authorize]
    public async Task<Result<VehicleDto>> DeleteVehicle(int vehicleId)
    {
        return await _vehicleAppService.DeleteAsync(vehicleId);
    }
    [HttpGet]
    [Authorize]
    public async Task<Result<List<VehicleDto>>> GetAllVehicles()
    {
        return await _vehicleAppService.GetListAsync();
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
    [HttpGet("platetypes")]
    [Authorize]
    public async Task<Result<List<LicenceTypeDto>>> GetAllLicenceTypes()
    {
        return await _licenceTypesAppService.GetAllLicenceTypesAsync();
    }
}
