using SmartParking.Invoices;
using SmartParking.Results;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments;

public interface IPaymentMobileHelper : IApplicationService
{
    public Task<Result> HandleCancel(int invoiceId);
}
