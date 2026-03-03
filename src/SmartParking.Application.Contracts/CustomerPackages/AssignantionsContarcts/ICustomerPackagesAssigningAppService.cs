using SmartParking.CustomerPackages.Inputs;
using SmartParking.Cutomers;
using SmartParking.Entities.Customers;
using SmartParking.Entities.Organizations;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartParking.CustomerPackages.AssignantionsContarcts;

public interface ICustomerPackagesAssigningAppService
{


    /// <summary>
    /// Gets the expiry date for a specific customer's subscription to a package within a given organization.
    /// </summary>
    /// <param name="OrgId">The ID of the organization.</param>
    /// <param name="CustomerId">The ID of the customer.</param>
    /// <param name="PackageId">The ID of the package.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the <see cref="DateTime"/> of the package expiry.
    /// </returns>
    //Task<DateTime> GetExpireyDateForPackageSubscripedByCustomerInSpecificOrganization(int OrgId, Guid CustomerId, int PackageId);





    /// <summary>
    /// Retrieves a package entity by its ID.
    /// </summary>
    /// <param name="packageId">The ID of the package to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the <see cref="Package"/> entity if found; otherwise, null.
    /// </returns>
    Task<PackageDto> GetPackageAsync(int packageId);
    Task<string> HandleQRCode(CreateUpdateCustomerPackageDto customerPackageDto, CustomerDto customer, CustomerPackage customerPackage);
}
