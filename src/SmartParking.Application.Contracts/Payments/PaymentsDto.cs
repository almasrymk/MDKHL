using SmartParking.Enums;
using SmartParking.Organizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Payments;

public class PaymentDto 
{

    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string  CheckoutId { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string QrCodeInvoice { get; set; }


}
