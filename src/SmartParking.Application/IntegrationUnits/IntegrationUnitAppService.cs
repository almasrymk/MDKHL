using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SmartParking.CustomerPackages;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Entities.CustomerUnitPackages;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
using SmartParking.IntegrationUnit.Options;
using SmartParking.Lookups.VehicleLookups;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using SmartParking.UnitPackages;
using SmartParking.UnitVehiclePackages;
using SmartParking.Vehicles;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;


namespace SmartParking.IIntegrationUnit;

public class IntegrationUnitAppService : ApplicationService, IIntegrationUnitAppService
{
    private readonly ICustomerAppService _customerAppService;
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
        IConfiguration configuration)
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
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto dto)
    {
        try
        {

            var customer = await CreateCustomer(dto.Customer);
            var vehicle = await CreateVehicle(dto.Cars, customer.Id);
            var customerUnitPackage = await CreateCustomerUnitPackage(customer,dto, customer.Id);
            var customerPackages = await CreateCustomerPackage(customerUnitPackage, vehicle);
            var unitVehiclePackages = await CreateUnitVehiclePackage(vehicle, customerUnitPackage);
            BookingDto result = new BookingDto
            {
                Status = Status.Active,
                CustomerId = customer.Id,
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
                    QrCodeUrl = $"{_configuration["App:SelfUrl"]?.TrimEnd('/')}/{customerPackages.QRCodePath?.TrimStart('/')}",
                    ExpiresAt = customerUnitPackage.DectivationDate
                },
                Links = new LinksDto
                {
                    bookingId = dto.Booking.BookingId,
                    UnitId = dto.UnitCode
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
            var customer = await UpdateCustomer(dto.CustomerId!.Value, dto.Customer.MapToCustomerDto(dto.Customer));
            CreateUpdateVehicleDto vehicleDto = await VehicleValidations(dto.Cars, customer.Id);
            vehicleDto.CustomerId = customer.Id;
            var vehicle = await _vehicleAppService.UpdateAsync(dto.VehicleId!.Value, vehicleDto);
            var customerPackages = await _customerPackagesAppService.GetLatestByCustomerIdAndEnPlateNumber(new SearchByPlateAndCustomerIdDto { CustomerId = customer.Id , EnPlateNumber = vehicle.EnPlateNumber});
            BookingDto result = new BookingDto
            {
                Status = Status.Active,
                CustomerId = customer.Id,
                PackageId = customerPackages.PackageCode.ToString(),
                PackageNumber = customerPackages.PackageCode.ToString(),
                Validity = new ValidityDto
                {
                    StartDate = dto.Booking.StartDate,
                    EndDate = dto.Booking.EndDate
                },

                Links = new LinksDto
                {
                    bookingId = dto.Booking.BookingId,
                    UnitId = dto.UnitCode
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

    private async Task<VehicleDto> CreateVehicle(CreateUpdateVehicleIntegrationUnitDto dto, Guid customerId)
    {
        CreateUpdateVehicleDto vehicleDto = await VehicleValidations(dto, customerId);
        return await _vehicleAppService.CreateAsync(vehicleDto);
    }

    private async Task<CreateUpdateVehicleDto> VehicleValidations(CreateUpdateVehicleIntegrationUnitDto dto, Guid customerId)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var x = _options.Vehicle.ModelName;
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
        CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto = new CustomerUnitPackageAssignationDto
        {
            UnitCode = dto.UnitCode,
            Discount = 0,
            Vat = dto.Price.Amount * 0.15m,
            DurationOfPackage = dto.Booking.NoOfDays,
            IsOwnerPackage = false
        };
        PackageDto packageDto = new PackageDto
        {
            EnglishName = OwnerPackageStaticValues.EnglishName,
            ArabicName = OwnerPackageStaticValues.ArabicName,
            Price = dto.Price.Amount,
            CustomerCategory = "Renter",
            Description = OwnerPackageStaticValues.Description,
            OrganizationId = customerDto.OrganizationId,
            PackageCategoryType = OwnerPackageStaticValues.PackageCategoryType,
            ActivationDate = dto.Booking.StartDate,
            DectivationDate = dto.Booking.EndDate,
            PublishFrom = OwnerPackageStaticValues.PublishedFrom,
            PublishTo = OwnerPackageStaticValues.PublishedTo,
            Status = OwnerPackageStaticValues.status,
            TenantId = CurrentTenant.Id,
            PackageStatus = OwnerPackageStaticValues.PackageStatus,
            Published = OwnerPackageStaticValues.Published,
            DurationOfPackage = dto.Booking.NoOfDays,
        };
        UnitPackageDto unitPackage = new UnitPackageDto
        {
            Package = packageDto,
            NumberOfSlots = 1,
            NumberOfVehicles = 1
        };
        var result = await _customerUnitPackageAppService.AssignPackageToCustomerAsync(unitPackage, customerId, customerUnitPackageAssignationDto);
        if (!result.IsSuccess)
        {
            throw new Exception(string.Join(",", result.Errors));
        }
        return result.Value;
    }
    private async Task<CustomerPackageDto> CreateCustomerPackage(CustomerUnitPackageDto dto, VehicleDto vehicle) 
    {
        CreateUpdateCustomerPackageDto packageDto = new CreateUpdateCustomerPackageDto
        {
            ActivationDate = DateOnly.FromDateTime(dto.ActivationDate),
            DectivationDateOfPackage = dto.DectivationDate,
            ActivationDateOfPackage = dto.ActivationDate,
            ExpiryDate = dto.DectivationDate,
            ArPlateNumber = vehicle.ArPlateNumber,
            EnPlateNumber = vehicle.EnPlateNumber,
            Discount = dto.Discount,
            PriceOfPackage = dto.Price,
            Vat = dto.Vat,
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
            CustomerCategory = CustomerCategoryStatus.None

        };

        var result = await _customerPackageAppService.AssignPackageToCustomerAsync(packageDto);
        if (!result.IsSuccess)
        {
            throw new Exception(string.Join(",", result.Errors));

        }
        return result;
    }
    private async Task<UnitVehiclePackageDto> CreateUnitVehiclePackage(VehicleDto vehicle, CustomerUnitPackageDto customerUnitPackage)
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
        
        return result;
    }
}
