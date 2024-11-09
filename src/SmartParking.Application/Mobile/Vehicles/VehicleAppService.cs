using Microsoft.AspNetCore.Http;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Lookups.VehiclesLookups;
using SmartParking.Entities.Vehicles;
using SmartParking.Lookups.VehicleBrand;
using SmartParking.Results;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;
using System.Drawing;
using System.Text;


namespace SmartParking.Mobile.Vehicles;

[RemoteService(IsEnabled = false)]

public class VehicleAppService : ApplicationService, IVehicleAppService
{
    public readonly IRepository<Vehicle, int> _vehicleRepository;
    public readonly ICurrentUser _currentUser;
    private IDataFilter _dataFilter;
    private readonly IRepository<LicenceType, int> _licenceTypeRepository;
    private readonly IRepository<VehicleBrand, int> _brandRepository;
    private readonly IRepository<VehicleModel, int> _modelRepository;
	private readonly IRepository<Customer, Guid> _customerRepository;

	public VehicleAppService(IRepository<Vehicle, int> vehicleRepository, ICurrentUser currentUser, IDataFilter dataFilter,IRepository<LicenceType,int> LicenceTypeRepository,IRepository<VehicleBrand,int> BrandRepository, IRepository<VehicleModel, int> ModelRepository, IRepository<Customer, Guid> CustomerRepository)
    {
        _vehicleRepository = vehicleRepository;
        _currentUser = currentUser;
        _dataFilter = dataFilter;
        _licenceTypeRepository = LicenceTypeRepository;
        _brandRepository = BrandRepository;
        _modelRepository = ModelRepository;
		_customerRepository = CustomerRepository;
	}

