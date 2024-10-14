
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.VehicleModel
{
	public interface IVehicleModelsAppService : ICrudAppService<VehicleModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleModelDto>
	{

        /// <summary>
        /// abstract method to create vehicle model
        /// </summary>
        /// <param name="input">vehicle model create dto</param>
        /// <returns>VehicleModelDto</returns>
        //Task<VehicleModelDto> CreateAsync(CreateUpdateVehicleModelDto input);

        /// <summary>
        /// abstract method to update specific vehicle model
        /// </summary>
        /// <param name="input">vehicle model create dto</param>
        /// <param name="id">id of specific vehicle model</param>
        /// <returns>VehicleModelDto</returns>

        //Task<VehicleModelDto> UpdateAsync(int id, CreateUpdateVehicleModelDto input);

        /// <summary>
        /// abstract method to delete specific vehicle model
        /// </summary>
        /// <param name="input">id of specific vehicle model</param>

        public Task DeleteAsync(int id);
        public Task<PagedResultDto<VehicleModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// abstract method to filter of cameras
        /// </summary>
        /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
        /// <returns>Camera Dto</returns>
        public Task<PagedResultDto<VehicleModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    }
}
