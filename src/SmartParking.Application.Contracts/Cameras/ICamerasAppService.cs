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

        Task<CameraDto> CreateAsync(CreateUpdateCameraDto input);

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


    /// <summary>
    /// Method used to generate camera's Code.
    /// It contains of first 3 characters from the organization it belongs to.
    /// First 2 characters from the garage it belongs to.
    /// First 2 characters from camera's name.
    /// 2 Numeric numbers
    /// </summary>
    /// <param name="OrgEnglishName">OrganizationEnglishName</param>
    /// <param name="GarageEnglishName">GarageEnglishName</param>
    /// <param name="CameraEnglishName">CameraEnglishName</param>
    /// <returns></returns>
    Task<string> GenerateCameraCode(string OrgEnglishName, string GarageEnglishName, string CameraEnglishName);
    /// <summary>
    /// method used to get list of cameraes for MDKHL.
    /// </summary>
    /// <param name="input">specifies max number of camera and if it sorted or not</param>
    /// <returns>cameraDto List</returns>
    Task<PagedResultDto<CameraDto>> SearchForMDKHL(PagedAndSortedAndSearchResultRequestDto input);
    /// <summary>
    /// method used to get list of cameraes for MDKHL
    /// </summary>
    /// <param name="input">specifies max number of camera and if it filtered or not</param>
    /// <returns>cameraeDto List</returns>
    Task<PagedResultDto<CameraDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);

}
