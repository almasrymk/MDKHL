using SmartParking.Enums;
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
    public string GarageName { get; set; }
    public string PlateNumber { get; set; }
    public DateTime? InvoiceTime { get; set; }
    public DateTime? EntranceTime { get; set; }
    public string CustomerArName { get; set; }
    public string CustomerEnName { get; set; }
    public bool IsPackage { get; set; }



}
