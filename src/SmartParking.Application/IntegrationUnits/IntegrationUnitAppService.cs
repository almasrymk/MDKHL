using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SmartParking.Cameras;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Entities.Cameras.Watchers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.CustomerUnitPackages;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using SmartParking.IntegrationUnit.Options;
using SmartParking.Lookups.VehicleLookups;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using SmartParking.UnitPackages;
using SmartParking.Units;
using SmartParking.UnitVehiclePackages;
using SmartParking.Utilities.StaticValues;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;


namespace SmartParking.IIntegrationUnit;

public class IntegrationUnitAppService : ApplicationService, IIntegrationUnitAppService
{
    private readonly ICustomerAppService _customerAppService;
    private readonly IUnitAppService _unitAppService;
    private readonly IVehiclesAppService _vehicleAppService;
    private readonly IOrganizationsAppService _organizationsAppService;
    private readonly ICustomerUnitPackageAppService _customerUnitPackageAppService;
    private readonly IAssignCustomerPackagesAppService _customerPackageAppService;
    private readonly IUnitVehiclePackageAppService _unitVehiclePackageAppService;
    private readonly IVehicleLookupResolver _vehicleLookupResolver;
    private readonly ICustomerPackagesAppService _customerPackagesAppService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IConfiguration _configuration;
    private readonly MyIntegrationOptions _options;

