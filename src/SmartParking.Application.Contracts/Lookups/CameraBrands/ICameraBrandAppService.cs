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
    /// Abstract method to create a new camera brand.
    /// </summary>
    /// <param name="input">Camera brand creation DTO.</param>
    /// <returns>The created <see cref="CameraBrandDto"/>.</returns>
    public Task<CameraBrandDto> CreateAsync(CreateUpdateCameraBrandDto input);

    /// <summary>
    /// Abstract method to update an existing camera brand by ID.
    /// </summary>
    /// <param name="id">The ID of the camera brand to update.</param>
    /// <param name="input">Camera brand update DTO.</param>
    /// <returns>The updated <see cref="CameraBrandDto"/>.</returns>
    public Task<CameraBrandDto> UpdateAsync(int id, CreateUpdateCameraBrandDto input);

    /// <summary>
    /// Abstract method to search camera brands using pagination, sorting, and a search query.
    /// </summary>
    /// <param name="input">DTO containing pagination, sorting, and search parameters.</param>
    /// <returns>A paged result of <see cref="CameraBrandDto"/>.</returns>
    public Task<PagedResultDto<CameraBrandDto>> Search(PagedAndSortedAndSearchResultRequestDto input);

    /// <summary>
    /// Abstract method to filter camera brands based on provided criteria such as filters, sorting, and pagination.
    /// </summary>
    /// <param name="input">DTO specifying filtering, sorting, and pagination parameters.</param>
    /// <returns>A paged result of <see cref="CameraBrandDto"/> that matches the filter criteria.</returns>
    public Task<PagedResultDto<CameraBrandDto>> Filter(PagedAndSortedAndFilteredResultRequestDto input);

    /// <summary>
    /// Abstract method to delete a specific camera brand by ID.
    /// </summary>
    /// <param name="id">The ID of the camera brand to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task DeleteAsync(int id);

    /// <summary>
    /// Abstract method to retrieve all camera brands without pagination or filtering.
    /// </summary>
    /// <returns>A list of all <see cref="CameraBrandDto"/>.</returns>
    public Task<List<CameraBrandDto>> GetAllCameraBrandsAsync();

}
