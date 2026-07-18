using SmartParking.Entities.Payments;
using SmartParking.Enums;
using System.Threading.Tasks;

namespace SmartParking.Payments;

public interface IPaymentCompletionStrategy
{
    PaymentCategory Category { get; }
    Task CompleteAsync(Payment payment);
}
