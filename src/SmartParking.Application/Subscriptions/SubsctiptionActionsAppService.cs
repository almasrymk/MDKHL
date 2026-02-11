using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Subscriptions;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.Subscriptions;

public class SubsctiptionActionsAppService : ISubscriptionActionsAppService
{
    private readonly IDataFilter _dataFilter;
    private readonly IRepository<Subscription, int> _subscriptionRepository;


    public SubsctiptionActionsAppService(IDataFilter dataFilter, IRepository<Subscription, int> subscriptionRepository)
    {
        _dataFilter = dataFilter;
        _subscriptionRepository = subscriptionRepository;
    }



    public async Task<SubscriptionLimitationsDto> CheckSubscriptionCustomersLimitsAsync(int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _subscriptionRepository.GetQueryableAsync();

            var res= await query.Where(s => s.Organizations.Any(org => org.Id == organizationId))
            .Select(s => new SubscriptionLimitationsDto
            {
                NumberOfCustomers = s.NumberOfCustomers,
            })
            .SingleOrDefaultAsync();
            return res;
        }
    }
}
