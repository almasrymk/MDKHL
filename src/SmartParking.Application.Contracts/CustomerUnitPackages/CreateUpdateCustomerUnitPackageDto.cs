using SmartParking.Cutomers;
using SmartParking.Enums;
using SmartParking.UnitPackages;
using SmartParking.Validations;
using System;

namespace SmartParking.CustomerUnitPackages;

public class CreateUpdateCustomerUnitPackageDto
{
    public int NumberOfVehicles { get; set; }
    public int NumberOfSlots { get; set; }
    public decimal Price { get; set; }
    public TimeOnly PublishFrom { get; set; }
    public TimeOnly PublishTo { get; set; }
    public DateTime ActivationDate { get; set; }
    public DateTime DectivationDate { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public Guid CustomerId { get; set; }
    public int UnitPackageId { get; set; }
    public Status Status { get; set; }
    public decimal Discount { get; set; }
    public int DurationOfPackage { get; set; }
    public decimal Vat { get; set; }
    public int OrganizationId { get; set; }
    public CustomerCategoryStatus CustomerCategory { get; set; }
    public string UnitCode { get; set; }

    public static CreateUpdateCustomerUnitPackageDto CreateFrom(
    CustomerDto customer,
    UnitPackageDto package,
    CustomerUnitPackageAssignationDto assignDto)
    {
        var p = package.Package;
        var isCustom = p.PackageCategoryType == PackageCategoryTypes.CustomPackage;

        Enum.TryParse(p.CustomerCategory, true, out CustomerCategoryStatus category);

        return new CreateUpdateCustomerUnitPackageDto
        {
            // Basic Info
            ArabicName = p.ArabicName,
            EnglishName = p.EnglishName,
            Status = p.Status,
            Price = p.Price,
            OrganizationId = p.OrganizationId,
            PublishFrom = p.PublishFrom,
            PublishTo = p.PublishTo,

            // Calculated Dates
            ActivationDate = PackageAssignationLogic.PackageActivationTime(p.ActivationDate, p.PublishFrom, isCustom),
            DectivationDate = PackageAssignationLogic.PackageExpirationTime(p.DectivationDate, p.DurationOfPackage, isCustom),

            // Package Relations
            UnitPackageId = package.Id,
            NumberOfSlots = package.NumberOfSlots,
            NumberOfVehicles = package.NumberOfVehicles,

            // Customer & Assignation Info
            CustomerId = customer.Id,
            CustomerCategory = category,
            DurationOfPackage = assignDto.DurationOfPackage,
            Vat = assignDto.Vat,
            Discount = assignDto.Discount,
            UnitCode = assignDto.UnitCode
        };
    }

}