	[RemoteService(IsEnabled =false)]
    public async Task<Result<VehicleDto>> CreateAsync(CreateUpdateVehicleMobileDto input)
    {
        try
        {  
            var mappedVehicle = ObjectMapper.Map<CreateUpdateVehicleMobileDto, Vehicle>(input);
            // check plate number must include both plate letters and plate numbers, either in Arabic or English.
            if (!input.ArPlateLetters.IsNullOrEmpty() && !input.ArPlateNumber.IsNullOrEmpty())
            {
                mappedVehicle.PlateNumber = input.ArPlateLetters + input.ArPlateNumber;
                
            }
            else if(!input.EnPlateLetters.IsNullOrEmpty() && !input.EnPlateNumber.IsNullOrEmpty())
            {
                mappedVehicle.PlateNumber = input.EnPlateLetters + input.EnPlateNumber;
            }
            else
            {
                return Result<VehicleDto>.Invalid(new ValidationError
                {
                    ErrorCode = StatusCodes.Status400BadRequest.ToString(),
                    ErrorMessage = "Plate number must include both plate letters and plate numbers, either in Arabic or English."

                });
            }
            // LicenceType is plate type is found 
            var plateType = await _licenceTypeRepository.FirstOrDefaultAsync(l => l.ArabicName == input.plateType || l.EnglishName == input.plateType);
            if(plateType == null)
            {
                return Result<VehicleDto>.NotFound("Plate type not found");
                
            }
            var brand = await _brandRepository.FirstOrDefaultAsync(b => b.Id==input.BrandId);
            if(brand == null)
            {
                return Result<VehicleDto>.NotFound("Vehicle Brand not found");
            }            
            var model = await _modelRepository.FirstOrDefaultAsync(b => b.Id==input.ModelId);
            if(model == null)
            {
                return Result<VehicleDto>.NotFound("Vehicle Model not found");
            }
            var customer = await _customerRepository.FirstOrDefaultAsync(b => b.IdentityUserId == CurrentUser.Id);

            mappedVehicle.LicenceType = plateType;
            mappedVehicle.LicenceTypeId = plateType.Id;
            mappedVehicle.PlateColorId = plateType.PlateColorId;
            mappedVehicle.VehicleModelId = model.Id;
            mappedVehicle.VehicleBrandId = brand.Id;
            mappedVehicle.CustomerId = customer.Id;
            var vehicle = await _vehicleRepository.InsertAsync(mappedVehicle, autoSave: true);

            var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);

            return Result<VehicleDto>.Success(vehicleDto, "Vehicle Update Succesuffuly");
        }
        catch (Exception ex)
        {
            return Result<VehicleDto>.Error(ex.Message);
        }

    }

   

    public async Task<Result<VehicleDto>> DeleteAsync(int vehicleId)
    {
		var vehicle = await _vehicleRepository.GetAsync(vehicleId);
		if (vehicle == null)
		{
			return Result<VehicleDto>.NotFound("vehicle not found");
		}
        await _vehicleRepository.DeleteAsync(vehicle, autoSave: true);
		var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
		return Result<VehicleDto>.Success(vehicleDto, "Vehicle Delete Succesuffuly");
	}

    public async Task<Result<VehicleDto>> EditAsync(int vehicleId, CreateUpdateVehicleMobileDto input)
    {
		try
		{
            var vehicle = await _vehicleRepository.GetAsync(vehicleId);
            if (vehicle == null)
            {
                return Result<VehicleDto>.NotFound("vehicle not found");
            }
            
            var mappedVehicle = ObjectMapper.Map<CreateUpdateVehicleMobileDto, Vehicle>(input);
            // check plate number must include both plate letters and plate numbers, either in Arabic or English.
			if (!input.ArPlateLetters.IsNullOrEmpty() && !input.ArPlateNumber.IsNullOrEmpty())
			{
				mappedVehicle.PlateNumber = input.ArPlateLetters + input.ArPlateNumber;

			}
			else if (!input.EnPlateLetters.IsNullOrEmpty() && !input.EnPlateNumber.IsNullOrEmpty())
			{
				mappedVehicle.PlateNumber = input.EnPlateLetters + input.EnPlateNumber;
			}
			else
			{
				return Result<VehicleDto>.Invalid(new ValidationError
				{
					ErrorCode = StatusCodes.Status400BadRequest.ToString(),
					ErrorMessage = "Plate number must include both plate letters and plate numbers, either in Arabic or English."

				});
			}
			// LicenceType is plate type is found 
			var plateType = await _licenceTypeRepository.FirstOrDefaultAsync(l => l.ArabicName == input.plateType || l.EnglishName == input.plateType);
			if (plateType == null)
			{
				return Result<VehicleDto>.NotFound("Plate type not found");

			}
			var brand = await _brandRepository.FirstOrDefaultAsync(b => b.Id == input.BrandId);
			if (brand == null)
			{
				return Result<VehicleDto>.NotFound("Vehicle Brand not found");
			}
			var model = await _modelRepository.FirstOrDefaultAsync(b => b.Id == input.ModelId);
			if (model == null)
			{
				return Result<VehicleDto>.NotFound("Vehicle Model not found");
			}
			var customer = await _customerRepository.FirstOrDefaultAsync(b => b.IdentityUserId == CurrentUser.Id);
            mappedVehicle.Id = vehicleId;
			mappedVehicle.LicenceType = plateType;
			mappedVehicle.LicenceTypeId = plateType.Id;
			mappedVehicle.PlateColorId = plateType.PlateColorId;
			mappedVehicle.VehicleModelId = model.Id;
			mappedVehicle.VehicleBrandId = brand.Id;
			mappedVehicle.CustomerId = customer.Id;
			var vehicleUpdate = await _vehicleRepository.UpdateAsync(mappedVehicle, autoSave: true);

			var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicleUpdate);

			return Result<VehicleDto>.Success(vehicleDto, "Vehicle Add Succesuffuly");
		}
		catch (Exception ex)
		{
			return Result<VehicleDto>.Error(ex.Message);
		}
	}

    public async Task<Result<PagedResultDto<VehicleDto>>> GetListAsync(PagedAndSortedResultRequestDto input)
    {

        // Check if the user is authenticated
        //if (!_currentUser.IsInRole("admin"))
        //{
        //    return Result<VehicleDto>.Error("User is not authenticated");
        //}

        var queryable = await _vehicleRepository.GetQueryableAsync();

        if (!input.Sorting.IsNullOrEmpty())
        {
            if (input.Sorting.Contains("DESC"))
            {
                var sorting = input.Sorting.Split(' ').ToList();
                queryable = queryable.OrderBy(sorting[0] + " descending");
            }
            else
            {
                queryable = queryable.OrderBy(input.Sorting);
            }

        }
        //Get the total count of filtered records
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        //Apply pagination
        queryable = queryable.Skip(input.SkipCount).Take(input.MaxResultCount);
        //Execute the query and get the list of hotel entities
        var vehicles = await AsyncExecuter.ToListAsync(queryable);

        // Map the hotel entities to hotelDto objects
        var vehiclesDto = ObjectMapper.Map<List<Vehicle>, List<VehicleDto>>(vehicles);
        // Return the paginated result
        return new Result<PagedResultDto<VehicleDto>>(
           new PagedResultDto<VehicleDto>
           (totalCount, vehiclesDto)
       );

    }



}