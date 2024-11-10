using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments;

public class CreateUpdatePaymentDto 
{

    public Guid? CustomerId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentCategory? PaymentCategory { get; set; }
    public int PaymentCategoryId  { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string PaymentType { get; set; }
    public string EntityId { get; set; }
    public string CheckoutId { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

}
