using Abp.Application.Services.Dto;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Organizations;
using SmartParking.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Payments;

public class PaymentResponseDto : FullAuditedEntityDto<Guid>
{

    public string PaymentCategory { get; set; }
    public string PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; }
    public string Currency { get; set; }
    public  CustomerDto Customers { get; set; }
}
