using SmartParking.Enums;
using SmartParking.Results;
using System.Threading.Tasks;

namespace SmartParking.Payments;

public interface IPaymentInitializationService
{
    Task<Result<MobilePaymentSessionDto>> InitializeAsync(PaymentCategory category, int sourceId, PaymentMethod paymentMethod);
}
