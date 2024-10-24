using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;


namespace SmartParking.Lookups.PlateColor;

    public interface IPlateColorsAppService : ICrudAppService<PlateColorDto, int, PagedAndSortedResultRequestDto, CreateUpdatePlateColorDto>
    {
        /// <summary>
        /// abstract method to create plate colot
        /// </summary>
        /// <param name="input">plate color create dto</param>
        /// <returns>plate colorDto</returns>
        public Task<PlateColorDto> CreateAsync(CreateUpdatePlateColorDto input);

        /// <summary>
        /// abstract method to update specific plate color
        /// </summary>
        /// <param name="input">plate color create dto</param>
        /// <param name="id">id of specific plate color</param>
        /// <returns>plate colorDto</returns>
        public Task<PlateColorDto> UpdateAsync(int id, CreateUpdatePlateColorDto input);
        public Task<PagedResultDto<PlateColorDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

	/// <summary>
	/// abstract method to filter of cameras
	/// </summary>
	/// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
	/// <returns>Camera Dto</returns>
	public Task<PagedResultDto<PlateColorDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

	public  Task DeleteAsync(int id);
}
