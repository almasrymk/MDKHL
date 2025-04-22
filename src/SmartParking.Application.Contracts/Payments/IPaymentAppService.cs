using SmartParking.Invoices;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments;

public interface IPaymentAppService : ICrudAppService<PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>
{

    Task<PaymentCheckoutDto> CreateCheckoutPaymentAsync(PaymentCheckoutRequestDto input);

    Task<PaymentCheckoutDto?> CreateCheckoutInvoicePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest);
    Task<PaymentDto> GetPaymentStatus(string input, string entityId,string ArabicPlateNumber , string EnglishPlateNumber);

    Task<Result<PayementPackageDto>> GetPackagePaymentDetailsAsync(int OrganizationId, Guid CustomerId, Guid PayemntId);
    Task<Result<InvoiceDto>> GetInvoicePaymentDetailsAsync(int OrganizationId, Guid CustomerId, Guid PayemntId);


}
