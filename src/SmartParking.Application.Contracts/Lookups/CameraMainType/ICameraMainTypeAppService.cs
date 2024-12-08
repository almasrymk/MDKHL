using SmartParking.Lookups.CameraMainType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.CameraMainType;

public interface ICameraMainTypeAppService : ICrudAppService<CameraMainTypeDto, int, PagedAndSortedResultRequestDto, CreateUpdateCameraMainTypeDto>
{

    /// <summary>
    /// abstract method to create camera maintype
    /// </summary>
    /// <param name="input">camera maintype create dto</param>
    /// <returns>camera maintypeDto</returns>
    public Task<CameraMainTypeDto> CreateAsync(CreateUpdateCameraMainTypeDto input);

    /// <summary>
    /// abstract method to update specific camera maintype
    /// </summary>
    /// <param name="input">camera maintype create dto</param>
    /// <param name="id">id of specific camera maintype</param>
    /// <returns>camera maintypeDto</returns>
    public Task<CameraMainTypeDto> UpdateAsync(int id, CreateUpdateCameraMainTypeDto input);
    public Task<PagedResultDto<CameraMainTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of cameras
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
    /// <returns>Camera Dto</returns>
    public Task<PagedResultDto<CameraMainTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    public Task DeleteAsync(int id);

    Task<List<CameraMainTypeDto>> GetAllCameraMainTypesAsync();
}
