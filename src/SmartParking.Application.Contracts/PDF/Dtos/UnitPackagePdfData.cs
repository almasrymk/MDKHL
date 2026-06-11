using SmartParking.CustomerUnitPackages;
using SmartParking.Payments.PaymentRelations;

namespace SmartParking.PDF.Dtos;

public class UnitPackagePdfData : PdfData<CustomerUnitPackageDto>
{
    public new UnitPackageLocalizationData Localization { get; set; }
    public PaymentDetailsDto PaymentDetails { get; set; }
}
