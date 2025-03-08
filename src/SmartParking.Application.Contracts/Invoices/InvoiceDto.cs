using System;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Invoices;

public class InvoiceDto : FullAuditedEntityDto<int>
{
    public string InvoiceNumber { get; set; }
    public int TotalHours { get; set; }
    public decimal InvoicePrice { get; set; }
    public string InvoiceType { get; set; }
    public string OrganizationArabicName { get; set; }
    public string OrganizationEnglishName { get; set; }
    public string GarageArabicName { get; set; }
    public string GarageEnglishName { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? InvoiceTime { get; set; }
    public DateTime? EntranceTime { get; set; }
    public string CustomerArName { get; set; }
    public string CustomerEnName { get; set; }
    public bool IsPackage { get; set; }
    public Guid? CustomerId { get; set; }
    public string EntryTransactionImage { get; set; }
    public string ExitTransactionImage { get; set; }
    public string QrCodeInvoice { get; set; }
    public int InvoiceId { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationAddress { get; set; }
    public string OrganizationTIN { get; set; }
    public int GarageTransactionExitId { get; set; }
    public decimal TotalPriceWithVatAndDiscount { get; set; }
    public decimal Discount { get; set; }







}
