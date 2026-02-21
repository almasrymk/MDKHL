using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages;

public interface ICustomerPackagesCreationValidationsAppService
{
    /// <summary>
    /// Checks if the number of subscribed customers has reached the limit for the given organization.
    /// Throws an exception if the limit has been exceeded.
    /// </summary>
    /// <param name="organizationId">The ID of the organization to check limits for.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CheckSubscriptionCustomersLimitsAsync(int organizationId);


    Task<int> GetNumberOfVehiclesPackagesInOrganizationForCustomer(Guid customerId , int organizationId);
}
