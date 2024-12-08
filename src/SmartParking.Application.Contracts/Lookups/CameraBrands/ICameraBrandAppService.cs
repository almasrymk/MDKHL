using SmartParking.Lookups.CameraBrands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.CameraBrands;

public interface ICameraBrandAppService : ICrudAppService<CameraBrandDto, int, PagedAndSortedResultRequestDto, CreateUpdateCameraBrandDto>
{

    /// <summary>
    /// abstract method to create camera brand
    /// </summary>
    /// <param name="input">camera brand create dto</param>
    /// <returns>camera brandDto</returns>
    public Task<CameraBrandDto> CreateAsync(CreateUpdateCameraBrandDto input);

    /// <summary>
    /// abstract method to update specific camera brand
    /// </summary>
    /// <param name="input">camera brand create dto</param>
    /// <param name="id">id of specific camera brand</param>
    /// <returns>camera brandDto</returns>
    public Task<CameraBrandDto> UpdateAsync(int id, CreateUpdateCameraBrandDto input);
    public Task<PagedResultDto<CameraBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of cameras
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
    /// <returns>Camera Dto</returns>
    public Task<PagedResultDto<CameraBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    public Task DeleteAsync(int id);

    Task<List<CameraBrandDto>> GetAllCameraBrandsAsync();
}
