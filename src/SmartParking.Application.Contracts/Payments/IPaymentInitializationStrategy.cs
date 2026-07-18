using SmartParking.Enums;
using SmartParking.Results;
using System.Threading.Tasks;

namespace SmartParking.Payments;

public interface IPaymentInitializationStrategy
{
    PaymentCategory Category { get; }
    Task<Result<PaymentInitializationContext>> BuildAsync(int sourceId, PaymentMethod paymentMethod);
}
