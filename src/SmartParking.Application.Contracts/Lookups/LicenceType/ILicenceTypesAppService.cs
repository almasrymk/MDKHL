using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.LicenceType
{
	public interface ILicenceTypesAppService : ICrudAppService<LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>
	{
        public Task<PagedResultDto<LicenceTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

        /// <summary>
        /// abstract method to filter of cameras
        /// </summary>
        /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
        /// <returns>Camera Dto</returns>
        public Task<PagedResultDto<LicenceTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);
    }
}
