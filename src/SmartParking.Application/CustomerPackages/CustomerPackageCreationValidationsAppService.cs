using Microsoft.EntityFrameworkCore;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Extensions;
using SmartParking.Localization;
using SmartParking.Subscriptions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace SmartParking.CustomerPackages;

[RemoteService(false)]
public class CustomerPackageCreationValidationsAppService : ApplicationService, ICustomerPackagesCreationValidationsAppService 
{
    private readonly IDataFilter _dataFilter;
    private readonly ISubscriptionActionsAppService _subscriptionActionsService;
    private readonly IRepository<CustomerPackage, int> _repository;
    private readonly IRepository<Customer, Guid> _customerRepository;


    public CustomerPackageCreationValidationsAppService(ISubscriptionActionsAppService subscriptionActionsService, IDataFilter dataFilter, IRepository<CustomerPackage, int> repository, IRepository<Customer, Guid> customerRepository)
    {
        _subscriptionActionsService = subscriptionActionsService;
        _dataFilter = dataFilter;
        _repository = repository;
        _customerRepository = customerRepository;
    }

    public async Task CheckSubscriptionCustomersLimitsAsync(int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            int numberOfCustomers = await GetNumberOfCustomerAtOrganizationAsync(organizationId);
            var subscription = await _subscriptionActionsService.CheckSubscriptionCustomersLimitsAsync(organizationId);
            if (subscription == null || subscription.NumberOfCustomers < numberOfCustomers + 1)
            {
                throw new Exception(LocalizationHelper.GetLocalizedString("UpgradeSubscriptionValidationUser"));
            }
        }
    }

    public async Task<int> GetNumberOfVehiclesPackagesInOrganizationForCustomer(
        Guid customerId,
        int organizationId)
    {
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var query = await _repository.GetQueryableAsync();

                return await query.Where(x=>x.CustomerId==customerId).CountOfVehicleHasActivePackage(organizationId , DateTime.Now).CountAsync();
        }
    }


    #region Private Methods
    /// <summary>
    /// Retrieves the number of customers associated with a specific organization.
    /// </summary>
    /// <param name="organizationId">The ID of the organization.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the number of customers in the organization.
    /// </returns>
    private async Task<int> GetNumberOfCustomerAtOrganizationAsync(int organizationId)
    {
        var query = await _customerRepository.GetQueryableAsync();
        query = query.Include(x => x.CustomerPackages)
                     .Where(x => x.CustomerPackages != null && x.CustomerPackages.Any(o => o.OrganizationId == organizationId));

        var count = await AsyncExecuter.CountAsync(query);
        return count;

    }

    #endregion
}
