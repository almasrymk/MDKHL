using SmartParking.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartParking.Cameras;

/// <summary>
/// Application service interface for managing Camera entities.
/// </summary>
public interface ICamerasAppService :
    ICrudAppService<
        CameraDto,
        int,
        PagedAndSortedResultRequestDto,
        CreateUpdateCameraDto
    >
{
    /// <summary>
    /// Creates a new camera.
    /// </summary>
    /// <param name="input">The camera data to create.</param>
    /// <returns>The created CameraDto.</returns>
    Task<CameraDto> CreateAsync(CreateUpdateCameraDto input);

    /// <summary>
    /// Retrieves a paginated and sorted list of cameras for the current organization.
    /// </summary>
    /// <param name="input">Search, sorting, and pagination information.</param>
    /// <returns>Paginated result of CameraDto.</returns>
    Task<PagedResultDto<CameraDto>> GetListOrganizationAsync(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Retrieves a paginated and sorted list of cameras for MDKHL with optional tenant context.
    /// </summary>
    /// <param name="input">Search, sorting, and pagination information.</param>
    /// <param name="tenantId">Optional tenant ID to scope the results.</param>
    /// <returns>Paginated result of CameraDto.</returns>
    Task<PagedResultDto<CameraDto>> GetListMdkhlAsync(PagedAndSortedAndSearchResultRequestDto input, Guid? tenantId);

    /// <summary>
    /// Updates an existing camera.
    /// </summary>
    /// <param name="id">The ID of the camera to update.</param>
    /// <param name="input">The updated camera data.</param>
    /// <returns>The updated CameraDto.</returns>
    Task<CameraDto> UpdateAsync(int id, CreateUpdateCameraDto input);

    /// <summary>
    /// Retrieves a camera by its ID.
    /// </summary>
    /// <param name="id">The ID of the camera.</param>
    /// <returns>The CameraDto if found.</returns>
    Task<CameraDto> GetAsync(int id);

    /// <summary>
    /// Deletes a camera by its ID.
    /// </summary>
    /// <param name="id">The ID of the camera to delete.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Searches for cameras based on a search term with pagination and sorting.
    /// </summary>
    /// <param name="input">Specifies the search term, maximum number of results, and sorting options.</param>
    /// <returns>Paginated result of CameraDto.</returns>
    Task<PagedResultDto<CameraDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Filters the list of cameras based on custom filter criteria.
    /// </summary>
    /// <param name="input">Specifies filtering criteria, sorting, and pagination.</param>
    /// <returns>Paginated result of CameraDto.</returns>
    Task<PagedResultDto<CameraDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Generates a unique camera code based on organization name, garage name, and camera name.
    /// The format includes:
    /// - First 3 characters of the organization name
    /// - First 2 characters of the garage name
    /// - First 2 characters of the camera name
    /// - 2 numeric digits
    /// </summary>
    /// <param name="OrgEnglishName">The English name of the organization.</param>
    /// <param name="GarageEnglishName">The English name of the garage.</param>
    /// <param name="CameraEnglishName">The English name of the camera.</param>
    /// <returns>The generated camera code string.</returns>
    Task<string> GenerateCameraCode(string OrgEnglishName, string GarageEnglishName, string CameraEnglishName);

    /// <summary>
    /// Filters the list of cameras for MDKHL using advanced filter criteria.
    /// </summary>
    /// <param name="input">Filter, sorting, and pagination information.</param>
    /// <returns>Paginated result of CameraDto.</returns>
    Task<PagedResultDto<CameraDto>> FilterForMDKHL(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Retrieves the camera's IP address and related information using its ID.
    /// </summary>
    /// <param name="cameraId">The ID of the camera.</param>
    /// <returns>CameraDto including the IP address.</returns>
    Task<CameraDto> GetCameraIPByCameraId(int cameraId);
}
