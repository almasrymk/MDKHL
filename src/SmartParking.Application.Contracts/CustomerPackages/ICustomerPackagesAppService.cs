using SmartParking.CustomerPackages.Dtos;
using SmartParking.CustomerPackages.Inputs;
using SmartParking.Packages;
using SmartParking.Payments;
using SmartParking.Results;
using System;
using System.Collections.Generic;
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
        /// Retrieves a list of all packages currently assigned to the authenticated customer.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. 
        /// The task result contains a <see cref="Result{T}"/> with a list of <see cref="CustomerPackageDto"/> assigned to the customer.
        /// </returns>
        Task<Result<List<CustomerPackageDto>>> GetPackageByCustomer();

        Task AddDefaultPackageToCustomerAsync(DefaultCustomerPackageDto defaultCustomerPackageDto);

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
        Task<CustomerPackageDto> GetLatestByCustomerIdAndEnPlateNumber(SearchByPlateAndCustomerIdDto searchByPlateAndCustomerIdDto);

       
        new Task DeleteAsync(int id);
        /// <summary>
        /// Asynchronously deletes the vehicle record associated with the specified customer and license plate number.
        /// </summary>
        /// <param name="customerId">The unique identifier of the customer whose vehicle record is to be deleted.</param>
        /// <param name="enPlateNumber">The license plate number of the vehicle to delete. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteAsync(Guid customerId, string enPlateNumber);

    }
}
