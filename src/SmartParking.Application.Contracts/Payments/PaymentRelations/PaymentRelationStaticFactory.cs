using SmartParking.CustomerUnitPackages;
using System;

namespace SmartParking.Payments.PaymentRelations;

public abstract class PaymentRelationStaticFactory
{
    public static CreateUpdatePaymentRelationDto CreateByCustomerUnitPackage(Guid PaymentId, CustomerUnitPackageDto dto)
    {
        return new CreateUpdatePaymentRelationDto
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
