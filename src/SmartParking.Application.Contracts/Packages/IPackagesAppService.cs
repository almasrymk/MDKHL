using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartParking.Packages
{
	public interface IPackagesAppService
    {
        /// <summary>
        /// Retrieves a list of packages associated with a specified organization ID.
        /// </summary>
        /// <param name="organizationId">The ID of the organization whose packages are to be retrieved.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains a list of <see cref="PackageDto"/> objects associated with the organization.</returns>
        Task<List<PackageDto>> GetListPackageByOrganizationId(int organizationId);
		/// <summary>
		/// abstract method to create Package
		/// </summary>
		/// <param name="input">Create Package dto</param>
		/// <returns>Package Dto</returns>
		Task<PackageDto> CreateAsync(CreateUpdatePackageDto input);

		/// <summary>
		/// abstract method to get list of Packages
		/// </summary>
		/// <param name="input">specifies max number of Packages and if it sorted or not</param>
		/// <returns>Package Dto</returns>
		Task<PagedResultDto<PackageDto>> GetListAsync(PackageListDto input);

		/// <summary>
		/// abstract method to get Package by id
		/// </summary>
		/// <param name="id">id of Package</param>
		/// <returns>Package Dto</returns>
		Task<PackageDto> GetAsync(int id);

		/// <summary>
		/// abstract method to get Packages by OrgnizationId
		/// </summary>
		/// <param name="OrgnizationId">id of DefaultPackage</param>
		/// <param name="input">specifies max number of Packages and if it sorted or not</param>
		/// 
		/// <returns>Package Dto</returns>
		Task<PagedResultDto<PackageDto>> GetbyOrganizationAsync(PackageListDto input , int OrgnizationId);

		/// <summary>
		/// abstract method used to update Package
		/// </summary>
		/// <param name="id">Package id</param>
		/// <param name="input">updated Package dto</param>
		/// <returns>updated Package dto</returns>
		/// 
		Task<PackageDto> UpdateAsync(int id, CreateUpdatePackageDto input);

		///// <summary>
		///// abstract method to delete DefaultPackage
		///// </summary>
		///// <param name="id">id of DefaultPackage</param>
		///// <returns>successfull message of deletion</returns>
		

	}
}
