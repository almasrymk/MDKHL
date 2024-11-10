
using SmartParking.Results;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
namespace SmartParking.Lookups.VehicleBrand
{
	public interface IVehicleBrandsAppService : ICrudAppService<VehicleBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleBrandDto>
	{



        /// <summary>
        /// abstract method to create vehicle brand
        /// </summary>
        /// <param name="input">vehicle brand create dto</param>
        /// <returns>vehicle brandDto</returns>
        public Task<VehicleBrandDto> CreateAsync(CreateUpdateVehicleBrandDto input);

        /// <summary>
        /// abstract method to update specific vehicle brand
        /// </summary>
        /// <param name="input">vehicle brand create dto</param>
        /// <param name="id">id of specific vehicle brand</param>
        /// <returns>vehicle brandDto</returns>
        public Task<VehicleBrandDto> UpdateAsync(int id, CreateUpdateVehicleBrandDto input);
        public Task<PagedResultDto<VehicleBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// abstract method to filter of cameras
        /// </summary>
        /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
        /// <returns>Camera Dto</returns>
        public Task<PagedResultDto<VehicleBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

        public  Task DeleteAsync(int id);

        /// <summary>
        /// Asynchronously retrieves a list of vehicle brand data transfer objects.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation. 
        /// The task result contains a list of <see cref="VehicleBrandDto"/> objects representing the vehicle brands.
        /// </returns>
        Task<Result<List<VehicleBrandDto>>> GetbrandsAsync();

        Task<List<VehicleBrandDto>> GetAllAsync();    
    }
}
