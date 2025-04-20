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
using Volo.Abp.MultiTenancy;
using SmartParking.Lookups.LicenceType;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;


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
        using (_dataFilter.Disable<IMultiTenant>())
        {
            try
            {
                var mappedVehicle = ObjectMapper.Map<CreateUpdateVehicleMobileDto, Vehicle>(input);
                // check plate number must include both plate letters and plate numbers, either in Arabic or English.

                mappedVehicle.ArPlateNumber = input.ArPlateNumber.Replace(" ", string.Empty) + input.ArPlateLetters.Replace(" ", string.Empty);



                mappedVehicle.EnPlateNumber = input.EnPlateNumber.Replace(" ", string.Empty) + input.EnPlateLetters.Replace(" ", string.Empty);
                var existVehicle = await _vehicleRepository.FirstOrDefaultAsync(_ => _.ArPlateNumber == mappedVehicle.ArPlateNumber || _.EnPlateNumber == mappedVehicle.EnPlateNumber);
                if (existVehicle is not null)
                {
                    return Result<VehicleDto>.Invalid(new ValidationError
                    {
                        ErrorCode = StatusCodes.Status400BadRequest.ToString(),
                        ErrorMessage = "this vehicle is exist"
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

                mappedVehicle.LicenceType = plateType;
                mappedVehicle.LicenceTypeId = plateType.Id;
                mappedVehicle.PlateColorId = plateType.PlateColorId;
                mappedVehicle.VehicleModelId = model.Id;
                mappedVehicle.VehicleBrandId = brand.Id;
                mappedVehicle.CustomerId = customer.Id;
                ObjectHelper.TrySetProperty(
                            mappedVehicle,
                            x => x.CreatorId,
                            () => CurrentUser.Id
                        );
                var vehicle = await _vehicleRepository.InsertAsync(mappedVehicle, autoSave: true);

                var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);

                return Result<VehicleDto>.Success(vehicleDto, "Vehicle Update Succesuffuly");
            }
            catch (Exception ex)
            {
                return Result<VehicleDto>.Error(ex.Message);
            }
        }
    }


    public async Task<Result<VehicleDto>> DeleteAsync(int vehicleId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var vehicle = await _vehicleRepository.GetAsync(vehicleId);
            if (vehicle == null)
            {
                return Result<VehicleDto>.NotFound("vehicle not found");
            }
            await _vehicleRepository.HardDeleteAsync(vehicle, autoSave: true);
            var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
            return Result<VehicleDto>.Success(vehicleDto, "Vehicle Delete Succesuffuly");
        }
	}

    public async Task<Result<VehicleDto>> EditAsync(int vehicleId, CreateUpdateVehicleMobileDto input)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {

            var vehicle = await _vehicleRepository.GetAsync(vehicleId);
            if (vehicle == null)
            {
                return Result<VehicleDto>.NotFound("vehicle not found");
            }


            // check plate number must include both plate letters and plate numbers, either in Arabic or English.
            if (!input.ArPlateLetters.IsNullOrEmpty() && !input.ArPlateNumber.IsNullOrEmpty())
            {
                vehicle.ArPlateNumber = input.ArPlateLetters + input.ArPlateNumber;

            }
            if (!input.EnPlateLetters.IsNullOrEmpty() && !input.EnPlateNumber.IsNullOrEmpty())
            {
                vehicle.EnPlateNumber = input.EnPlateLetters + input.EnPlateNumber;
            }
            else
            {
                return Result<VehicleDto>.Invalid(new ValidationError
                {
                    ErrorCode = StatusCodes.Status400BadRequest.ToString(),
                    ErrorMessage = "Plate number must include both plate letters and plate numbers, either in Arabic or English."

                });
            }
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
            vehicle.LicenceType = plateType;
            vehicle.LicenceTypeId = plateType.Id;
            vehicle.PlateColorId = plateType.PlateColorId;
            vehicle.VehicleModelId = model.Id;
            vehicle.VehicleBrandId = brand.Id;
            vehicle.CustomerId = customer.Id;
            vehicle.ModelYear = input.Year;
            vehicle.LastModificationTime = DateTime.Now;
            vehicle.LastModifierId = CurrentUser.Id;
            var vehicleUpdate = await _vehicleRepository.UpdateAsync(vehicle, autoSave: true);

            var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicleUpdate);

            return Result<VehicleDto>.Success(vehicleDto, "Vehicle Edit Succesuffuly");
        }
	}

    public async Task<Result<List<VehicleDto>>> GetListAsync()
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var queryable = await _vehicleRepository.GetQueryableAsync();
            queryable = queryable.Where(_ => _.Customer.IdentityUser.Id == CurrentUser.Id);
            queryable = queryable.OrderBy(v => v.Id);
            var vehicles = await AsyncExecuter.ToListAsync(queryable);
            var vehiclesDto = ObjectMapper.Map<List<Vehicle>, List<VehicleDto>>(vehicles);
            return vehiclesDto;
           
        }
        

    }   
    
    public async Task<Result<VehicleDto>> GetAsync(int vehicleId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var vehicle = await _vehicleRepository.FirstOrDefaultAsync(v => v.Id == vehicleId);
           if(vehicle == null)
                return Result<VehicleDto>.NotFound("vehicle not found");
            
            var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
            return vehicleDto;
           
        }
        

    }

    public async Task<VehicleDto> GetVehicleByPlateNumberAsync(string plateNumber)
    {
        var vehicle = await _vehicleRepository.FirstOrDefaultAsync(v => v.ArPlateNumber == plateNumber || v.EnPlateNumber == plateNumber);
        var vehicleDto = ObjectMapper.Map<Vehicle, VehicleDto>(vehicle);
        return vehicleDto;
    }

}