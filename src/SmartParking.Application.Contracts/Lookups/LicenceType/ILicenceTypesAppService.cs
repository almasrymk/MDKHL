using SmartParking.Lookups.PlateColor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.LicenceType;

	public interface ILicenceTypesAppService : ICrudAppService<LicenceTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateLicenceTypeDto>
	{


        /// <summary>
        /// abstract method to create licence type 
        /// </summary>
        /// <param name="input">licence type  create dto</param>
        /// <returns>licence type Dto</returns>
        public Task<LicenceTypeDto> CreateAsync(CreateUpdateLicenceTypeDto input);

        /// <summary>
        /// abstract method to update specific licence type 
        /// </summary>
        /// <param name="input">licence type  create dto</param>
        /// <param name="id">id of specific licence type </param>
        /// <returns>licence type Dto</returns>
        public Task<LicenceTypeDto> UpdateAsync(int id, CreateUpdateLicenceTypeDto input);
        public Task<PagedResultDto<LicenceTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of cameras
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
    /// <returns>Camera Dto</returns>
    public Task<PagedResultDto<LicenceTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    public Task DeleteAsync(int id);

    public Task<List<LicenceTypeDto>> GetAllLicencesAsync();
}
