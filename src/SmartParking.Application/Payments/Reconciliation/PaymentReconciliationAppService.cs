using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Payments;
using SmartParking.Enums;
using SmartParking.Invoices;
using SmartParking.Payments.PaymentIntegrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Payments.Reconciliation;

public class PaymentReconciliationAppService : SmartParkingAppService, IPaymentReconciliationAppService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IPublicInvoiceAppService _publicInvoiceAppService;
    private readonly IPaymentAppService _paymentAppService;
    private readonly IPaymentCompletionService _paymentCompletionService;
    private readonly IDataFilter _dataFilter;
    private readonly IOptions<PaymentReconciliationOptions> _reconciliationOptions;

    public PaymentReconciliationAppService(
        IRepository<Payment, Guid> paymentRepository,
        IPublicInvoiceAppService publicInvoiceAppService,
        IPaymentAppService paymentAppService,
        IPaymentCompletionService paymentCompletionService,
        IDataFilter dataFilter,
        IOptions<PaymentReconciliationOptions> reconciliationOptions)
    {
        _paymentRepository = paymentRepository;
        _publicInvoiceAppService = publicInvoiceAppService;
        _paymentAppService = paymentAppService;
        _paymentCompletionService = paymentCompletionService;
        _dataFilter = dataFilter;
        _reconciliationOptions = reconciliationOptions;
    }

    public async Task ProcessNonSuccessPaymentAsync(Guid paymentId, int? invoiceId = null)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            Payment? payment = null;
            if (paymentId != Guid.Empty)
            {
                try
                {
                    payment = await _paymentRepository.FirstOrDefaultAsync(p => p.Id == paymentId);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "[RECONCILIATION] Failed to fetch payment {PaymentId}", paymentId);
                }
            }

            int targetInvoiceId = (invoiceId.HasValue && invoiceId.Value > 0)
                ? invoiceId.Value
                : (payment?.InvoiceId ?? 0);
            // 2. Delete payment if record still exists
            if (paymentId != Guid.Empty)
            {
                try
                {
                    var existingPayment = await _paymentRepository.FirstOrDefaultAsync(p => p.Id == paymentId);
                    if (existingPayment != null)
                    {
                        await _paymentRepository.DeleteAsync(existingPayment.Id);
                        Logger.LogInformation("[RECONCILIATION] Successfully deleted payment {PaymentId}", paymentId);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "[RECONCILIATION] Payment {PaymentId} deletion skipped/failed (may already be deleted)", paymentId);
                }
            }
            // 1. Cancel invoice (unmaps entry transaction for re-checkout, deletes exit transaction & invoice)
            if (targetInvoiceId > 0)
            {
                try
                {
                    await _publicInvoiceAppService.CancelInvoiceAsync(targetInvoiceId);
                    Logger.LogInformation("[RECONCILIATION] Successfully cancelled invoice {InvoiceId}", targetInvoiceId);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "[RECONCILIATION] Invoice {InvoiceId} cancellation skipped/failed (may already be deleted)", targetInvoiceId);
                }
            }

        
        }
    }

    public async Task ReconcileStalePendingPaymentsAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.Now;

        int timeoutMinutes = _reconciliationOptions.Value.PendingTimeoutMinutes;
        if (timeoutMinutes <= 0)
        {
            Logger.LogWarning("[RECONCILIATION] PendingTimeoutMinutes in payment configuration is invalid ({Value}). Falling back to default (10 mins).", timeoutMinutes);
            timeoutMinutes = 10;
        }

        var cutoffTime = DateTime.Now.AddMinutes(-timeoutMinutes);

        Logger.LogInformation("[RECONCILIATION] Job started. Finding pending payments created before {CutoffTime} (timeout: {TimeoutMinutes} mins)", cutoffTime, timeoutMinutes);

        List<Payment> stalePayments;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _paymentRepository.GetQueryableAsync();
            stalePayments = await query
                .Where(p => p.PaymentStatus == PaymentStatus.Pending && p.CreationTime <= cutoffTime)
                .ToListAsync();
        }

        Logger.LogInformation("[RECONCILIATION] Found {Count} stale pending payments to reconcile", stalePayments.Count);

        int reconciledCount = 0;
        int errorCount = 0;

        foreach (var payment in stalePayments)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                Logger.LogInformation(
                    "[RECONCILIATION] Reconciling PaymentId={PaymentId}, PaymentNumber={PaymentNumber}, InvoiceId={InvoiceId}, CreationTime={CreationTime}",
                    payment.Id, payment.PaymentNumber, payment.InvoiceId, payment.CreationTime);

                await ProcessNonSuccessPaymentAsync(payment.Id, payment.InvoiceId);
                reconciledCount++;

                Logger.LogInformation("[RECONCILIATION] Successfully reconciled stale payment PaymentId={PaymentId}", payment.Id);
            }
            catch (Exception ex)
            {
                errorCount++;
                Logger.LogError(ex, "[RECONCILIATION] Error reconciling payment PaymentId={PaymentId}", payment.Id);
            }
        }

        var duration = DateTime.Now - startTime;
        Logger.LogInformation(
            "[RECONCILIATION] Job finished. Processed={Total}, Reconciled={Reconciled}, Errors={Errors}, Duration={DurationMs}ms",
            stalePayments.Count, reconciledCount, errorCount, duration.TotalMilliseconds);
    }
}
