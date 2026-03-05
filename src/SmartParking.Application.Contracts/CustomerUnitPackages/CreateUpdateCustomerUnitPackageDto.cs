using SmartParking.Enums;
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

}
