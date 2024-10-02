using SmartParking.Enums;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Invoices;

public class InvoiceDto : FullAuditedEntityDto<int>
{
    public string InvoiceId { get; set; }
    public int TotalHours { get; set; }
    public decimal InvoicePrice { get; set; }
    public InvoiceType Invoicetype { get; set; }
    public string OrganizationArabicName { get; set; }
    public string GarageName { get; set; }
    public string PlateNumber { get; set; }
    public string PackageArabicName { get; set; }
}
