using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Enums;
using SmartParking.IntegrationUnit;
using SmartParking.IntegrationUnit.Dtos;
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

    public IntegrationUnitAppService(ICustomerAppService customerAppService,
        IVehiclesAppService vehicleAppService,
        IOrganizationsAppService organizationsAppService,
        ICustomerUnitPackageAppService customerUnitPackageAppService,
        IAssignCustomerPackagesAppService customerPackageAppService,
        IUnitVehiclePackageAppService unitVehiclePackageAppService,
        IVehicleLookupResolver vehicleLookupResolver)
    {
        _customerAppService = customerAppService;
        _vehicleAppService = vehicleAppService;
        _organizationsAppService = organizationsAppService;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _customerPackageAppService = customerPackageAppService;
        _unitVehiclePackageAppService = unitVehiclePackageAppService;
        _vehicleLookupResolver = vehicleLookupResolver;
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto dto)
    {
        try
        {

            var customer = await CreateCustomer(dto.CustomerDto);
            var vehicle = await CreateVehicle(dto.VehicleDto, customer.Id);
            var customerUnitPackage = await CreateCustomerUnitPackage(customer,dto.IntegrationDetailsDto, customer.Id);
            var customerPackages = await CreateCustomerPackage(customerUnitPackage, vehicle);
            var unitVehiclePackages = await CreateUnitVehiclePackage(vehicle, customerUnitPackage);
            BookingDto result = new BookingDto
            {
                CustomerDto = customer,
                VehicleDto = vehicle,
                ActivationDate = dto.IntegrationDetailsDto.ReservationDate,
                DeactivationDate = dto.IntegrationDetailsDto.DueDate,
                Price = dto.IntegrationDetailsDto.Price,
                UnitCode = dto.IntegrationDetailsDto.UnitCode,
                Discount = dto.IntegrationDetailsDto.Discount
            };
            return Result.Success(result);
        }
        catch (Volo.Abp.BusinessException ex)
        {
            return Result.Error(ex.Code);
        }
        catch(InvalidOperationException ex)
        {
            return Result.Error(ex.Message);

        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> UpdateBookingAsync(CreateUpdateBookingDto dto)
    {
        var customer = await _customerAppService.UpdateAsync(dto.CustomerId!.Value, dto.CustomerDto.MapToCustomerDto(dto.CustomerDto));
        CreateUpdateVehicleDto vehicleDto = await VehicleValidations(dto.VehicleDto);
        var vehicle = await _vehicleAppService.UpdateAsync(dto.VehicleId!.Value, vehicleDto);
        BookingDto result = new BookingDto
        {
            CustomerDto = customer,
            VehicleDto = vehicle,
            ActivationDate = dto.IntegrationDetailsDto.ReservationDate,
            DeactivationDate = dto.IntegrationDetailsDto.DueDate,
            Price = dto.IntegrationDetailsDto.Price,
            UnitCode = dto.IntegrationDetailsDto.UnitCode,
            Discount = dto.IntegrationDetailsDto.Discount
        };
        return result;
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
        dto.SetCustomerId(customerId);
        CreateUpdateVehicleDto vehicleDto = await VehicleValidations(dto);
        return await _vehicleAppService.CreateAsync(vehicleDto);
    }

    private async Task<CreateUpdateVehicleDto> VehicleValidations(CreateUpdateVehicleIntegrationUnitDto dto)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {

            var modelId = await _vehicleLookupResolver.ResolveModelIdAsync(dto.ModelName);
            var brandId = await _vehicleLookupResolver.ResolveBrandIdAsync(dto.BrandName);
            var plateColorId = await _vehicleLookupResolver.ResolvePlateColorIdAsync(dto.PlateColorName);
            var licenceTypeId = await _vehicleLookupResolver.ResolveLicenceTypeIdAsync(dto.LicenceTypeName);
            var vehicleDto = dto.MapToVehicleDto(dto, modelId, brandId, plateColorId, licenceTypeId, CurrentUser.Id);
            return vehicleDto;
        }
    }

    private async Task<CustomerUnitPackageDto> CreateCustomerUnitPackage(CustomerDto customerDto, CreateUpdateBookingIntegrationDetailsDto IntegrationDetailsDto , Guid customerId) 
    {
        CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto = new CustomerUnitPackageAssignationDto
        {
            UnitCode = IntegrationDetailsDto.UnitCode,
            Discount = IntegrationDetailsDto.Discount,
            Vat = IntegrationDetailsDto.Price * 0.15m,
            DurationOfPackage = (int)(IntegrationDetailsDto.DueDate - IntegrationDetailsDto.ReservationDate).TotalDays,
            IsOwnerPackage = false
        };
        PackageDto packageDto = new PackageDto
        {
            EnglishName = OwnerPackageStaticValues.EnglishName,
            ArabicName = OwnerPackageStaticValues.ArabicName,
            Price = IntegrationDetailsDto.Price,
            CustomerCategory = "Renter",
            Description = OwnerPackageStaticValues.Description,
            OrganizationId = customerDto.OrganizationId,
            PackageCategoryType = OwnerPackageStaticValues.PackageCategoryType,
            ActivationDate = IntegrationDetailsDto.ReservationDate,
            DectivationDate = IntegrationDetailsDto.DueDate,
            PublishFrom = OwnerPackageStaticValues.PublishedFrom,
            PublishTo = OwnerPackageStaticValues.PublishedTo,
            Status = OwnerPackageStaticValues.status,
            TenantId = CurrentTenant.Id,
            PackageStatus = OwnerPackageStaticValues.PackageStatus,
            Published = OwnerPackageStaticValues.Published,
            DurationOfPackage = (int)(IntegrationDetailsDto.DueDate - IntegrationDetailsDto.ReservationDate).TotalDays,
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
