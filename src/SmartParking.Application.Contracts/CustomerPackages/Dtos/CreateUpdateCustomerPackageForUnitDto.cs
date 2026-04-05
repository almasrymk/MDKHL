using SmartParking.Enums;
using System;
using System.Collections.Generic;

namespace SmartParking.CustomerPackages.Dtos;

public class CreateUpdateCustomerPackageForUnitDto
{
    public Guid CustomerId { get; set; }
    public string CustomerMobile { get; set; }
    public int PackageId { get; set; }
    public int OrganizationId { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }
    public Guid? TenantId { get; set; }
    public List<CustomerPackageVehicleDto> Vehicles { get; set; } = new List<CustomerPackageVehicleDto>();

    // ========= Package-related values =========
    public int? DurationOfPackage { get; set; }
    public decimal? PriceOfPackage { get; set; }
    public decimal? PriceOfPackagePerDay { get; set; }
    public decimal? CountOfDays { get; set; }
    public string DescriptionOfPackage { get; set; }
    public bool? PublishedOfPackage { get; set; } // renamed to PascalCase

    public TimeOnly? PublishFromOfPackage { get; set; }
    public TimeOnly? PublishToOfPackage { get; set; }

    public bool IsCustomPackage { get; set; }
    public string PackageCode { get; set; }
    public string QRCodePath { get; set; }

    public string PackageArabicName { get; set; }
    public string PackageEnglishName { get; set; }

    public decimal Discount { get; set; }
    public decimal Vat { get; set; }

    // ========= All dates/times =========
    public DateTime? ActivationDateOfPackage { get; set; }
    public DateTime? DectivationDateOfPackage { get; set; } // keeping your spelling
    public DateTime ExpiryDate { get; set; } // non-null in source

    public DateOnly ExpiryDateOfPackage { get; set; }
    public TimeOnly ExpiryTimeOfPackage { get; set; }

    public DateOnly? ActivationDate { get; set; }
    public TimeOnly? ActivationTime { get; set; }
    public CustomerCategoryStatus CustomerCategory { get; set; }
}
