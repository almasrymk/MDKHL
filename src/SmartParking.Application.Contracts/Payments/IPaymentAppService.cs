using Abp.Authorization;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments;

public interface IPaymentAppService : ICrudAppService<PaymentDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePaymentDto>
{

    Task<PaymentCheckoutDto> CreateCheckoutPaymentAsync(PaymentCheckoutRequestDto input);

    Task<PaymentCheckoutDto?> CreateCheckoutInvoicePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest);
    Task<PaymentCheckoutDto?> CreateCheckoutPackagePaymentWebAsync(PaymentCheckoutRequestDto paymentCheckoutRequest);

    Task<Result<PaymentPackageDto>> GetPackagePaymentDetailsAsync(Guid PayemntId);
    Task<Result<InvoiceDto>> GetInvoicePaymentDetailsAsync(int OrganizationId, Guid CustomerId, Guid PayemntId);
    [AbpAllowAnonymous]
    Task<Result<PaymentCheckoutDto>> PayCustomerInvoiceAsync(PayCustomerInvoiceDto payCustomerInvoiceDto);

    Task<bool> ResendZatca(Guid Id);
    Task<string> GeneratePaymentNumberAsync(int organizationId);

    Task<Result<List<PaymentPackageDto>>> GetAllCustomerPackgePaymentsAsync(int organizationId);
    Task<PaymentCheckoutDto> SavePaymentRequest(PaymentCheckoutRequestDto paymentCheckoutRequest,
        PaymentStatus paymentStatus,
        Guid? customerId,
        string checkoutid = null,
        string entityId = null);
    Task<Guid> UpdatePaymentStausByCheckOutIdAsync(string checkoutid);
    Task<Guid> UpdatePaymentStausByPaymentNumberAsync(string paymentNumber, string checkoutid, PaymentMethod paymentMethod);

}
