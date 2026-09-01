using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace SmartParking.Payments.Reconciliation;

public class PaymentReconciliationWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PaymentReconciliationWorker> _logger;

    public PaymentReconciliationWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PaymentReconciliationWorker> logger,
        IOptions<PaymentReconciliationOptions> reconciliationOptions) : base(timer, serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;

        int intervalMinutes = reconciliationOptions.Value.ReconciliationIntervalMinutes;
        if (intervalMinutes <= 0)
        {
            _logger.LogWarning("[RECONCILIATION_WORKER] Invalid ReconciliationIntervalMinutes ({Value}). Falling back to default (10 mins).", intervalMinutes);
            intervalMinutes = 10;
        }

        Timer.Period = intervalMinutes * 60 * 1000;

        _logger.LogInformation("[RECONCILIATION_WORKER] Initialized with period {PeriodMs} ms ({Minutes} mins)", Timer.Period, intervalMinutes);
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        _logger.LogInformation("[RECONCILIATION_WORKER] DoWorkAsync triggered");

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            try
            {
                var reconciliationAppService = scope.ServiceProvider.GetRequiredService<IPaymentReconciliationAppService>();
                await reconciliationAppService.ReconcileStalePendingPaymentsAsync(workerContext.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RECONCILIATION_WORKER] Encountered unhandled exception in DoWorkAsync");
            }
        }
    }
}
