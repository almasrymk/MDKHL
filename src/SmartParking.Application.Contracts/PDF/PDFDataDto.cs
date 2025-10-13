using SmartParking.Organizations;
using SmartParking.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.PDF;

public class PdfData<T>
{
    public T Data { get; set; }
    public PaymentGetResponseDto Payment { get; set; }
    public OrganizationDto Organization { get; set; }
    public InvoiceLocalizationData Localization { get; set; }
}