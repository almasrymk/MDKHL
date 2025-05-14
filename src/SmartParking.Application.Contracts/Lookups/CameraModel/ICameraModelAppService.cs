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
    /// Abstract method to create a new camera model.
    /// </summary>
    /// <param name="input">Camera model creation DTO.</param>
    /// <returns>The created <see cref="CameraModelDto"/>.</returns>
    public Task<CameraModelDto> CreateAsync(CreateUpdateCameraModelDto input);

    /// <summary>
    /// Abstract method to update an existing camera model by ID.
    /// </summary>
    /// <param name="id">The ID of the camera model to update.</param>
    /// <param name="input">Camera model update DTO.</param>
    /// <returns>The updated <see cref="CameraModelDto"/>.</returns>
    public Task<CameraModelDto> UpdateAsync(int id, CreateUpdateCameraModelDto input);

    /// <summary>
    /// Abstract method to search camera models using pagination, sorting, and a search query.
    /// </summary>
    /// <param name="input">DTO containing pagination, sorting, and search parameters.</param>
    /// <returns>A paged result of <see cref="CameraModelDto"/>.</returns>
    public Task<PagedResultDto<CameraModelDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter camera models based on provided criteria such as filters, sorting, and pagination.
    /// </summary>
    /// <param name="input">DTO specifying filtering, sorting, and pagination parameters.</param>
    /// <returns>A paged result of <see cref="CameraModelDto"/> that matches the filter criteria.</returns>
    public Task<PagedResultDto<CameraModelDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to delete a specific camera model by ID.
    /// </summary>
    /// <param name="id">The ID of the camera model to delete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    public Task DeleteAsync(int id);

    /// <summary>
    /// Abstract method to retrieve all camera models without pagination or filtering.
    /// </summary>
    /// <returns>A list of all <see cref="CameraModelDto"/>.</returns>
    public Task<List<CameraModelDto>> GetAllCameraModelsAsync();

}
