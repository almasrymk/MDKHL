using SmartParking.CustomerPackages.Inputs;
using SmartParking.Enums;
using SmartParking.Packages;
using System;

namespace SmartParking.CustomerUnitPackages;

public class CustomerUnitPackageDto
{
    public int Id { get; set; }
    public int NumberOfVehicles { get; set; }
    public int NumberOfSlots { get; set; }
    public decimal Price { get; set; }

    public decimal Discount { get; set; }
    public decimal Vat { get; set; }
    public TimeOnly PublishFrom { get; set; }
    public TimeOnly PublishTo { get; set; }
    public DateTime ActivationDate { get; set; }
    public DateTime DectivationDate { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public int DurationOfPackage { get; set; }
    public int OrganizationId { get; set; }
    public Guid CustomerId { get; set; }
    public int UnitPackageId { get; set; }
    public  Guid TenantId { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }

    public CreateUpdateCustomerPackageDto ToCreateUpdateCustomerPackageDto(
        string enPlateNumber,
        string arPlateNumber

        )
    {
        if (string.IsNullOrWhiteSpace(enPlateNumber))
            throw new ArgumentException("English plate number is required.", nameof(enPlateNumber));

        if (string.IsNullOrWhiteSpace(arPlateNumber))
            throw new ArgumentException("Arabic plate number is required.", nameof(arPlateNumber));

        return new CreateUpdateCustomerPackageDto
        {
            // Core
            CustomerId = CustomerId,
            OrganizationId = OrganizationId,
            CustomerPackageStatus = CustomerPackageStatus,
            TenantId = TenantId,

            // Plate
            EnPlateNumber = enPlateNumber,
            ArPlateNumber = arPlateNumber,

            // Package-related values you CAN map from this DTO
            PriceOfPackage = Price,
            
            PublishedOfPackage = PublishFrom==PublishTo,               
            PublishFromOfPackage = PublishFrom,
            PublishToOfPackage = PublishTo,
            PackageArabicName = ArabicName,
            PackageEnglishName = EnglishName,

            // Dates
            ActivationDateOfPackage = ActivationDate,
            DectivationDateOfPackage = DectivationDate,

            // ExpiryDate is non-nullable in CreateUpdateCustomerPackageDto
            // If your domain says "deactivation = expiry", set it like this:
            ExpiryDate = DectivationDate,

            // Derived DTO-only fields if you want them consistent
            ExpiryDateOfPackage = DateOnly.FromDateTime(DectivationDate),
            ExpiryTimeOfPackage = TimeOnly.FromDateTime(DectivationDate),

            // Optional: if you want ActivationDate/Time (the DateOnly/TimeOnly pair)
            ActivationDate = DateOnly.FromDateTime(ActivationDate),
            ActivationTime = TimeOnly.FromDateTime(ActivationDate),

            // Defaults / not available from this DTO
            IsCustomPackage = true,
            Discount = Discount,
            Vat = Vat,
            


            // If you don’t have these values here, leave as null
            DurationOfPackage = DurationOfPackage,
            PriceOfPackagePerDay = Math.Round(Price / DurationOfPackage,2),
            CountOfDays = DurationOfPackage,
        };
    }

}
