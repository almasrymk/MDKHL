using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartParking.Entities.Organizations;
using SmartParking.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace SmartParking.Services.GarageTransactions;

public class GarageTransactionsBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
{
    private readonly IGarageTransactionWorkerRepository _service;
    private readonly ILogger<GarageTransactionsBackgroundWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;


    public GarageTransactionsBackgroundWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory, IGarageTransactionWorkerRepository service, ILogger<GarageTransactionsBackgroundWorker> logger) : base(timer, serviceScopeFactory)
    {
        _service = service;
        Timer.Period = 60 * 1000;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;

        _logger.LogInformation("Background job for garageTransaction Has been Called");

    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var dataFilter = scope.ServiceProvider.GetRequiredService<IDataFilter>();
            var organizationRepository = scope.ServiceProvider
                .GetRequiredService<IRepository<Organization, int>>();
            var service = scope.ServiceProvider
                .GetRequiredService<IGarageTransactionWorkerRepository>();

            using (dataFilter.Disable<IMultiTenant>())
            {
                var organizations = await organizationRepository.GetListAsync();
                _logger.LogInformation($"Found {organizations.Count} organizations");

                foreach (var organization in organizations)
                {
                    _logger.LogInformation("Processing organization: {Name}", organization.EnglishName);

                    if (organization.Graceperiod > 0)
                    {
                        try
                        {
                            await service.UpdateTransactionsAsync(organization.Graceperiod,organization.Id);
                            _logger.LogInformation("Successfully updated transactions for organization: {Name}",
                                organization.EnglishName);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error updating transactions for organization: {Name}",
                                organization.EnglishName);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Skipping organization {Name} - grace period is 0 or invalid",
                            organization.EnglishName);
                    }
                }
            }
        }

        _logger.LogInformation("Finished garage transactions update");
    }
}


