using SmartParking.Lookups.CameraModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Lookups.CameraModel;

public interface ICameraModelAppService : ICrudAppService<CameraModelDto, int, PagedAndSortedResultRequestDto, CreateUpdateCameraModelDto>
{

    /// <summary>
    /// abstract method to create camera model
    /// </summary>
    /// <param name="input">camera model create dto</param>
    /// <returns>camera modelDto</returns>
    public Task<CameraModelDto> CreateAsync(CreateUpdateCameraModelDto input);

    /// <summary>
    /// abstract method to update specific camera model
    /// </summary>
    /// <param name="input">camera model create dto</param>
    /// <param name="id">id of specific camera model</param>
    /// <returns>camera modelDto</returns>
    public Task<CameraModelDto> UpdateAsync(int id, CreateUpdateCameraModelDto input);
    public Task<PagedResultDto<CameraModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of cameras
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
    /// <returns>Camera Dto</returns>
    public Task<PagedResultDto<CameraModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    public Task DeleteAsync(int id);

    Task<List<CameraModelDto>> GetAllCameraModelsAsync();
}
