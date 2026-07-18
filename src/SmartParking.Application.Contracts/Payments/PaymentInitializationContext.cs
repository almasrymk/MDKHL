using SmartParking.Enums;

namespace SmartParking.Payments;

public class PaymentInitializationContext
{
    public decimal Amount { get; set; }
    public PaymentCheckoutRequestDto CheckoutRequest { get; set; } = null!;
}
