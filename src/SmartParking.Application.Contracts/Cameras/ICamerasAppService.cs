using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cameras;

public interface ICamerasAppService:
     ICrudAppService<
        CameraDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCameraDto
        >
{
        Task<PagedResultDto<CameraDto>> GetListAsync(PagedAndSortedResultRequestDto input);
        Task<CameraDto> UpdateAsync(int id, CreateUpdateCameraDto input);

        Task<CameraDto> GetAsync(int id);
        Task DeleteAsync(int id);

    /// <summary>
    /// abstract method to search of camera
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not and search term</param>
    /// <returns>Camera Dto</returns>
    public Task<PagedResultDto<CameraDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// abstract method to filter of cameras
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not and filter</param>
    /// <returns>Camera Dto</returns>
    public Task<PagedResultDto<CameraDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

}
