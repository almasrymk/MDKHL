using Humanizer;
using iText.Kernel.XMP;
using SmartParking.CustomerPackages.AssignantionsContarcts;
using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.CustomerUnitPackages;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Entities.Vehicles;
using SmartParking.Enums;
using SmartParking.IntegrationUnit;
using SmartParking.Mobile.Vehicles;
using SmartParking.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using SmartParking.UnitPackages;
using SmartParking.UnitVehiclePackages;
using SmartParking.Vehicles;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.MultiTenancy;


namespace SmartParking.IIntegrationUnit;

public class IntegrationUnitAppService : ApplicationService, IIntegrationUnitAppService
{
    private readonly ICustomerAppService _customerAppService;
    private readonly IVehiclesAppService _vehicleAppService;
    private readonly IUnitPackageAppService _unitPackageAppService;
    private readonly IOrganizationsAppService _organizationsAppService;
    private readonly ICustomerUnitPackageAppService _customerUnitPackageAppService;
    private readonly IAssignCustomerPackagesAppService _customerPackageAppService;
    private readonly IUnitVehiclePackageAppService _unitVehiclePackageAppService;

    public IntegrationUnitAppService(ICustomerAppService customerAppService,
        IVehiclesAppService vehicleAppService,
        IUnitPackageAppService unitPackageAppService,
        IOrganizationsAppService organizationsAppService,
        ICustomerUnitPackageAppService customerUnitPackageAppService,
        IAssignCustomerPackagesAppService customerPackageAppService,
        IUnitVehiclePackageAppService unitVehiclePackageAppService)
    {
        _customerAppService = customerAppService;
        _vehicleAppService = vehicleAppService;
        _unitPackageAppService = unitPackageAppService;
        _organizationsAppService = organizationsAppService;
        _customerUnitPackageAppService = customerUnitPackageAppService;
        _customerPackageAppService = customerPackageAppService;
        _unitVehiclePackageAppService = unitVehiclePackageAppService;
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> CreateBookingAsync(CreateUpdateBookingDto dto)
    {
        var customer = await CreateCustomer(dto.CustomerDto);
        var vehicle = await CreateVehicle(dto.VehicleDto, customer.Id);
        //var unitPackage = await CreateUnitPackage(dto,numberOfVehicles);
        var customerUnitPackage = await CreateCustomerUnitPackage(dto, customer.Id);
        var customerPackages = await CreateCustomerPackage(customerUnitPackage, vehicle);
        var unitVehiclePackages = await CreateUnitVehiclePackage(vehicle, customerUnitPackage);
        BookingDto result = new BookingDto
        {
            CustomerDto = customer,
            VehicleDto = vehicle,
            ActivationDate = dto.ActivationDate,
            DeactivationDate = dto.DeactivationDate,
            Price = dto.Price,
            UnitCode = dto.UnitCode,
            Discount = dto.Discount
        };
        return Result.Success(result);
    }
    [RemoteService(false)]
    public async Task<Result<BookingDto>> UpdateBookingAsync(CreateUpdateBookingDto dto)
    {
        var customer = await _customerAppService.UpdateAsync(dto.CustomerId!.Value, dto.CustomerDto);
        dto.VehicleDto.CustomerId = customer.Id;
        var vehicle = await _vehicleAppService.UpdateAsync(dto.VehicleId!.Value, dto.VehicleDto);
        BookingDto result = new BookingDto
        {
            CustomerDto = customer,
            VehicleDto = vehicle,
            ActivationDate = dto.ActivationDate,
            DeactivationDate = dto.DeactivationDate,
            Price = dto.Price,
            UnitCode = dto.UnitCode,
            Discount = dto.Discount
        };
        return result;
    }

    private async Task<CustomerDto> CreateCustomer(CreateUpdateCustomerDto dto)
    {
        dto.TenantId = CurrentTenant.Id;
        var organization = await _organizationsAppService.GetOrganizationForCurrentTenatAsync();
        dto.OrganizationId = organization.Id;
        return await _customerAppService.CreateMdkhlAsync(dto);
    }

    private async Task<VehicleDto> CreateVehicle(CreateUpdateVehicleDto dto, Guid customerId)
    {
        dto.CustomerId = customerId;
        return await _vehicleAppService.CreateAsync(dto);
    }
    private async Task<CustomerUnitPackageDto> CreateCustomerUnitPackage(CreateUpdateBookingDto bookingDto, Guid customerId) 
    {
        CustomerUnitPackageAssignationDto customerUnitPackageAssignationDto = new CustomerUnitPackageAssignationDto
        {
            UnitCode = bookingDto.UnitCode,
            Discount = bookingDto.Discount,
            Vat = bookingDto.Price * 0.15m,
            DurationOfPackage = (int)(bookingDto.DeactivationDate - bookingDto.ActivationDate).TotalDays,
            IsOwnerPackage = false
        };
        PackageDto packageDto = new PackageDto
        {
            EnglishName = OwnerPackageStaticValues.EnglishName,
            ArabicName = OwnerPackageStaticValues.ArabicName,
            Price = bookingDto.Price,
            CustomerCategory = "Renter",
            Description = OwnerPackageStaticValues.Description,
            OrganizationId = bookingDto.CustomerDto.OrganizationId,
            PackageCategoryType = OwnerPackageStaticValues.PackageCategoryType,
            ActivationDate = bookingDto.ActivationDate,
            DectivationDate = bookingDto.DeactivationDate,
            PublishFrom = OwnerPackageStaticValues.PublishedFrom,
            PublishTo = OwnerPackageStaticValues.PublishedTo,
            Status = OwnerPackageStaticValues.status,
            TenantId = CurrentTenant.Id,
            PackageStatus = OwnerPackageStaticValues.PackageStatus,
            Published = OwnerPackageStaticValues.Published,
            DurationOfPackage = (int)(bookingDto.DeactivationDate - bookingDto.ActivationDate).TotalDays,
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
