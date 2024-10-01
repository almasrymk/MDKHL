using SmartParking.Lookups.LicenceType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.VehicleModel
{
	public interface IVehicleModelsAppService : ICrudAppService<VehicleModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateVehicleModelDto>
	{
        public Task<PagedResultDto<VehicleModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// abstract method to filter of cameras
        /// </summary>
        /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
        /// <returns>Camera Dto</returns>
        public Task<PagedResultDto<VehicleModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    }
}
