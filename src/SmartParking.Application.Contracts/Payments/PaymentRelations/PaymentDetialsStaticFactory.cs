using SmartParking.CustomerUnitPackages;
using System;

namespace SmartParking.Payments.PaymentRelations;

public abstract class PaymentDetialsStaticFactory
{
    public static CreateUpdatePaymentDetailsDto CreateByCustomerUnitPackage(Guid PaymentId, CustomerUnitPackageDto dto)
    {
        return new CreateUpdatePaymentDetailsDto
        {
            ArabicName = dto.ArabicName,
            EnglishName = dto.EnglishName,
            Price = dto.Price,
            Discount = dto.Discount,
            Vat = dto.Vat,
            PayableId = dto.Id,
            PaymentId = PaymentId,
        };
    }
}
