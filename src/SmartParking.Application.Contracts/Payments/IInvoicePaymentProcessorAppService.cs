using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments;

public interface IInvoicePaymentProcessorAppService : IApplicationService
{
    Task<ProcessInvoicePaymentResultDto> InitiatePaymentAsync(ProcessInvoicePaymentRequestDto input);
    Task<ProcessInvoicePaymentResultDto> CompletePosPaymentAsync(CompletePosPaymentRequestDto input);
}
