using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmartParking.Entities.Invoices;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Enums.PosTerminal;
using SmartParking.Payments.PosTerminal;
using SmartParking.Utilities;
using SmartParking.Validations;
using Volo.Abp.Application.Services;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;

namespace SmartParking.Payments;

public class InvoicePaymentProcessorAppService : ApplicationService, IInvoicePaymentProcessorAppService
{
    private readonly IAbpDistributedLock _distributedLock;
    private readonly IRepository<Invoice, int> _invoiceRepository;
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IPaymentAppService _paymentAppService;
    private readonly ILogger<InvoicePaymentProcessorAppService> _logger;

    public InvoicePaymentProcessorAppService(
        IAbpDistributedLock distributedLock,
        IRepository<Invoice, int> invoiceRepository,
        IRepository<Payment, Guid> paymentRepository,
        IPaymentAppService paymentAppService,
        ILogger<InvoicePaymentProcessorAppService> logger)
    {
        _distributedLock = distributedLock;
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _paymentAppService = paymentAppService;
        _logger = logger;
    }

    public async Task<ProcessInvoicePaymentResultDto> InitiatePaymentAsync(ProcessInvoicePaymentRequestDto input)
    {
        var lockKey = $"SmartParking:InvoicePayment:{input.InvoiceId}";
        await using var handle = await _distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10));
        if (handle == null)
        {
            _logger.LogWarning("[InvoicePaymentProcessor] Could not acquire lock for invoice {InvoiceId}", input.InvoiceId);
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.PaymentInProgress,
                Message = "Payment operation is currently in progress for this invoice."
            };
        }

        var invoice = await _invoiceRepository.FirstOrDefaultAsync(i => i.Id == input.InvoiceId);
        if (invoice == null)
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Failed,
                Message = "Invoice not found."
            };
        }

        if (invoice.InvoiceType == InvoiceType.Paid)
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.AlreadyPaid,
                Message = "Invoice is already paid."
            };
        }

        var existingPayments = await _paymentRepository.GetListAsync(
            p => p.InvoiceId == input.InvoiceId &&
                 (p.PaymentStatus == PaymentStatus.Success || p.PaymentStatus == PaymentStatus.Pending)
        );

        if (existingPayments.Any(p => p.PaymentStatus == PaymentStatus.Success))
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.AlreadyPaid,
                Message = "A successful payment already exists for this invoice."
            };
        }

        var activePending = existingPayments.FirstOrDefault(p => p.PaymentStatus == PaymentStatus.Pending);
        if (activePending != null)
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.PaymentInProgress,
                PaymentId = activePending.Id,
                PaymentNumber = activePending.PaymentNumber,
                Message = "Payment is already in progress for this invoice."
            };
        }

        decimal discount = input.Discount ?? invoice.Discount;
        var validationError = NumbersValidations.ValidateInvoice(discount, invoice.InvoicePrice);
        if (!string.IsNullOrEmpty(validationError))
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Failed,
                Message = validationError
            };
        }

        if (input.Discount.HasValue && invoice.Discount != input.Discount.Value)
        {
            invoice.Discount = input.Discount.Value;
            await _invoiceRepository.UpdateAsync(invoice, autoSave: true);
        }

        var amount = PaymentUtilityCalculationts.GetPriceForPayment(invoice.InvoicePrice, discount);

        var checkoutRequest = new PaymentCheckoutRequestDto
        {
            OrganizationId = invoice.OrganizationId,
            PaymentCategory = PaymentCategory.PaymentParking,
            InvoiceId = invoice.Id,
            PaymentMethod = input.PaymentMethod,
            Amount = amount,
            CustomerId = invoice.CustomerId,
            PaymentName = input.PaymentMethod.ToString(),
            PaymentNumber = string.Empty,
            TenantId = invoice.TenantId ?? Guid.Empty
        };

        if (input.PaymentMethod == PaymentMethod.POS)
        {
            var pendingPayment = await _paymentAppService.CreatePendingPosPaymentAsync(checkoutRequest, input.TerminalId);
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Started,
                PaymentId = pendingPayment.PaymentId,
                PaymentNumber = pendingPayment.PaymentNumber,
                CheckoutData = pendingPayment
            };
        }
        else
        {
            var completedPayment = await _paymentAppService.CreateCheckoutInvoicePaymentWebAsync(checkoutRequest);
            if (completedPayment == null)
            {
                return new ProcessInvoicePaymentResultDto
                {
                    Result = InvoicePaymentResult.Failed,
                    Message = "Failed to create payment checkout."
                };
            }

            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Completed,
                PaymentId = completedPayment.PaymentId,
                PaymentNumber = completedPayment.PaymentNumber,
                CheckoutData = completedPayment
            };
        }
    }

    public async Task<ProcessInvoicePaymentResultDto> CompletePosPaymentAsync(CompletePosPaymentRequestDto input)
    {
        var lockKey = $"SmartParking:InvoicePayment:{input.InvoiceId}";
        await using var handle = await _distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(10));
        if (handle == null)
        {
            _logger.LogWarning("[InvoicePaymentProcessor] Could not acquire lock for POS completion on invoice {InvoiceId}", input.InvoiceId);
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.PaymentInProgress,
                PaymentId = input.PaymentId,
                Message = "Concurrent operation in progress for this invoice."
            };
        }

        var invoice = await _invoiceRepository.FirstOrDefaultAsync(i => i.Id == input.InvoiceId);
        var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.Id == input.PaymentId);

        var outcome = SamaResponseCodeMapping.Evaluate(input.SamaCode);

        // If invoice already Paid by another caller
        if (invoice != null && invoice.InvoiceType == InvoiceType.Paid)
        {
            // If terminal took money, compensate via existing refund
            if (outcome == SamaResponseOutcome.Approved && payment != null && payment.PaymentStatus != PaymentStatus.Success)
            {
                _logger.LogWarning("[InvoicePaymentProcessor] Invoice {InvoiceId} already Paid, but POS terminal approved PaymentId={PaymentId}. Triggering refund.",
                    input.InvoiceId, input.PaymentId);

                await _paymentAppService.RefundPosPaymentAsync(new CompletePosTerminalRefundDto
                {
                    PaymentId = input.PaymentId,
                    SamaCode = input.SamaCode ?? string.Empty,
                    RawJsonResult = input.RawJsonResult ?? string.Empty,
                    TerminalId = input.TerminalId,
                    TransactionStan = input.TransactionStan,
                    Rrn = input.Rrn,
                    AuthCode = input.AuthCode,
                    MaskedPan = input.MaskedPan,
                    CardScheme = input.CardScheme
                });

                return new ProcessInvoicePaymentResultDto
                {
                    Result = InvoicePaymentResult.AlreadyPaid,
                    PaymentId = input.PaymentId,
                    Message = "Invoice was already paid. External POS transaction has been refunded."
                };
            }

            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.AlreadyPaid,
                PaymentId = input.PaymentId,
                Message = "Invoice is already paid."
            };
        }

        if (payment == null)
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Failed,
                Message = "Payment record not found."
            };
        }

        if (payment.PaymentStatus == PaymentStatus.Success)
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Completed,
                PaymentId = payment.Id,
                PaymentNumber = payment.PaymentNumber
            };
        }

        var completeInput = new CompletePosTerminalPaymentDto
        {
            PaymentId = input.PaymentId,
            SamaCode = input.SamaCode,
            RawJsonResult = input.RawJsonResult,
            TerminalId = input.TerminalId,
            TransactionStan = input.TransactionStan,
            Rrn = input.Rrn,
            AuthCode = input.AuthCode,
            MaskedPan = input.MaskedPan,
            CardScheme = input.CardScheme
        };

        var completedResult = await _paymentAppService.CompletePosTerminalPaymentAsync(completeInput);

        if (completedResult.PaymentStatus == PaymentStatus.Success)
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Completed,
                PaymentId = completedResult.Id,
                PaymentNumber = completedResult.PaymentNumber
            };
        }
        else
        {
            return new ProcessInvoicePaymentResultDto
            {
                Result = InvoicePaymentResult.Failed,
                PaymentId = completedResult.Id,
                PaymentNumber = completedResult.PaymentNumber,
                Message = "POS transaction declined or cancelled."
            };
        }
    }
}
