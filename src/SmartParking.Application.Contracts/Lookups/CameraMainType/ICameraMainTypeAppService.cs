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
    /// Abstract method to create a new camera main type.
    /// </summary>
    /// <param name="input">Camera main type creation DTO.</param>
    /// <returns>The created <see cref="CameraMainTypeDto"/>.</returns>
    public Task<CameraMainTypeDto> CreateAsync(CreateUpdateCameraMainTypeDto input);

    /// <summary>
    /// Abstract method to update an existing camera main type by ID.
    /// </summary>
    /// <param name="id">The ID of the camera main type to update.</param>
    /// <param name="input">Camera main type update DTO.</param>
    /// <returns>The updated <see cref="CameraMainTypeDto"/>.</returns>
    public Task<CameraMainTypeDto> UpdateAsync(int id, CreateUpdateCameraMainTypeDto input);

    /// <summary>
    /// Abstract method to search camera main types using pagination, sorting, and a search query.
    /// </summary>
    /// <param name="input">DTO containing pagination, sorting, and search parameters.</param>
    /// <returns>A paged result of <see cref="CameraMainTypeDto"/>.</returns>
    public Task<PagedResultDto<CameraMainTypeDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter camera main types based on provided criteria such as filters, sorting, and pagination.
    /// </summary>
    /// <param name="input">DTO specifying filtering, sorting, and pagination parameters.</param>
    /// <returns>A paged result of <see cref="CameraMainTypeDto"/> that matches the filter criteria.</returns>
    public Task<PagedResultDto<CameraMainTypeDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to delete a specific camera main type by ID.
    /// </summary>
    /// <param name="id">The ID of the camera main type to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(int id);

    /// <summary>
    /// Abstract method to retrieve all camera main types without pagination or filtering.
    /// </summary>
    /// <returns>A list of all <see cref="CameraMainTypeDto"/>.</returns>
    public Task<List<CameraMainTypeDto>> GetAllCameraMainTypesAsync();

}
