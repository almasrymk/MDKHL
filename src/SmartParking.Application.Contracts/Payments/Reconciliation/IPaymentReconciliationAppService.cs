using SmartParking.Results;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartParking.Payments.Reconciliation;

public interface IPaymentReconciliationAppService : IApplicationService
{
    /// <summary>
    /// Processes non-success (failed / cancelled / declined / expired / stale) payment by cancelling the invoice,
    /// restoring the entry transaction for re-checkout, and deleting the payment record.
    /// Idempotent: safe to invoke multiple times even if payment or invoice is already deleted.
    /// </summary>
    Task<Result> ProcessNonSuccessPaymentAsync(Guid paymentId, int? invoiceId = null);

    /// <summary>
    /// Background job execution method to find all stale pending payments older than configured timeout
    /// and reconcile them.
    /// </summary>
    Task ReconcileStalePendingPaymentsAsync(CancellationToken cancellationToken = default);
}
