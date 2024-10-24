
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
    }
}
