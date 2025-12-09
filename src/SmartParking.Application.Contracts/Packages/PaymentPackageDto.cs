using System;

namespace SmartParking.Packages;

public class PaymentPackageDto
{ 

    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string Description { get; set; }
    public DateTime PaymentTime { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string CustomerEnglishName { get; set; }
    public string CustomerArabicName { get; set; }
    public string OrganizationAddress { get; set; }
    public string EnPlateNumber { get; set; }
    public string ArPlateNumber { get; set; }
    public DateOnly ExpiryDateOfPackage { get; set; }
    public TimeOnly ExpiryTimeOfPackage { get; set; }
    public DateOnly ActivationDate { get; set; }
    public TimeOnly ActivationTime { get; set; }
    public string? PackageCode { get; set; }
    public string? QRCodePath { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Vat { get; set; }


}
