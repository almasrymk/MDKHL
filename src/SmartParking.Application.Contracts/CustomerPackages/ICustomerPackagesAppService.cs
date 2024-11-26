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
        Task<CustomerPackageDto> GetOneActiveCustomerPackageByOrgIdAndCustomerId(int orgID, Guid customerID);
        Task<Result<List<PackageDto>>> GetPackageByCustomer();
        Task<Result> AssignPackageToCustomerAsync(int packageId);

		/// <summary>
		/// Asynchronously assigns a package to a customer based on the provided package ID and customer ID.
		/// </summary>
		/// <param name="packageId">The ID of the package to be assigned.</param>
		/// <param name="customerId">The unique identifier (GUID) of the customer to whom the package will be assigned.</param>
		/// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the package was successfully assigned; otherwise, <c>false</c>.</returns>
		Task<bool> AssignPackageToCustomerWebAsync(int packageId, Guid customerId);

        /// <summary>
        /// Retrieves a list of packages associated with a specific customer.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer whose packages are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="PackageDto"/> objects associated with the customer.</returns>
        Task<List<PackageDto>> GetListPackagesByCustomer(Guid customerId);

        Task<DateTime> GetExpireyDateForPackageSubscripedByCustomerInSpecificOrganization(int OrgId,Guid CustomerId , int PackageId);


    }
}