    public IntegrationUnitAppService(ICustomerAppService customerAppService,
        IVehiclesAppService vehicleAppService,
        IOrganizationsAppService organizationsAppService,
        ICustomerUnitPackageAppService customerUnitPackageAppService,
        IAssignCustomerPackagesAppService customerPackageAppService,
        IUnitVehiclePackageAppService unitVehiclePackageAppService,
        IVehicleLookupResolver vehicleLookupResolver,
        IUnitOfWorkManager unitOfWorkManager,
        IOptions<MyIntegrationOptions> options,
        ICustomerPackagesAppService customerPackagesAppService,
        IConfiguration configuration,
        IUnitAppService unitAppService)
    {
        _customerAppService = customerAppService;
        _vehicleAppService = vehicleAppService;
        _organizationsAppService = organizationsAppService;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _customerPackageAppService = customerPackageAppService;
        _unitVehiclePackageAppService = unitVehiclePackageAppService;
        _vehicleLookupResolver = vehicleLookupResolver;
        _unitOfWorkManager = unitOfWorkManager;
        _options = options.Value;
        _customerPackagesAppService = customerPackagesAppService;
        _configuration = configuration;
        _unitAppService = unitAppService;
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto dto)
    {
        try
        {
            var unit = await _unitAppService.GetActiveUnitByCodeAsync(dto.UnitId);
            if (!unit.IsSuccess)
            {
                return Result.Error(@L["UnitNotFound"]);
            }
            var getCustomer = await _customerAppService.GetCustomerByPhoneNumberAsync(dto.Customer.Mobile);
            var customer = (getCustomer is null) ? await CreateCustomer(dto.Customer) : getCustomer;
            var vehicles = await CreateVehicle(dto.Customer.Cars, customer.Id);
            var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
            if (vehicles.Count > organization.OrganizationSettings.RentalSlots)
            {
                throw new Exception("Count of vehicles bigger than number of slots");
            }
            var resultValidate = await ValidateCustomerUnitPackage(customer.Id, dto, false);
            if (!resultValidate.IsSuccess)
            {
                throw new Exception(resultValidate.Errors.FirstOrDefault()!);
            }
            var customerUnitPackage = await CreateCustomerUnitPackage(customer,dto, customer.Id);
            var customerPackages = await CreateCustomerPackage(customerUnitPackage, vehicles, customer.MobileNumber);
            var unitVehiclePackages = await CreateUnitVehiclePackage(vehicles, customerUnitPackage);
            BookingDto result = new BookingDto
            {
                Status = Status.Active,
                PhoneNumber = customer.MobileNumber,
                PackageId = customerPackages.PackageCode.ToString(),
                PackageNumber = customerPackages.PackageCode.ToString(),
                Validity = new ValidityDto
                {
                    StartDate = customerUnitPackage.ActivationDate,
                    EndDate = customerUnitPackage.DectivationDate
                },
                Qr = new QrDto
                {
                    Required = true,
                    QrCodePayload = String.Empty,
                    QrCodeUrl = $"/{customerPackages.QrCodePath?.TrimStart('/')}",
                    ExpiresAt = customerUnitPackage.DectivationDate
                },
                Links = new LinksDto
                {
                    bookingId = dto.Booking.BookingId,
                    UnitId = dto.UnitId
                }
            };
            return Result.Success(result);
        }
        catch (Volo.Abp.BusinessException ex)
        {
            if (_unitOfWorkManager.Current != null)
            {
                await _unitOfWorkManager.Current.RollbackAsync();
            }
            return Result.Error(ex.Code);
        }
        catch(InvalidOperationException ex)
        {
            if (_unitOfWorkManager.Current != null)
            {
                await _unitOfWorkManager.Current.RollbackAsync();
            }
            return Result.Error(ex.Message);

        }
        catch (Exception ex)
        {
            if (_unitOfWorkManager.Current != null)
            {
                await _unitOfWorkManager.Current.RollbackAsync();
            }
            return Result.Error(ex.Message);
        }
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> UpdateBookingAsync(CreateUpdateBookingDto dto)
    {
        try
        {
            var customerDto = await _customerAppService.GetCustomerByPhoneNumberAsync(dto.phoneNumber);
            var customer = await UpdateCustomer(customerDto.Id, dto.Customer.MapToCustomerDto(dto.Customer));

            

            var searchCustomerPackageDto = new SearchByPlateAndCustomerIdDto
            {
                CustomerId = customer.Id
            };





            CustomerPackageDto customerPackages = new CustomerPackageDto();
            UnitVehiclePackageDto unitVehiclePackage = new UnitVehiclePackageDto();
            CustomerUnitPackageDto customerUnitPackage = new CustomerUnitPackageDto();

            var Cars = await _vehicleAppService.GetVehiclesByCustomerIdAsync(customer.Id);


            for (int i = 0; i < Cars.Count(); i++)
            {

                searchCustomerPackageDto.EnPlateNumber = Cars[i].EnPlateNumber;
                customerPackages = await _customerPackagesAppService.GetLatestByCustomerIdAndEnPlateNumber(searchCustomerPackageDto);
                unitVehiclePackage = await _unitVehiclePackageAppService.GetByCustmoerIdAndPlateNumber(searchCustomerPackageDto);
                customerUnitPackage = await _customerUnitPackageAppService.GetAsync(unitVehiclePackage.CustomerUnitPackageId);

                await _unitVehiclePackageAppService.DeleteByCustomerUnitPackage(customerUnitPackage.Id);
                await _customerUnitPackageAppService.DeleteAsync(customerUnitPackage.Id);
                await _customerPackagesAppService.DeleteAsync(customerPackages.Id);
                await _vehicleAppService.DeleteAsync(Cars[i].Id);

            }

            await _unitOfWorkManager.Current!.SaveChangesAsync();

            var vehicles = await CreateVehicle(dto.Customer.Cars, customer.Id);
            var resultValidate = await ValidateCustomerUnitPackage(customer.Id, dto, false);
            if (!resultValidate.IsSuccess)
            {
                throw new Exception(resultValidate.Errors.FirstOrDefault()!);
            }
            var createdcustomerUnitPackage = await CreateCustomerUnitPackage(customer, dto, customer.Id);
            var createdcustomerPackages = await CreateCustomerPackage(customerUnitPackage, vehicles, customer.MobileNumber);
            var unitVehiclePackages = await CreateUnitVehiclePackage(vehicles, customerUnitPackage);


            BookingDto result = new BookingDto
            {
                Status = Status.Active,
                PhoneNumber = customer.MobileNumber,
                PackageId = createdcustomerPackages.PackageCode.ToString(),
                PackageNumber = createdcustomerPackages.PackageCode.ToString(),
                Validity = new ValidityDto
                {
                    StartDate = createdcustomerUnitPackage.ActivationDate,
                    EndDate = createdcustomerUnitPackage.DectivationDate
                },
                Qr = new QrDto
                {
                    Required = true,
                    QrCodePayload = String.Empty,
                    QrCodeUrl = $"/{createdcustomerPackages.QrCodePath?.TrimStart('/')}",
                    ExpiresAt = createdcustomerUnitPackage.DectivationDate
                },
                Links = new LinksDto
                {
                    bookingId = dto.Booking.BookingId,
                    UnitId = dto.UnitId
                }
            };
            return result;
        }
        catch (Exception ex) 
        {
            if (_unitOfWorkManager.Current != null)
            {
                await _unitOfWorkManager.Current.RollbackAsync();
            }
            return Result.Error(ex.Message);
        }
        
    }

    private async Task<CustomerDto> UpdateCustomer(Guid customerId ,CreateUpdateCustomerDto customerDto)
    {
        customerDto.TenantId = CurrentTenant.Id;
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        customerDto.OrganizationId = organization.Id;
        customerDto.RoleId = Guid.Empty;
        return await _customerAppService.UpdateAsync(customerId ,customerDto);
    }
    private async Task<CustomerDto> CreateCustomer(CreateUpdateCustomerIntegrationUnitDto dto)
    {
        var customerDto = dto.MapToCustomerDto(dto);
        customerDto.TenantId = CurrentTenant.Id;
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        customerDto.OrganizationId = organization.Id;
        customerDto.RoleId=Guid.Empty;
        var result = await _customerAppService.CreateMdkhlAsync(customerDto);
        result.OrganizationId = organization.Id;
        return result;
    }

    private async Task<List<VehicleDto>> CreateVehicle(List<CreateUpdateVehicleIntegrationUnitDto> dto, Guid customerId)
    {
        List<VehicleDto> vehicles = new List<VehicleDto>();
        var cars = await _vehicleAppService.GetVehiclesByCustomerIdAsync(customerId);
        foreach (var vehicle in dto)
        {
            if (cars.Any(x => x.EnPlateNumber == vehicle.PlateNumber))
            {
                vehicles.Add(cars.Where(x => x.EnPlateNumber == vehicle.PlateNumber).FirstOrDefault());
                continue;
            }
            CreateUpdateVehicleDto vehicleDto = await VehicleValidations(vehicle, customerId);
            var result = await _vehicleAppService.CreateAsync(vehicleDto);
            vehicles.Add(result);
        }
        return vehicles;
    }

    private async Task<CreateUpdateVehicleDto> VehicleValidations(CreateUpdateVehicleIntegrationUnitDto dto, Guid customerId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var modelId = await _vehicleLookupResolver.ResolveModelIdAsync(_options.Vehicle.ModelName);
            var brandId = await _vehicleLookupResolver.ResolveBrandIdAsync(_options.Vehicle.BrandName);
            var plateColorId = await _vehicleLookupResolver.ResolvePlateColorIdAsync(_options.Vehicle.PlateColorName);
            var licenceTypeId = await _vehicleLookupResolver.ResolveLicenceTypeIdAsync(_options.Vehicle.LicenceTypeName);
            var modelYear = _options.Vehicle.ModelYear;
            VehicleOptionsDto vehicleOptions = new VehicleOptionsDto
            {
                ModelId = modelId,
                BrandId = brandId,
                PlateColorId = plateColorId,
                LicenceTypeId = licenceTypeId,
                ModelYear = modelYear,
                CustomerId = customerId
            };
            var vehicleDto = dto.MapToVehicleDto(dto, vehicleOptions, CurrentUser.Id);
            return vehicleDto;
        }
    }

