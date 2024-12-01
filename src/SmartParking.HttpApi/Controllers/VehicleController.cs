
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
    [HttpGet("image")]
    [Authorize]
    public async  Task<string> GetImage(string base64String)
    {
		var imageFormat = "jpg"; // Adjust formats as needed

		// Convert Base64 string to byte array
		byte[] imageBytes = Convert.FromBase64String(base64String);
		
		// Define the path where the image will be saved
		string imageDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "QRCodeImage", "image");

		// Ensure the directory exists
		if (!Directory.Exists(imageDirectoryPath))
		{
			Directory.CreateDirectory(imageDirectoryPath);
		}
		// Generate a unique name for the image
		string fileName = $"{Guid.NewGuid()}.{imageFormat}";
		string fullPath = Path.Combine(imageDirectoryPath, fileName);

        Image img = Image.Load(imageBytes);
        img.Save(fullPath);
		// Save the image to the specified path
		//System.IO.File.WriteAllBytes(fullPath, imageBytes);

		// Return the relative path for storing in the database
		return $"/QRCodeImage/image/{fileName}";

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
    [HttpGet("licenceTypes")]
    [Authorize]
    public async Task<Result<List<LicenceTypeDto>>> GetAllLicenceTypes()
    {
        return await _licenceTypesAppService.GetAllLicenceTypesAsync();
    }
}
