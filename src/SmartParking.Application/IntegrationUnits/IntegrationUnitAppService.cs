using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using SmartParking.IntegrationUnit.Options;
using SmartParking.Lookups.VehicleLookups;
using SmartParking.Mappers;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using SmartParking.UnitPackageIntegrations;
using SmartParking.UnitPackageIntegrations.Dtos;
using SmartParking.UnitPackages;
using SmartParking.Units;
using SmartParking.UnitVehiclePackages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private readonly IUnitPackageIntegrationOrchestratorAppService _unitPackageIntegrationOrchestratorAppService;
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
        IUnitAppService unitAppService,
        IUnitPackageIntegrationOrchestratorAppService unitPackageIntegrationOrchestratorAppService)
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
        _unitPackageIntegrationOrchestratorAppService = unitPackageIntegrationOrchestratorAppService;
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto dto)
    {
        var providerName = (dto is CreateUpdateBookingV2Dto v2Dto && !string.IsNullOrWhiteSpace(v2Dto.ProviderName))
            ? v2Dto.ProviderName
            : "Pronight Package";
        var providerArabicName = (dto is CreateUpdateBookingV2Dto v2DtoAr && !string.IsNullOrWhiteSpace(v2DtoAr.ProviderArabicName))
            ? v2DtoAr.ProviderArabicName
            : "باقه برونايت";
        return await CreateBookingInternalAsync(dto, providerName, providerArabicName, isExternal: false);
    }

    [RemoteService(false)]
    public async Task<Result<BookingDto>> CreateBookingV2Async(CreateUpdateBookingV2Dto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ProviderName))
        {
            return Result.Error("ProviderName is required.");
        }
        return await CreateBookingInternalAsync(dto, dto.ProviderName, dto.ProviderArabicName, isExternal: true);
    }

    private async Task<Result<BookingDto>> CreateBookingInternalAsync(CreateUpdateBookingDto dto, string providerName, string? providerArabicName, bool isExternal)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            try
            {
                var unit = await _unitAppService.GetActiveUnitByCodeAsync(dto.UnitId);
                if (!unit.IsSuccess)
                {
                    return Result.Error(@L["UnitNotFound"]);
                }
                if (dto.Customer?.Cars != null && dto.Customer.Cars.Count > 3)
                {
                    return Result.Error("Count of cars cannot exceed 3");
                }
                var existingBookings = await _unitPackageIntegrationOrchestratorAppService.BookingIdExist(dto.Booking.BookingId);
                if (existingBookings)
                {
                    return Result.Error(@L["BookingIdAlreadyExist"]);
                }
                var getCustomer = await _customerAppService.GetCustomerByPhoneNumberAsync(dto.Customer.Mobile);
                var customer = (getCustomer is null) ? await CreateCustomer(dto.Customer) : getCustomer;
                var vehicles = await CreateVehicle(dto.Customer.Cars, customer.Id);
                var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
                if (vehicles.Count > organization.OrganizationSettings.RentalSlots)
                {
                    return Result.Error("Count of vehicles bigger than number of slots");
                }
                var resultValidate = await ValidateCustomerUnitPackage(customer.Id, dto, false);
                if (!resultValidate.IsSuccess)
                {
                    return Result.Error(resultValidate.Errors.FirstOrDefault()!);
                }
                var customerUnitPackage = await CreateCustomerUnitPackage(customer, dto, customer.Id, unit, providerName, providerArabicName, isExternal);
                var customerPackages = await CreateCustomerPackage(customerUnitPackage, vehicles, customer.MobileNumber);
                var unitVehiclePackages = await CreateUnitVehiclePackage(vehicles, customerUnitPackage, customerPackages);
                var PackageResult = await HandlePackageResponseDto(customerPackages);
                var QrResult = await HandleQrResponseDto(customerPackages, customerUnitPackage);
                await CreateUnitPackageIntegrationsAsync(unitVehiclePackages, dto.Booking.BookingId, organization.Id, organization.TenantId);
                return Result.Success(new BookingDto
                {
                    Status = Status.Active,
                    PhoneNumber = customer.MobileNumber,

                    Package = PackageResult,
                    Validity = new ValidityDto
                    {
                        StartDate = customerUnitPackage.ActivationDate,
                        EndDate = customerUnitPackage.DectivationDate
                    },
                    Qr = QrResult,
                    Links = new LinksDto
                    {
                        bookingId = dto.Booking.BookingId,
                        UnitId = dto.UnitId
                    }
                });
            }
            catch (BusinessException ex)
            {
                if (_unitOfWorkManager.Current != null)
                {
                    await _unitOfWorkManager.Current.RollbackAsync();
                }
                return Result.Error(ex.Code);
            }
            catch (InvalidOperationException ex)
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
    }

    [RemoteService(false)]
    public async Task<Result<BookingDto>> UpdateBookingAsync(CreateUpdateBookingDto dto)
    {
        var providerName = (dto is CreateUpdateBookingV2Dto v2Dto && !string.IsNullOrWhiteSpace(v2Dto.ProviderName))
            ? v2Dto.ProviderName
            : "Pronight Package";
        var providerArabicName = (dto is CreateUpdateBookingV2Dto v2DtoAr && !string.IsNullOrWhiteSpace(v2DtoAr.ProviderArabicName))
            ? v2DtoAr.ProviderArabicName
            : "باقه برونايت";
        return await UpdateBookingInternalAsync(dto, providerName, providerArabicName, isExternal: false);
    }

    [RemoteService(false)]
    public async Task<Result<BookingDto>> UpdateBookingV2Async(CreateUpdateBookingV2Dto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ProviderName))
        {
            return Result.Error("ProviderName is required.");
        }
        return await UpdateBookingInternalAsync(dto, dto.ProviderName, dto.ProviderArabicName, isExternal: true);
    }

    private async Task<Result<BookingDto>> UpdateBookingInternalAsync(CreateUpdateBookingDto dto, string providerName, string? providerArabicName, bool isExternal)
    {
        try
        {
            var unit = await _unitAppService.GetActiveUnitByCodeAsync(dto.UnitId);
            if (!unit.IsSuccess)
            {
                return Result.Error(@L["UnitNotFound"]);
            }
            if (dto.Customer?.Cars != null && dto.Customer.Cars.Count > 3)
            {
                return Result.Error("Count of cars cannot exceed 3");
            }
            var customerDto = await _customerAppService.GetCustomerByPhoneNumberAsync(dto.CustomerId);
            var customer = await UpdateCustomer(customerDto.Id, dto.Customer.MapToCustomerDto(dto.Customer));
            var resultValidate = await ValidateCustomerUnitPackage(customer.Id, dto, true);
            if (!resultValidate.IsSuccess)
            {
                return Result.Error(resultValidate.Errors.FirstOrDefault()!);
            }
            var bookings = await _unitPackageIntegrationOrchestratorAppService.GetAllByBookingIdAsync(dto.Booking.BookingId);
            if (bookings.Count() == 0)
            {
                return Result.Error($"there is no bookings with such id {dto.Booking.BookingId}");
            }
            var firstUVP = await _unitVehiclePackageAppService.GetAsync(bookings.FirstOrDefault()!.UnitVehiclePackageId);
            var customerUnitPackage = await UpdateCustomerUnitPackage(firstUVP.CustomerUnitPackageId, dto, isExternal);
            foreach (var booking in bookings) 
            { 
                var unitVehiclePackage = await _unitVehiclePackageAppService.GetAsync(booking.UnitVehiclePackageId);
                await _customerPackagesAppService.DeleteAsync(unitVehiclePackage.CustomerPackgeId);
                await _unitPackageIntegrationOrchestratorAppService.DeleteAsync(booking.Id);
            }
            await _unitVehiclePackageAppService.DeleteByCustomerUnitPackage(customerUnitPackage.Id);

            var vehicles = await CreateVehicle(dto.Customer.Cars, customer.Id);

            var customerPackages = await CreateCustomerPackage(customerUnitPackage, vehicles, customer.MobileNumber);
            var unitVehiclePackages = await CreateUnitVehiclePackage(vehicles, customerUnitPackage, customerPackages);
            var PackageResult = await HandlePackageResponseDto(customerPackages);
            var QrResult = await HandleQrResponseDto(customerPackages, customerUnitPackage);

            var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
            await CreateUnitPackageIntegrationsAsync(unitVehiclePackages, dto.Booking.BookingId, organization.Id, organization.TenantId);

            await _unitOfWorkManager.Current!.SaveChangesAsync();

            BookingDto result = new BookingDto
            {
                Status = Status.Active,
                PhoneNumber = customer.MobileNumber,
                Package = PackageResult,
                Validity = new ValidityDto
                {
                    StartDate = customerUnitPackage.ActivationDate,
                    EndDate = customerUnitPackage.DectivationDate
                },
                Qr = QrResult,
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
            if (cars.Any(x => x.EnPlateNumber?.Trim().Equals(vehicle.PlateNumber?.Trim(), StringComparison.OrdinalIgnoreCase) == true))
            {
                vehicles.Add(cars.Where(x => x.EnPlateNumber?.Trim().Equals(vehicle.PlateNumber?.Trim(), StringComparison.OrdinalIgnoreCase) == true).FirstOrDefault());
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
            var licenceTypeId = await _vehicleLookupResolver.ResolveLicenceTypeIdAsync(dto.licenseType);
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

    private async Task<CustomerUnitPackageDto> CreateCustomerUnitPackage(CustomerDto customerDto, CreateUpdateBookingDto dto, Guid customerId, UnitDto unitDto, string providerName = "Pronight Package", string? providerArabicName = "باقه برونايت", bool isExternal = false) 
    {
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto = new CustomerUnitPackageAssignationDto
        {
            UnitCode = dto.UnitId,
            Discount = 0,
            Vat = dto.Price.Amount * 0.15m,
            DurationOfPackage = dto.Booking.NoOfDays,
            IsOwnerPackage = false,
            UnitPackageType = CustomerUnitPackageType.Renter,
            IsExternal = isExternal
        };
        PackageDto packageDto = new PackageDto
        {
            EnglishName = providerName,
            ArabicName = string.IsNullOrWhiteSpace(providerArabicName) ? providerName : providerArabicName,
            Price = dto.Price.Amount,
            CustomerCategory = "Renter",
            Description = "This package is for renters",
            OrganizationId = organization.Id,
            PackageCategoryType = PackageCategoryTypes.CustomPackage,
            ActivationDate = dto.Booking.StartDate,
            DectivationDate = dto.Booking.EndDate,
            PublishFrom = new TimeOnly(00, 00),
            PublishTo = new TimeOnly(00, 00),
            Status = Status.Active,
            TenantId = CurrentTenant.Id,
            PackageStatus = SubscriptionStatus.Active,
            Published = true,
            DurationOfPackage = dto.Booking.NoOfDays,
        };
        UnitPackageDto unitPackage = new UnitPackageDto
        {
            Package = packageDto,
            NumberOfSlots = !(organization.OrganizationSettings.IsSlotsEditable) ? organization.OrganizationSettings.RentalSlots : unitDto.RenterSlots,
            NumberOfVehicles = !(organization.OrganizationSettings.IsSlotsEditable) ? organization.OrganizationSettings.RentalSlots : unitDto.RenterSlots
        };
        var result = await _customerUnitPackageAppService.AssignPackageToCustomerAsync(unitPackage, customerId, customerUnitPackageAssignationDto);
        if (!result.IsSuccess)
        {
            throw new Exception(string.Join(",", result.Errors));
        }
        return result.Value;
    }
    private async Task<List<UnitQrDto>> CreateCustomerPackage(CustomerUnitPackageDto dto, List<VehicleDto> vehicles, string mobileNumber) 
    {
        

        CreateUpdateCustomerPackageDto createUpdateCustomerPackageDto  = new CreateUpdateCustomerPackageDto
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
            ActivationTime = TimeOnly.FromTimeSpan(dto.ActivationDate.TimeOfDay),
            ExpiryDateOfPackage = DateOnly.FromDateTime(dto.DectivationDate),
            ExpiryTimeOfPackage = TimeOnly.FromTimeSpan(dto.DectivationDate.TimeOfDay),
            IsCustomPackage = false,
        };
        List<UnitQrDto> qrCodeDtos = new List<UnitQrDto>();
        List<CustomerPackageVehicleDto> vehiclesDto = new List<CustomerPackageVehicleDto>();
        foreach (var vehicle in vehicles)
        {
            createUpdateCustomerPackageDto.ArPlateNumber = vehicle.ArPlateNumber;
            createUpdateCustomerPackageDto.EnPlateNumber = vehicle.EnPlateNumber;
            var res = await _customerPackageAppService.AssignPackageToCustomerAsync(createUpdateCustomerPackageDto);
            qrCodeDtos.Add(new UnitQrDto
            {
                PlateNumber = vehicle.EnPlateNumber,
                PackageCode = res.Value.PackageCode.ToString(),
                QrCodePath = res.Value.QRCodePath,
                CustomerPackageId = res.Value.Id
            });
        }

        return qrCodeDtos;
    }
    private async Task<List<UnitVehiclePackageDto>> CreateUnitVehiclePackage(List<VehicleDto> vehicles, CustomerUnitPackageDto customerUnitPackage, List<UnitQrDto> unitQrDtos)
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
                PublishTo = customerUnitPackage.PublishTo,
                PackageCode = unitQrDtos.Where(x => x.PlateNumber == vehicle.EnPlateNumber).FirstOrDefault()!.PackageCode,
                CustomerPackgeId = unitQrDtos.Where(x => x.PlateNumber == vehicle.EnPlateNumber).FirstOrDefault()!.CustomerPackageId
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


        var conflictingPackage = await _customerUnitPackageAppService.GetConflictingRentalPackageAsync(customerId, CustomerPackage, dto.UnitId);
        if (conflictingPackage != null)
        {
            var startDateStr = conflictingPackage.ActivationDate.ToString("d/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
            var endDateStr = conflictingPackage.DectivationDate.ToString("d/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture);
            return Result<bool>.Error(@L["UnitBookedDuringTime", startDateStr, endDateStr]);
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

    private async Task<List<PronightPackageResponseDto>> HandlePackageResponseDto(List<UnitQrDto> dtos)
    {
        List<PronightPackageResponseDto> result = new List<PronightPackageResponseDto>();
        foreach (var dto in dtos)
        {
            result.Add(new PronightPackageResponseDto
            {
                PlateNumber = dto.PlateNumber,
                PackageCode = dto.PackageCode,
            });
        }
        return result;
    }
    private async Task<List<QrDto>> HandleQrResponseDto(List<UnitQrDto> dtos, CustomerUnitPackageDto customerUnitPackage)
    {
        List<QrDto> result = new List<QrDto>();
        foreach (var dto in dtos)
        {
            result.Add(new QrDto
            {
                Required = true,
                QrCodeUrl = dto.QrCodePath,
                ExpiresAt = customerUnitPackage.DectivationDate
            });
        }
        return result;
    }

    private async Task<List<UnitPackageIntegrationDto>> CreateUnitPackageIntegrationsAsync(List<UnitVehiclePackageDto> unitVehiclePackages, string bookingId, int organizationId, Guid? tenantId)
    {
        List<UnitPackageIntegrationDto> unitPackageIntegrations = new List<UnitPackageIntegrationDto>();
        foreach (var item in unitVehiclePackages)
        {
            CreateUpdateUnitPackageIntegrationDto dto = new CreateUpdateUnitPackageIntegrationDto
            {
                BookingId = bookingId,
                CustomerId = item.CustomerId,
                UnitCode = item.UnitCode,
                UnitVehiclePackageId = item.Id,
                OrganizationId = organizationId,
                TenantId = tenantId
            };
            var result = await _unitPackageIntegrationOrchestratorAppService.CreateAsync(dto);
            unitPackageIntegrations.Add(result);
        }
        return unitPackageIntegrations;
    }

    private async Task<CustomerUnitPackageDto> UpdateCustomerUnitPackage(int id, CreateUpdateBookingDto bookingDto, bool isExternal)
    {
        var customerUnitPackage = await _customerUnitPackageAppService.GetAsync(id);
        var dto = customerUnitPackage.ToCreateUpdateDto();
        dto.ActivationDate = bookingDto.Booking.StartDate;
        dto.DectivationDate = bookingDto.Booking.EndDate;
        dto.UnitCode = bookingDto.UnitId;
        dto.DurationOfPackage = bookingDto.Booking.NoOfDays;
        dto.IsExternal = isExternal;
        var result = await _customerUnitPackageAppService.UpdateAsync(id, dto);
        return result;
    }
}
