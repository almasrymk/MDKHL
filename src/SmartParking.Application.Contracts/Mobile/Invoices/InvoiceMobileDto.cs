
using SmartParking.Invoices;
using SmartParking.Vehicles;

namespace SmartParking.Mobile.Invoices;

public class InvoiceMobileDto : InvoiceDto
{
    public VehicleDto? Vehicle { get; set; }
    public decimal InvoicePriceWithDiscountAndVat { get; set; } = default;
}
