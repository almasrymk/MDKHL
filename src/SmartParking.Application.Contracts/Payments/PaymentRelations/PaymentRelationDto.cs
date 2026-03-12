using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments.PaymentRelations;

public class PaymentRelationDto : FullAuditedEntityDto<Guid>
{
    public Guid PaymentId { get; set; }
    public int PayableId { get; set; }
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
    public decimal Vat { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
}