    private async Task<CustomerUnitPackageDto> CreateCustomerUnitPackage(CustomerDto customerDto, CreateUpdateBookingDto dto , Guid customerId) 
    {
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto = new CustomerUnitPackageAssignationDto
        {
            UnitCode = dto.UnitId,
            Discount = 0,
            Vat = dto.Price.Amount * 0.15m,
            DurationOfPackage = dto.Booking.NoOfDays,
            IsOwnerPackage = false,
            UnitPackageType = CustomerUnitPackageType.Renter
        };
        PackageDto packageDto = new PackageDto
        {
            EnglishName = ProNightStaticValues.EnglishName,
            ArabicName = ProNightStaticValues.ArabicName,
            Price = dto.Price.Amount,
            CustomerCategory = "Renter",
            Description = ProNightStaticValues.Description,
            OrganizationId = organization.Id,
            PackageCategoryType = ProNightStaticValues.PackageCategoryType,
            ActivationDate = dto.Booking.StartDate,
            DectivationDate = dto.Booking.EndDate,
            PublishFrom = ProNightStaticValues.PublishedFrom,
            PublishTo = ProNightStaticValues.PublishedTo,
            Status = ProNightStaticValues.status,
            TenantId = CurrentTenant.Id,
            PackageStatus = ProNightStaticValues.PackageStatus,
            Published = ProNightStaticValues.Published,
            DurationOfPackage = dto.Booking.NoOfDays,
            
        };
        UnitPackageDto unitPackage = new UnitPackageDto
        {
            Package = packageDto,
            NumberOfSlots = organization.OrganizationSettings.RentalSlots,
            NumberOfVehicles = organization.OrganizationSettings.RentalSlots
        };
        var result = await _customerUnitPackageAppService.AssignPackageToCustomerAsync(unitPackage, customerId, customerUnitPackageAssignationDto);
        if (!result.IsSuccess)
        {
            throw new Exception(string.Join(",", result.Errors));
        }
        return result.Value;
    }
    private async Task<UnitQrDto> CreateCustomerPackage(CustomerUnitPackageDto dto, List<VehicleDto> vehicles, string mobileNumber) 
    {
        CreateUpdateCustomerPackageForUnitDto createUpdateCustomerPackageForUnitDto = new CreateUpdateCustomerPackageForUnitDto
        {
            ActivationDate = DateOnly.FromDateTime(dto.ActivationDate),
            DectivationDateOfPackage = dto.DectivationDate,
            ActivationDateOfPackage = dto.ActivationDate,
            ExpiryDate = dto.DectivationDate,
            Discount = dto.Discount,
            PriceOfPackage = dto.Price,
            PackageEnglishName = dto.EnglishName,
            PackageArabicName = dto.ArabicName,
            CustomerId = dto.CustomerId,
            OrganizationId = dto.OrganizationId,
            TenantId = dto.TenantId,
            CustomerPackageStatus = dto.CustomerPackageStatus,
            CountOfDays = dto.DurationOfPackage,
            DurationOfPackage = dto.DurationOfPackage,
            PriceOfPackagePerDay = dto.Price / dto.DurationOfPackage,
            DescriptionOfPackage = "Renter package",
            CustomerCategory = CustomerCategoryStatus.Customer,
            PublishFromOfPackage = TimeOnly.FromTimeSpan(dto.ActivationDate.TimeOfDay),
            PublishToOfPackage = TimeOnly.FromTimeSpan(dto.DectivationDate.TimeOfDay),
            Vat = Math.Round((dto.Price - dto.Discount) * 0.15m, 2),
            PublishedOfPackage = true,
            CustomerMobile = mobileNumber,
            

        };
        List<CustomerPackageVehicleDto> vehiclesDto = new List<CustomerPackageVehicleDto>();
        foreach (var vehicle in vehicles)
        {
            vehiclesDto.Add(new CustomerPackageVehicleDto
            {
                ArPlateNumber = vehicle.ArPlateNumber,
                EnPlateNumber = vehicle.EnPlateNumber,
                LicenseTypeImage = vehicle.LicenceTypeImage,
                VehicleModelImage = vehicle.ModelImage
            });
        }
        createUpdateCustomerPackageForUnitDto.Vehicles = vehiclesDto;
        var result = await _customerPackageAppService.AssignUnitPackageToCustomerAsync(createUpdateCustomerPackageForUnitDto);

        return result;
    }
    private async Task<List<UnitVehiclePackageDto>> CreateUnitVehiclePackage(List<VehicleDto> vehicles, CustomerUnitPackageDto customerUnitPackage)
    {
        List<UnitVehiclePackageDto> unitVehiclePackages = new List<UnitVehiclePackageDto>();
        foreach (var vehicle in vehicles)
        {
            CreateUpdateUnitVehiclePackageDto dto = new CreateUpdateUnitVehiclePackageDto
            {
                CustomerId = vehicle.CustomerId,
                ArPlateNumber = vehicle.ArPlateNumber,
                EnPlateNumber = vehicle.EnPlateNumber,
                CustomerUnitPackageId = customerUnitPackage.Id,
                UnitCode = customerUnitPackage.UnitCode,
                CustomerUnitPackageType = CustomerUnitPackageType.Renter,
                ActivationDate = customerUnitPackage.ActivationDate,
                DectivationDate = customerUnitPackage.DectivationDate,
                PublishFrom = customerUnitPackage.PublishFrom,
                PublishTo = customerUnitPackage.PublishTo
            };
            
            var result = await _unitVehiclePackageAppService.CreateAsync(dto);
            unitVehiclePackages.Add(result);
        }
        
        return unitVehiclePackages;
    }

    private async Task<Result<bool>> ValidateCustomerUnitPackage(Guid customerId, CreateUpdateBookingDto dto, bool isUpdate) 
    {
        UnitPackageDto CustomerPackage = new UnitPackageDto
        {
            Package = new PackageDto
            {
                ActivationDate = dto.Booking.StartDate,
                DectivationDate = dto.Booking.EndDate,
            }
        };


        var result = await _customerUnitPackageAppService.MultipuleCustomerRentalValidation(customerId, CustomerPackage, dto.UnitId);
        if (result)
        {
            return Result<bool>.Error(@L["MultipleRentalError"]);
        }
        if (!isUpdate)
        {
            var result1 = await _customerUnitPackageAppService.IfCustomerHasMultiplePackageAsync(dto.UnitId, CustomerPackage);
            if (result1)
            {
                return Result<bool>.Error(@L["CustomerHasActivePackage"]);
            }
        }
        var result2 = await _customerUnitPackageAppService.CheckRenterSlotsOverUnit(CustomerPackage, dto.UnitId);
        if (result2)
        {
            return Result<bool>.Error(@L["RenterSlotsOverUnitValidation"]);
        }
        return Result<bool>.Success(true);
    }
}
