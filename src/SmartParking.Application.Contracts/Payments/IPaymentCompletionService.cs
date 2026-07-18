using SmartParking.Enums;
using System;
using System.Threading.Tasks;

namespace SmartParking.Payments;

public interface IPaymentCompletionService
{
    Task<Guid> CompletePaymentByCheckoutIdAsync(string checkoutId);
    Task<Guid> CompletePaymentByPaymentNumberAsync(string paymentNumber, string checkoutId, PaymentMethod paymentMethod);
}
