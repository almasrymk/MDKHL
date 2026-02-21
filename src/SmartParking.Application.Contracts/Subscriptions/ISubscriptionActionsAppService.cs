using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.Subscriptions;

public interface ISubscriptionActionsAppService
{
    /// <summary>
    /// method returns number of customer subsriptions if the organization has a subscription, otherwise returns 0.
    /// </summary>
    /// <param name="organizationId">The ID of the organization to check limits for.</param>
    /// <returns name"SubscriptionLimitationsDto">A subscription Limitation.</returns>
    Task<SubscriptionLimitationsDto> CheckSubscriptionCustomersLimitsAsync(int organizationId);
}
