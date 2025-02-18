using SmartParking.Cutomers;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments;

public class PaymentGetResponseDto : FullAuditedEntityDto<Guid>
{
    public OrganizationDto Organization { get; set; } = new OrganizationDto();
    public InvoiceDto Invoice { get; set; } = new InvoiceDto();
    public PackageDto Package { get; set; } = new PackageDto();
    public string PaymentCategory { get; set; }
    public string PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; }
    public string Currency { get; set; }
    public string PaymentNumber { get; set; }
    public CustomerDto Customers { get; set; } = new CustomerDto();
}
