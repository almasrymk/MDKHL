using SmartParking.Enums;
using SmartParking.Payments;
using System;

namespace SmartParking.CustomerPackages.Dtos;

public class CustomerPackageMobileDto
{
    public int Id { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }
    public int? DurationOfPackage { get; set; }
    public decimal? PriceOfPackage { get; set; }
    public decimal? PriceOfPackagePerDay { get; set; }
    public decimal? CountOfDays { get; set; }
    public bool? PublishedOfPackage { get; set; }

    public string PackageArabicName { get; set; }
    public string PackageEnglishName { get; set; }

    public CustomerCategoryStatus CustomerCategory { get; set; }

    public string CustomerArabicName { get; set; }
    public string CustomerEnglishName { get; set; }

    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }

    public string EnPlateNumber { get; set; }
    public string ArPlateNumber { get; set; }

    public DateOnly ExpiryDateOfPackage { get; set; }
    public TimeOnly ExpiryTimeOfPackage { get; set; }

    public DateOnly? ActivationDate { get; set; }
    public TimeOnly? ActivationTime { get; set; }

    public bool IsCustomPackage { get; set; }

    public string PackageCode { get; set; }
    public string QRCodePath { get; set; }

    public decimal Discount { get; set; }
    public decimal Vat { get; set; }

    public string LicenseTypeImage { get; set; }
    public string VehicleModelImage { get; set; }
}