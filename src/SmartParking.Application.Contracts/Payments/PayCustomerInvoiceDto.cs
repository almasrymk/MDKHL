using SmartParking.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Payments;

public class PayCustomerInvoiceDto
{
    public int InvoiceId { get; set; }
    public PaymentMethod Method { get; set; }
}
