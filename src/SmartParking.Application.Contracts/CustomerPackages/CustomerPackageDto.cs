using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Enums;
using SmartParking.Organizations;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages;

public class CustomerPackageDto
{
    public int Id { get; set; }
    public Guid CustomerId { get; set; }
    public int PackageId { get; set; }
    public int OrganizationId { get; set; }
    public CustomerPackageStatus CustomerPackageStatus { get; set; }
    public  PackageDto Package { get; set; }
    public  OrganizationDto Organization { get; set; }
    public Guid? TenantId { get; set; }
    public int? DurationOfPackage { get; set; }
    public decimal? PriceOfPackage { get; set; }
    public string? DescriptionOfPackage { get; set; }
    public bool? publishedOfPackage { get; set; }
    public TimeOnly? PublishFromOfPackage { get; set; }
    public TimeOnly? PublishToOfPackage { get; set; }
    public DateTime? ActivationDateOfPackage { get; set; }
    public DateTime? DectivationDateOfPackage { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string PackageArabicName { get; set; }
    public string PackageEnglishName { get; set; }
    public CustomerCategoryStatus CustomerCategory { get; set; }

    public CustomerDto Customer { get; set; }
    public string CustomerArabicName { get; set; }
    public string CustomerEnglishName { get; set; }
    public string EnPlateNumber { get; set; }
    public string ArPlateNumber { get; set; }
    public DateOnly ExpiryDateOfPackage { get; set; }
    public TimeOnly ExpiryTimeOfPackage { get; set; }
    public DateOnly? ActivationDate { get; set; }
    public TimeOnly? ActivationTime { get; set; }
    public bool IsCustomPackage { get; set; }



}
