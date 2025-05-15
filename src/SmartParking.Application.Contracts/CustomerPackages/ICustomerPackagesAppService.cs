using SmartParking.Cameras;
using SmartParking.Packages;
using SmartParking.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.CustomerPackages
{
    public interface ICustomerPackagesAppService:
          ICrudAppService<
        CustomerPackageDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCustomerPackageDto
        >
    {
        /// <summary>
        /// Retrieves the active customer package for a specific organization and vehicle plate number, 
        /// based on the provided exit transaction date.
        /// </summary>
        /// <param name="orgID">The ID of the organization.</param>
        /// <param name="PlateNumber">The plate number of the vehicle.</param>
        /// <param name="ExitTransaction">The exit time of the garage transaction.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains the active <see cref="CustomerPackageDto"/> if one is found.
        /// </returns>
        Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrganizationIdAndPlateNumber(int orgID, string PlateNumber, DateTime ExitTransaction);
        /// <summary>
        /// Retrieves a list of all packages currently assigned to the authenticated customer.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="Result{T}"/> with a list of <see cref="CustomerPackageDto"/> assigned to the customer.
        /// </returns>
        Task<Result<List<CustomerPackageDto>>> GetPackageByCustomer();
        /// <summary>
        /// Assigns a package to a customer using the specified package ID and vehicle plate numbers.
        /// </summary>
        /// <param name="packageId">The ID of the package to assign.</param>
        /// <param name="ArabicPlateNumber">The Arabic version of the vehicle plate number.</param>
        /// <param name="EnglishPlateNumber">The English version of the vehicle plate number.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="Result"/> indicating the outcome of the assignment.
        /// </returns>
        Task<Result> AssignPackageToCustomerAsync(int packageId, string ArabicPlateNumber, string EnglishPlateNumber);


        /// <summary>
        /// Asynchronously assigns a package to a customer based on the provided package ID and customer ID.
        /// </summary>
        /// <param name="packageId">The ID of the package to be assigned.</param>
        /// <param name="customerId">The unique identifier (GUID) of the customer to whom the package will be assigned.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the package was successfully assigned; otherwise, <c>false</c>.</returns>
        Task<bool> AssignPackageToCustomerWebAsync(int? packageId, CustomerPackageDto customerPackage);

        /// <summary>
        /// Retrieves a list of packages associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer whose packages are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="PackageDto"/> objects associated with the customer.</returns>
        Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerMdkl(Guid customerId,PagedAndSortedResultRequestDto input);        
        /// <summary>
        /// Retrieves a list of packages associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer whose packages are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="PackageDto"/> objects associated with the customer.</returns>
        Task<PagedResultDto<CustomerPackageDto>> GetListPackagesByCustomerOrganization(Guid customerId, PagedAndSortedResultRequestDto input);

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
        Task<DateTime> GetExpireyDateForPackageSubscripedByCustomerInSpecificOrganization(int OrgId, Guid CustomerId, int PackageId);

        /// <summary>
        /// Checks if the number of subscribed customers has reached the limit for the given organization.
        /// Throws an exception if the limit has been exceeded.
        /// </summary>
        /// <param name="organizationId">The ID of the organization to check limits for.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CheckSubscriptionCustomersLimitsAsync(int organizationId);
    }
}
